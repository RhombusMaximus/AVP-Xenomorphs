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
            Scribe_Collections.Look<string, bool>(ref this.RaceKeyPairs, "RaceKeyPairs"/*, LookMode.Def, LookMode.Value, ref RaceKeyWorkingList, ref RaceValueWorkingList*/);
            /*
            Scribe_Collections.Look(ref this.RaceKeyWorkingList, "RaceKeyPairs", LookMode.Def, new object[0]);
            Scribe_Collections.Look(ref this.RaceValueWorkingList, "RaceKeyPairs", LookMode.Value);
            */
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
        }

        public override string SettingsCategory() => "Aliens Vs Predator";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float numa = inRect.width;
            float numa2 = 620f;

            Rect rect = new Rect(inRect.x, inRect.y + 50, numa, numa2);
            Widgets.Label(inRect.TopHalf().TopHalf().TopHalf().TopHalf().ContractedBy(4),
                "Restart before playing to ensure your changes take effect.");

            /*
            Widgets.CheckboxLabeled(inRect.TopHalf().TopHalf().BottomHalf().TopHalf().ContractedBy(4), "RRY_AllowXenomorphFaction".Translate(), ref settings.AllowXenomorphFaction);
            Widgets.CheckboxLabeled(inRect.TopHalf().TopHalf().BottomHalf().BottomHalf().LeftHalf().ContractedBy(4), "RRY_AllowHiddenInfections".Translate(), ref settings.AllowHiddenInfections);
            Widgets.CheckboxLabeled(inRect.TopHalf().TopHalf().BottomHalf().BottomHalf().RightHalf().ContractedBy(4), "RRY_AllowPredalienImpregnations".Translate(), ref settings.AllowPredalienImpregnations);


            this.settings.fachuggerRemovalFailureDeathChance = Widgets.HorizontalSlider(inRect.TopHalf().BottomHalf().TopHalf().TopHalf().ContractedBy(4),
                this.settings.fachuggerRemovalFailureDeathChance, 0f, 1f, true,
                "RRY_FacehuggerRemovalDeathChance".Translate(this.settings.fachuggerRemovalFailureDeathChance * 100)
                , "0%", "100%");

            this.settings.embryoRemovalFailureDeathChance = Widgets.HorizontalSlider(inRect.TopHalf().BottomHalf().TopHalf().BottomHalf().ContractedBy(4),
                this.settings.embryoRemovalFailureDeathChance, 0f, 1f, true,
                "RRY_EmbryoRemovalDeathChance".Translate(this.settings.embryoRemovalFailureDeathChance * 100)
                , "0%", "100%");

            //    Widgets.BeginScrollView(inRect.BottomHalf().BottomHalf().BottomHalf().LeftHalf().ContractedBy(4), ref );
            */
            Rect rectShowXenoOptions = new Rect(rect.x, rect.y + 10, numa, 160f);
            Widgets.CheckboxLabeled(rectShowXenoOptions.TopHalf().TopHalf().LeftHalf().ContractedBy(4), "RRY_AllowXenomorphFaction".Translate(), ref settings.AllowXenomorphFaction);
            Widgets.CheckboxLabeled(rectShowXenoOptions.TopHalf().BottomHalf().LeftHalf().ContractedBy(4), "RRY_AllowHiddenInfections".Translate(), ref settings.AllowHiddenInfections);
            Widgets.CheckboxLabeled(rectShowXenoOptions.TopHalf().TopHalf().RightHalf().ContractedBy(4), "RRY_AllowPredalienImpregnations".Translate(), ref settings.AllowPredalienImpregnations);

            Widgets.CheckboxLabeled(rectShowXenoOptions.TopHalf().BottomHalf().RightHalf().ContractedBy(4), "RRY_AllowXenoCocoonMetamorph".Translate(), ref settings.AllowXenoCocoonMetamorph);
            Widgets.CheckboxLabeled(rectShowXenoOptions.BottomHalf().TopHalf().LeftHalf().ContractedBy(4), "RRY_AllowXenoEggMetamorph".Translate(), ref settings.AllowXenoEggMetamorph);
            Widgets.CheckboxLabeled(rectShowXenoOptions.BottomHalf().BottomHalf().LeftHalf().ContractedBy(4), "RRY_AllowNonHumanlikeHosts".Translate(), ref settings.AllowNonHumanlikeHosts);
            TextFieldNumericLabeled<float>(rectShowXenoOptions.BottomHalf().TopHalf().RightHalf().ContractedBy(4), "RRY_FacehuggerRemovalDeathChance".Translate(this.settings.fachuggerRemovalFailureDeathChance * 100), ref settings.fachuggerRemovalFailureDeathChance, ref settings.fachuggerRemovalFailureDeathChanceBuffer, 0f, 1f);

            TextFieldNumericLabeled<float>(rectShowXenoOptions.BottomHalf().BottomHalf().RightHalf().ContractedBy(4), "RRY_EmbryoRemovalDeathChance".Translate(this.settings.embryoRemovalFailureDeathChance * 100), ref settings.embryoRemovalFailureDeathChance, ref settings.embryoRemovalFailureDeathChanceBuffer, 0f, 1f);

            // Debug toggle (placed below xeno options, before host list)
            Rect debugRect = new Rect(rect.x, rectShowXenoOptions.yMax + 6, rect.width, 22f);
            Widgets.CheckboxLabeled(debugRect, "Debug Mode (extra logging)", ref settings.DebugMode);

            // Faction selectability toggle
            Rect factionRect = new Rect(rect.x, debugRect.yMax + 2, rect.width, 22f);
            Widgets.CheckboxLabeled(factionRect, "Faction Selectable (show on map, selectable at game start)", ref settings.FactionSelectable);
            if (settings.FactionSelectable)
            {
                Rect warnRect = new Rect(rect.x + 20, factionRect.yMax, rect.width - 20, 18f);
                GUI.color = Color.yellow;
                Widgets.Label(warnRect, "Requires new game to take effect.");
                GUI.color = Color.white;
            }


            float x = inRect.x;
            float num2 = inRect.y;
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
            int potentialhostcount = XenomorphHostSystem.AllRaces.Where(z => z.isPotentialHost(true)).Count();
            int enabledhostcount = XenomorphHostSystem.AllRaces.Where(z => z.isPotentialHost()).Count();
            int unsuitablehostcount = XenomorphHostSystem.AllRaces.Where(z => !z.isPotentialHost(true)).Count();
            Widgets.Label(inRect.TopHalf().BottomHalf().BottomHalf().BottomHalf().LeftHalf().LeftHalf().ContractedBy(4), "RRY_HostKinds".Translate(potentialhostcount, enabledhostcount));
            Rect hostRect = new Rect(inRect.x, inRect.y, inRect.BottomHalf().LeftHalf().LeftHalf().ContractedBy(4).width - 20, potentialhostcount * 20f);
            Widgets.BeginScrollView(inRect.BottomHalf().LeftHalf().LeftHalf().ContractedBy(4), ref this.pos, hostRect, true);
            foreach (ThingDef td in XenomorphHostSystem.AllRaces.Where(z => z.isPotentialHost(true)).OrderBy(xy => xy.label))
            {
                string text = td.LabelCap;
                /*
                text += " possible Xenoforms:";
                foreach (var item in pkd.resultingXenomorph())
                {
                    text += " "+item.LabelCap;
                }
                */
                settings.RaceKeyPairs.TryGetValue(td.defName, out bool setting);
                Widgets.CheckboxLabeled(new Rect(x, num2, hostRect.width, 20f), text, ref setting, (td == ThingDefOf.Human));
                settings.RaceKeyPairs.SetOrAdd(td.defName, setting || (td == ThingDefOf.Human));
                num2 += 20f;
            }
            Widgets.EndScrollView();
            
            float num3 = inRect.y;
            Widgets.Label(inRect.TopHalf().BottomHalf().BottomHalf().BottomHalf().LeftHalf().RightHalf().ContractedBy(4), "RRY_NonHostKinds".Translate(unsuitablehostcount));
            Rect nothostRect = new Rect(inRect.x, inRect.y, inRect.BottomHalf().LeftHalf().RightHalf().ContractedBy(4).width - 20, unsuitablehostcount * 40f);
            Widgets.BeginScrollView(inRect.BottomHalf().LeftHalf().RightHalf().ContractedBy(4), ref this.pos2, nothostRect, true);
            foreach (ThingDef td in XenomorphHostSystem.AllRaces.Where(z => !z.isPotentialHost(true)).OrderBy(xy => xy.label))
            {
                td.isPotentialHost(out string fr, true);
                string text = td.LabelCap + ":\n" + fr;
                Widgets.Label(new Rect(x, num3, nothostRect.width, 40f), text);
                num3 += 40f;
            }
            Widgets.EndScrollView();
            

            float width = 400f;
            float num4 = inRect.y;
            Widgets.Label(inRect.TopHalf().BottomHalf().BottomHalf().BottomHalf().RightHalf().ContractedBy(4), "RRY_XenomorphSpawningOptions".Translate());
            Widgets.BeginScrollView(inRect.BottomHalf().RightHalf().ContractedBy(4), ref this.pos3, new Rect(inRect.x, inRect.y, width, 2 * 22f), true);

            Widgets.CheckboxLabeled(new Rect(x, num4, width, 32f), "RRY_PredalienSpawning".Translate(), ref settings.AllowPredaliens);
            num4 += 22f;
            Widgets.CheckboxLabeled(new Rect(x, num4, width, 32f), "RRY_ThrumbomorphSpawning".Translate(), ref settings.AllowThrumbomorphs);
            num4 += 22f;
            Widgets.CheckboxLabeled(new Rect(x, num4, width, 32f), "RRY_NeomorphSpawning".Translate(), ref settings.AllowNeomorphs);
            num4 += 22f;
            Widgets.EndScrollView();



            /* 
        //    Widgets.CheckboxLabeled(inRect.TopHalf().TopHalf().BottomHalf().TopHalf().ContractedBy(4), "setting3: Desc", ref settings.setting3);
        //    Widgets.CheckboxLabeled(inRect.TopHalf().TopHalf().BottomHalf().BottomHalf().ContractedBy(4), "setting4: Desc", ref settings.setting4);

            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().TopHalf().TopHalf().ContractedBy(4), "setting5: Desc", ref settings.setting5);
            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().TopHalf().BottomHalf().ContractedBy(4), "setting6: Desc", ref settings.setting6);
            
            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().BottomHalf().TopHalf().ContractedBy(4), "setting7: Desc", ref settings.setting7);
            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().BottomHalf().BottomHalf().ContractedBy(4), "setting8: Desc", ref settings.setting8);
            */
            this.settings.Write();
        }

        // Token: 0x06005BBA RID: 23482 RVA: 0x0029DFF0 File Offset: 0x0029C3F0
        public static void TextFieldNumericLabeled<T>(Rect rect, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
        {
            Rect rect2 = rect.LeftPart(0.85f);
            Rect rect3 = rect.RightPart(0.10f);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect2, label);
            Text.Anchor = anchor;
            Widgets.TextFieldNumeric<T>(rect3, ref val, ref buffer, min, max);
        }

        public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (placeCheckboxNearText)
            {
                rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
            }
            Rect rect2 = rect.LeftPart(0.85f);
            Rect rect3 = rect.RightPart(0.10f);
            Widgets.Label(rect2, label);
            if (!disabled && Widgets.ButtonInvisible(rect, false))
            {
                checkOn = !checkOn;
                if (checkOn)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                }
                else
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                }
            }
            CheckboxDraw(rect.x + rect.width - 24f, rect.y, checkOn, disabled, 24f, null, null);
            Text.Anchor = anchor;
        }

        private static void CheckboxDraw(float x, float y, bool active, bool disabled, float size = 24f, Texture2D texChecked = null, Texture2D texUnchecked = null)
        {
            Color color = GUI.color;
            if (disabled)
            {
                GUI.color = InactiveColor;
            }
            Texture2D image;
            if (active)
            {
                image = ((!(texChecked != null)) ? Widgets.CheckboxOnTex : texChecked);
            }
            else
            {
                image = ((!(texUnchecked != null)) ? Widgets.CheckboxOffTex : texUnchecked);
            }
            Rect position = new Rect(x, y, size, size);
            GUI.DrawTexture(position, image);
            if (disabled)
            {
                GUI.color = color;
            }
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            if (!settings.AllowThrumbomorphs)
            {

            }
        }

        private static readonly Color InactiveColor = new Color(0.37f, 0.37f, 0.37f, 0.8f);
        private Vector2 pos = new Vector2(0f, 0f);
        private Vector2 pos2 = new Vector2(0f, 0f);
        private Vector2 pos3 = new Vector2(0f, 0f);

    }
    
}