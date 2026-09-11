using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace RRYautja
{
    /// <summary>
    /// Randomizes filth textures for Xenomorph blood and slime.
    /// Graphic_Single only uses one texture, so we patch the Graphic
    /// property on Filth to return a random variant from our 16 textures.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class FilthTextureRandomizer
    {
        private static Graphic[] bloodGraphics;
        private static Graphic[] slimeGraphics;
        private static bool initialized = false;

        static FilthTextureRandomizer()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.filthrandomizer");
                // Patch Filth.Graphic getter to return a random variant
                var graphicProp = AccessTools.Property(typeof(Filth), "Graphic");
                if (graphicProp != null && graphicProp.GetMethod != null)
                {
                    harmony.Patch(graphicProp.GetMethod, postfix: new HarmonyMethod(typeof(FilthTextureRandomizer), nameof(GraphicFor_Postfix)));
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] Could not find Filth.Graphic property — filth randomizer not applied");
                }
                // Defer texture loading until after all mod content is loaded
                LongEventHandler.ExecuteWhenFinished(() => InitGraphics());
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
                
                // Check if texture exists
                if (ContentFinder<Texture2D>.Get(bloodPath, false) != null)
                {
                    bloodList.Add(GraphicDatabase.Get<Graphic_Single>(bloodPath, ShaderDatabase.Cutout, Vector2.one, bloodColor));
                }
                if (ContentFinder<Texture2D>.Get(slimePath, false) != null)
                {
                    slimeList.Add(GraphicDatabase.Get<Graphic_Single>(slimePath, ShaderDatabase.Cutout, Vector2.one, slimeColor));
                }
            }

            bloodGraphics = bloodList.ToArray();
            slimeGraphics = slimeList.ToArray();
            Log.Message($"[AVP Xenomorphs] Filth randomizer: {bloodGraphics.Length} blood, {slimeGraphics.Length} slime variants loaded");
        }

        /// <summary>
        /// Before Filth.DrawAt, check if this is our blood/slime filth and
        /// swap the graphic to a random variant. We store the chosen graphic
        /// per-filth-instance using the thingIDNumber as a stable hash.
        /// </summary>
        static void GraphicFor_Postfix(Filth __instance, ref Graphic __result)
        {
            if (bloodGraphics == null || slimeGraphics == null) return;
            if (bloodGraphics.Length == 0 && slimeGraphics.Length == 0) return;

            string defName = __instance.def?.defName;
            if (defName == null) return;

            Graphic[] pool = null;
            if (defName == "RRY_FilthBloodXenomorph" || defName == "RRY_FilthBloodXenomorph_Active" || defName == "RRY_FilthBloodNeomorph")
            {
                pool = bloodGraphics;
            }
            else if (defName == "RRY_Filth_Slime" || defName == "RRY_Xenomorph_Hive_Slime")
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