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

                // TrySpawnYield is the method that spawns chunks when mining
                // Two overloads: TrySpawnYield(Map, float, bool, Pawn) and TrySpawnYield(Map, bool, Pawn)
                var methods = typeof(Mineable).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var m in methods)
                {
                    if (m.Name == "TrySpawnYield")
                    {
                        harmony.Patch(m, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(TrySpawnYieldPrefix)));
                        Log.Message("[AVP Xenomorphs] Patched Mineable." + m.Name + "(" + string.Join(", ", System.Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name)) + ")");
                    }
                }

                // Also patch DestroyMined which is called when a mineable is destroyed by mining
                var destroyMined = AccessTools.Method(typeof(Mineable), "DestroyMined");
                if (destroyMined != null)
                {
                    harmony.Patch(destroyMined, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(DestroyMinedPrefix)));
                    Log.Message("[AVP Xenomorphs] Patched Mineable.DestroyMined");
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init mine patch: " + e.Message);
            }
        }

        /// <summary>
        /// Suppress yield when a Xenomorph mines.
        /// The Pawn parameter is the last parameter in both overloads.
        /// </summary>
        public static bool TrySpawnYieldPrefix(Mineable __instance, ref bool __result, object[] __args)
        {
            // Find the Pawn argument (last parameter in both overloads)
            Pawn miner = null;
            foreach (var arg in __args)
            {
                if (arg is Pawn p) { miner = p; break; }
            }

            if (miner != null && miner.isXenomorph())
            {
                __result = false;
                return false; // Skip TrySpawnYield entirely - no chunks
            }
            return true;
        }

        /// <summary>
        /// DestroyMined is called when a mineable is fully mined.
        /// It calls TrySpawnYield internally, but we patch both for safety.
        /// </summary>
        public static bool DestroyMinedPrefix(Mineable __instance, Pawn ___miner)
        {
            // Check if the miner pawn parameter is a Xenomorph
            // DestroyMined takes a Pawn parameter
            return true; // Don't skip - let it destroy, but TrySpawnYield will be suppressed
        }
    }
}