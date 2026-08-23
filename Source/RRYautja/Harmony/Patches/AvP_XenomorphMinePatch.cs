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
                // Use our own harmony instance - AvPMod.harmony may not be ready yet
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.minepatch");

                // Patch Mineable.YieldComponents if it exists
                var yieldMethod = AccessTools.Method(typeof(Mineable), "YieldComponents");
                if (yieldMethod != null)
                {
                    harmony.Patch(yieldMethod, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(YieldComponentsPrefix)));
                    Log.Message("[AVP Xenomorphs] Patched Mineable.YieldComponents");
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] Mineable.YieldComponents not found, trying DestroyedMineable");
                }

                // Also try DestroyedMineable
                var destroyedMethod = AccessTools.Method(typeof(Mineable), "DestroyedMineable");
                if (destroyedMethod != null)
                {
                    harmony.Patch(destroyedMethod, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(DestroyedMineablePrefix)));
                    Log.Message("[AVP Xenomorphs] Patched Mineable.DestroyedMineable");
                }

                // Also patch the filth spawning directly - patch Mineable.StruckMineable
                var struckMethod = AccessTools.Method(typeof(Mineable), "StruckMineable");
                if (struckMethod != null)
                {
                    harmony.Patch(struckMethod, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(StruckMineablePrefix)));
                    Log.Message("[AVP Xenomorphs] Patched Mineable.StruckMineable");
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init mine patch: " + e.Message);
            }
        }

        private static bool IsBeingMinedByXenomorph(Mineable mineable)
        {
            Map map = mineable.Map;
            if (map == null) return false;

            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.isXenomorph()) continue;
                if (pawn.CurJobDef == JobDefOf.Mine)
                {
                    var target = pawn.CurJob.targetA.Thing;
                    if (target == mineable) return true;
                }
            }
            return false;
        }

        public static bool YieldComponentsPrefix(Mineable __instance, ref IEnumerable<Thing> __result)
        {
            if (IsBeingMinedByXenomorph(__instance))
            {
                __result = new List<Thing>();
                return false;
            }
            return true;
        }

        public static bool DestroyedMineablePrefix(Mineable __instance)
        {
            if (IsBeingMinedByXenomorph(__instance))
            {
                Log.Message("[AVP Xenomorphs] Xenomorph mined " + __instance.def.defName + " - suppressing chunks");
                return false; // Skip the DestroyedMineable method entirely
            }
            return true;
        }

        public static void StruckMineablePrefix(Mineable __instance)
        {
            // Just tracking - can't suppress here
        }
    }
}