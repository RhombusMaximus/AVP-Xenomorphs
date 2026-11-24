using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

using RimWorld;
using RRYautja.ExtensionMethods;
using RRYautja.Xenomorph;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RRYautja.settings
{
    static internal class SettingsHelper
    {
        public static AvPSettings latest;
    }

    class AvPSettings : ModSettings
    {
        public string fachuggerRemovalFailureDeathChanceBuffer;
        public string embryoRemovalFailureDeathChanceBuffer;
        public bool AllowXenoCocoonMetamorph = true;
        public bool AllowXenoEggMetamorph = true;
        public bool AllowNonHumanlikeHosts = true;
        public bool AllowThrumbomorphs = true;
        public bool AllowNeomorphs = true; 
        public bool AllowPredaliens = true;
        public bool AllowXenomorphFaction = true, AllowHiddenInfections = true, AllowPredalienImpregnations = true;
        public bool DebugMode = false;
        public bool FactionSelectable = false;
        public float fachuggerRemovalFailureDeathChance = 0.35f, embryoRemovalFailureDeathChance = 0.35f;

        public Dictionary<string, bool> RaceKeyPairs;
        
        public List<ThingDef> RaceKeyWorkingList = new List<ThingDef>();
        public List<bool> RaceValueWorkingList = new List<bool>();
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.AllowXenomorphFaction, "AllowXenomorphFaction", true);
            Scribe_Values.Look(ref this.DebugMode, "DebugMode", false);
            Scribe_Values.Look(ref this.FactionSelectable, "FactionSelectable", false);

            Scribe_Values.Look(ref this.AllowHiddenInfections, "AllowHiddenInfections", true);
            Scribe_Values.Look(ref this.AllowPredalienImpregnations, "AllowPredalienImpregnations", true);
            Scribe_Values.Look(ref this.AllowXenoCocoonMetamorph, "AllowXenoCocoonMetamorph", true);
            Scribe_Values.Look(ref this.AllowXenoEggMetamorph, "AllowXenoEggMetamorph", true);
            Scribe_Values.Look(ref this.AllowNonHumanlikeHosts, "AllowNonHumanlikeHosts", true);
            Scribe_Values.Look(ref this.AllowThrumbomorphs, "AllowThrumbomorphs", true);
            Scribe_Values.Look(ref this.AllowNeomorphs, "AllowNeomorphs", true);
            Scribe_Values.Look(ref this.AllowPredaliens, "AllowPredaliens", true);
            Scribe_Values.Look<float>(ref this.fachuggerRemovalFailureDeathChance, "fachuggerRemovalFailureDeathChance", 0.35f);
            Scribe_Values.Look<float>(ref this.embryoRemovalFailureDeathChance, "embryoRemovalFailureDeathChance", 0.35f);
            Scribe_Collections.Look<string, bool>(ref this.RaceKeyPairs, "RaceKeyPairs");
        }
    }

    class AvPMod : Mod
    {
        private AvPSettings settings;
        public static Harmony harmony;
        public AvPMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<AvPSettings>();
            SettingsHelper.latest = this.settings;
            harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (System.Exception e)
            {
                Log.Error("AVP Xenomorphs: Failed to apply some harmony patches (likely 1.6 API changes): " + e.Message);
            }
            if (Prefs.DevMode) Log.Message(string.Format("Alien Vs Predator: successfully completed {0} harmony patches.", harmony.GetPatchedMethods().Select(new Func<MethodBase, Patches>(Harmony.GetPatchInfo)).SelectMany((Patches p) => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count((Patch p) => p.owner.Contains(harmony.Id))));

            // Toggle faction visibility based on settings
            if (this.settings.FactionSelectable)
            {
                var xenoDef = DefDatabase<FactionDef>.GetNamed("RRY_Xenomorph", false);
                if (xenoDef != null)
                {
                    xenoDef.hidden = false;
                    Log.Message("[AVP Xenomorphs] Faction made selectable (hidden=false)");
                }
            }

            // Set stunFromEMP on all Xenomorph and Neomorph race defs (not XML-writable in 1.6)
            var stunField = typeof(ThingDef).GetField("stunFromEMP", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stunProp = typeof(ThingDef).GetProperty("stunFromEMP", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stunField != null || stunProp != null)
            {
                foreach (var def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.defName != null && (def.defName.StartsWith("RRY_Xenomorph") || def.defName.StartsWith("RRY_Neomorph")))
                    {
                        try { if (stunField != null) stunField.SetValue(def, true); } catch { }
                        try { if (stunProp != null && stunProp.CanWrite) stunProp.SetValue(def, true); } catch { }
                    }
                }
                if (stunField == null && stunProp == null)
                {
                    Log.Warning("[AVP Xenomorphs] stunFromEMP field/property not found — EMP stun will not work");
                }
            }

            // Set canOverlapZones=false on hive/tunnel buildings to prevent lag when placing zones over hive area
            var zoneField = typeof(ThingDef).GetField("canOverlapZones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (zoneField != null)
            {
                foreach (var def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.defName != null && (def.defName == "TunnelHiveLikeSpawner" || def.defName == "TunnelHiveLikeChildSpawner"
                        || def.defName == "RRY_Xenomorph_Hive" || def.defName == "RRY_Xenomorph_Hive_Child"
                        || def.defName == "RRY_Xenomorph_Hive_Wall" || def.defName == "RRY_Tunneler"))
                    {
                        try { zoneField.SetValue(def, false); } catch { }
                    }
                }
            }

            // Set Minifiable=true on egg ThingDef (not XML-writable in 1.6, it's a get-only property)
            // Minifiable is a property backed by a field — try setting via reflection
            var minifiableField = typeof(ThingDef).GetField("minifiable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (minifiableField == null)
            {
                // Try property setter
                var minifiableProp = typeof(ThingDef).GetProperty("Minifiable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (minifiableProp != null && minifiableProp.CanWrite)
                {
                    var eggDef = DefDatabase<ThingDef>.GetNamed("RRY_EggXenomorphFertilized", false);
                    if (eggDef != null) { try { minifiableProp.SetValue(eggDef, true); } catch { } }
                }
            }
            else
            {
                var eggDef = DefDatabase<ThingDef>.GetNamed("RRY_EggXenomorphFertilized", false);
                if (eggDef != null) { try { minifiableField.SetValue(eggDef, true); } catch { } }
            }
        }

        public override string SettingsCategory() => "AVP Xenomorphs";

        // Rewritten 11/2026 (2): single-column layout made the impregnable-host list
        // hundreds of rows long with 140+ mods installed. Trade-window style now:
        // toggles/sliders at the top; the two long lists live in fixed-height,
        // independently-scrollable side-by-side columns below.
        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (settings.RaceKeyPairs == null)
            {
                settings.RaceKeyPairs = new Dictionary<string, bool>();
                foreach (ThingDef t in XenomorphHostSystem.AllRaces)
                {
                    if (!settings.RaceKeyPairs.ContainsKey(t.defName))
                    {
                        settings.RaceKeyPairs.SetOrAdd(t.defName, t.isPotentialHost(true));
                    }
                }
            }

            List<ThingDef> potentialHosts = XenomorphHostSystem.AllRaces.Where(z => z.isPotentialHost(true)).OrderBy(xy => xy.label).ToList();
            List<ThingDef> nonHosts = XenomorphHostSystem.AllRaces.Where(z => !z.isPotentialHost(true)).OrderBy(xy => xy.label).ToList();
            int enabledhostcount = XenomorphHostSystem.AllRaces.Where(z => z.isPotentialHost()).Count();

            Rect top = inRect.TopPartPixels(Mathf.Min(inRect.height - 200f, 470f));
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(top);
            ls.ColumnWidth = top.width;

            ls.Label("Restart before playing to ensure your changes take effect.");
            ls.Gap(6f);

            ls.CheckboxLabeled("RRY_AllowXenomorphFaction".Translate(), ref settings.AllowXenomorphFaction, null);
            ls.CheckboxLabeled("RRY_AllowHiddenInfections".Translate(), ref settings.AllowHiddenInfections, null);
            ls.CheckboxLabeled("RRY_AllowPredalienImpregnations".Translate(), ref settings.AllowPredalienImpregnations, null);
            ls.CheckboxLabeled("RRY_AllowXenoCocoonMetamorph".Translate(), ref settings.AllowXenoCocoonMetamorph, null);
            ls.CheckboxLabeled("RRY_AllowXenoEggMetamorph".Translate(), ref settings.AllowXenoEggMetamorph, null);
            ls.CheckboxLabeled("RRY_AllowNonHumanlikeHosts".Translate(), ref settings.AllowNonHumanlikeHosts, null);
            ls.Gap(6f);

            Rect sliderRect = ls.GetRect(38f);
            settings.fachuggerRemovalFailureDeathChance = Widgets.HorizontalSlider(sliderRect,
                settings.fachuggerRemovalFailureDeathChance, 0f, 1f, true,
                "RRY_FacehuggerRemovalDeathChance".Translate(settings.fachuggerRemovalFailureDeathChance * 100),
                "0%", "100%");
            sliderRect = ls.GetRect(38f);
            settings.embryoRemovalFailureDeathChance = Widgets.HorizontalSlider(sliderRect,
                settings.embryoRemovalFailureDeathChance, 0f, 1f, true,
                "RRY_EmbryoRemovalDeathChance".Translate(settings.embryoRemovalFailureDeathChance * 100),
                "0%", "100%");
            ls.Gap(6f);

            ls.CheckboxLabeled("Debug Mode (extra logging)", ref settings.DebugMode, null);
            ls.CheckboxLabeled("Faction Selectable (WIP - not yet working)", ref settings.FactionSelectable, null);
            if (settings.FactionSelectable)
            {
                Color c = GUI.color;
                GUI.color = Color.yellow;
                Rect warn = ls.GetRect(22f);
                warn.xMin += 20f;
                Widgets.Label(warn, "Requires new game to take effect.");
                GUI.color = c;
            }
            ls.Gap(8f);

            ls.Label("RRY_XenomorphSpawningOptions".Translate());
            ls.CheckboxLabeled("RRY_PredalienSpawning".Translate(), ref settings.AllowPredaliens, null);
            ls.CheckboxLabeled("RRY_ThrumbomorphSpawning".Translate(), ref settings.AllowThrumbomorphs, null);
            ls.CheckboxLabeled("RRY_NeomorphSpawning".Translate(), ref settings.AllowNeomorphs, null);

            ls.End();

            // Trade-style fixed-height list columns
            Rect bottom = new Rect(inRect.x, top.yMax + 4f, inRect.width, inRect.yMax - (top.yMax + 4f));
            float headerH = 24f;
            Rect hostHeader = new Rect(bottom.x, bottom.y, bottom.width * 0.5f, headerH);
            Rect hostView = new Rect(hostHeader.x, hostHeader.yMax, hostHeader.width, bottom.height - headerH);
            Rect nonHostHeader = new Rect(bottom.x + bottom.width * 0.5f, bottom.y, bottom.width * 0.5f, headerH);
            Rect nonHostView = new Rect(nonHostHeader.x, nonHostHeader.yMax, nonHostHeader.width, bottom.height - headerH);

            Widgets.Label(hostHeader, "RRY_HostKinds".Translate(potentialHosts.Count, enabledhostcount));
            Widgets.Label(nonHostHeader, "RRY_NonHostKinds".Translate(nonHosts.Count));

            Rect hostContent = new Rect(0f, 0f, hostView.width - 24f, Mathf.Max(potentialHosts.Count * 24f, hostView.height));
            Widgets.BeginScrollView(hostView, ref this.pos, hostContent, false);
            float y = 0f;
            foreach (ThingDef td in potentialHosts)
            {
                settings.RaceKeyPairs.TryGetValue(td.defName, out bool setting);
                Rect r = new Rect(0f, y, hostContent.width, 24f);
                bool disabled = td == ThingDefOf.Human;
                Widgets.CheckboxLabeled(r, td.LabelCap, ref setting, disabled);
                settings.RaceKeyPairs.SetOrAdd(td.defName, setting || disabled);
                y += 24f;
            }
            Widgets.EndScrollView();

            Rect nonHostContent = new Rect(0f, 0f, nonHostView.width - 24f, Mathf.Max(nonHosts.Count * 44f, nonHostView.height));
            Widgets.BeginScrollView(nonHostView, ref this.pos2, nonHostContent, false);
            y = 0f;
            foreach (ThingDef td in nonHosts)
            {
                td.isPotentialHost(out string fr, true);
                Widgets.Label(new Rect(0f, y, nonHostContent.width, 44f), td.LabelCap + ":\n" + fr);
                y += 44f;
            }
            Widgets.EndScrollView();

            this.settings.Write();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            if (!settings.AllowThrumbomorphs)
            {

            }
        }

        private Vector2 pos = new Vector2(0f, 0f);

    }
    
}
