using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    /// <summary>
    /// When a Xenomorph mines rock, suppress chunk/filth spawning.
    /// </summary>
    [StaticConstructorOnStartup]
    static class AvP_XenomorphMinePatch
    {
        static AvP_XenomorphMinePatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.minepatch");

                // TrySpawnYield is void in 1.6, two overloads
                // Patch only the non-obsolete 3-parameter version: TrySpawnYield(Map, bool, Pawn)
                var methods = typeof(Mineable).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var m in methods)
                {
                    if (m.Name == "TrySpawnYield")
                    {
                        // Skip obsolete methods (the 4-param version with yieldChance)
                        if (m.GetParameters().Length == 4) continue;
                        harmony.Patch(m, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(TrySpawnYieldPrefix)));
                        AvPDebug.LogOnce("Patch", "[AVP Xenomorphs] Patched Mineable.TrySpawnYield");
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init mine patch: " + e.Message);
            }
        }

        /// <summary>
        /// Suppress yield when a Xenomorph mines.
        /// TrySpawnYield is void, so we just return false to skip it.
        /// The Pawn parameter is the miner.
        /// </summary>
        public static bool TrySpawnYieldPrefix(Mineable __instance, object[] __args)
        {
            // Fast exit — find Pawn argument quickly
            if (__args == null || __args.Length == 0) return true;
            // Last argument is usually the Pawn
            Pawn miner = __args[__args.Length - 1] as Pawn;
            if (miner == null)
            {
                // Fallback: scan all args
                foreach (var arg in __args)
                {
                    if (arg is Pawn p) { miner = p; break; }
                }
            }
            if (miner == null) return true;
            // Direct FleshType check — faster than isXenomorph() extension
            if (miner.RaceProps?.FleshType != XenomorphRacesDefOf.RRY_Xenomorph) return true;
            return false; // Skip TrySpawnYield entirely - no chunks
        }
    }
}