using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace RRYautja
{
    /// <summary>
    /// Randomizes filth textures for Xenomorph blood and slime.
    /// Patches Thing.Graphic getter (base class) since Filth doesn't override it.
    /// Returns one of 16 texture variants per filth instance based on thingIDNumber.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class FilthTextureRandomizer
    {
        private static Graphic[] bloodGraphics;
        private static Graphic[] slimeGraphics;
        private static HashSet<string> bloodDefNames;
        private static HashSet<string> slimeDefNames;
        private static bool initialized = false;

        static FilthTextureRandomizer()
        {
            Log.Message("[AVP Xenomorphs] FilthTextureRandomizer static constructor running");
            try
            {
                // Load textures immediately — StaticConstructorOnStartup runs after defs are loaded
                InitGraphics();

                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.filthrandomizer");

                // Patch Thing.Graphic getter — Filth inherits from Thing and doesn't override Graphic
                var graphicProp = AccessTools.Property(typeof(Thing), "Graphic");
                if (graphicProp != null && graphicProp.GetMethod != null)
                {
                    harmony.Patch(graphicProp.GetMethod, postfix: new HarmonyMethod(typeof(FilthTextureRandomizer), nameof(GraphicFor_Postfix)));
                    Log.Message("[AVP Xenomorphs] Patched Thing.Graphic for filth texture randomization");
                }
                else
                {
                    // Fallback: try ThingWithComps
                    graphicProp = AccessTools.Property(typeof(ThingWithComps), "Graphic");
                    if (graphicProp != null && graphicProp.GetMethod != null)
                    {
                        harmony.Patch(graphicProp.GetMethod, postfix: new HarmonyMethod(typeof(FilthTextureRandomizer), nameof(GraphicFor_Postfix)));
                        Log.Message("[AVP Xenomorphs] Patched ThingWithComps.Graphic for filth texture randomization");
                    }
                    else
                    {
                        Log.Warning("[AVP Xenomorphs] Could not find Thing.Graphic or ThingWithComps.Graphic property — filth randomizer not applied");
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init filth randomizer: " + e.Message);
            }
        }

        private static void InitGraphics()
        {
            if (initialized) return;
            initialized = true;

            string[] suffixes = { "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p" };
            Color bloodColor = new Color(200f/255f, 180f/255f, 40f/255f, 200f/255f);
            Color slimeColor = new Color(200f/255f, 180f/255f, 40f/255f, 200f/255f);

            var bloodList = new List<Graphic>();
            var slimeList = new List<Graphic>();

            foreach (var s in suffixes)
            {
                string bloodPath = $"Things/Filth/XenomorphBlood_{s}";
                string slimePath = $"Things/Filth/XenomorphSlime_{s}";

                // Load directly via GraphicDatabase — no ContentFinder check needed
                // GraphicDatabase handles missing textures gracefully
                Graphic bg = GraphicDatabase.Get<Graphic_Single>(bloodPath, ShaderDatabase.Cutout, Vector2.one, bloodColor);
                if (bg != null) bloodList.Add(bg);

                Graphic sg = GraphicDatabase.Get<Graphic_Single>(slimePath, ShaderDatabase.Cutout, Vector2.one, slimeColor);
                if (sg != null) slimeList.Add(sg);
            }

            bloodGraphics = bloodList.ToArray();
            slimeGraphics = slimeList.ToArray();

            // Build defName lookup sets for fast filtering
            bloodDefNames = new HashSet<string> { "RRY_FilthBloodXenomorph", "RRY_FilthBloodXenomorph_Active", "RRY_FilthBloodNeomorph" };
            slimeDefNames = new HashSet<string> { "RRY_Filth_Slime", "RRY_Xenomorph_Hive_Slime" };

            Log.Message($"[AVP Xenomorphs] Filth randomizer: {bloodGraphics.Length} blood, {slimeGraphics.Length} slime variants loaded");
        }

        /// <summary>
        /// Postfix on Thing.Graphic getter.
        /// Replaces the returned graphic for our filth defs with a random variant.
        /// Fast early-return for non-filth Things to avoid performance impact.
        /// </summary>
        static void GraphicFor_Postfix(Thing __instance, ref Graphic __result)
        {
            // Fast guard: skip everything that isn't filth
            if (!(__instance is Filth)) return;
            if (bloodGraphics == null || slimeGraphics == null) return;
            if (__result == null) return;

            string defName = __instance.def?.defName;
            if (defName == null) return;

            Graphic[] pool = null;
            if (bloodDefNames.Contains(defName))
            {
                pool = bloodGraphics;
            }
            else if (slimeDefNames.Contains(defName))
            {
                pool = slimeGraphics;
            }

            if (pool == null || pool.Length == 0) return;

            // Use thingIDNumber for stable per-instance selection
            int index = Mathf.Abs(__instance.thingIDNumber) % pool.Length;
            __result = pool[index];
        }
    }
}