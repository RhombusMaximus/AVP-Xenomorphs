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
                var methods = typeof(Mineable).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var m in methods)
                {
                    if (m.Name == "TrySpawnYield")
                    {
                        harmony.Patch(m, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(TrySpawnYieldPrefix)));
                        Log.Message("[AVP Xenomorphs] Patched Mineable.TrySpawnYield");
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
            // Find the Pawn argument (last parameter in both overloads)
            Pawn miner = null;
            foreach (var arg in __args)
            {
                if (arg is Pawn p) { miner = p; break; }
            }

            if (miner != null && miner.isXenomorph())
            {
                return false; // Skip TrySpawnYield entirely - no chunks
            }
            return true;
        }
    }
}