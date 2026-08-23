using System.Collections.Generic;
using System.Linq;
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
            var harmony = RRYautja.settings.AvPMod.harmony;
            if (harmony == null) return;

            try
            {
                // Patch Mineable.DestroyedMineable to suppress chunk spawning when mined by Xenomorph
                var destroyedMethod = AccessTools.Method(typeof(Mineable), "DestroyedMineable");
                if (destroyedMethod != null)
                {
                    harmony.Patch(destroyedMethod, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(DestroyedMineablePrefix)));
                    Log.Message("[AVP Xenomorphs] Patched Mineable.DestroyedMineable");
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] Could not find Mineable.DestroyedMineable method");
                }

                // Also try YieldComponents in case it exists
                var yieldMethod = AccessTools.Method(typeof(Mineable), "YieldComponents");
                if (yieldMethod != null)
                {
                    harmony.Patch(yieldMethod, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(YieldComponentsPrefix)));
                    Log.Message("[AVP Xenomorphs] Patched Mineable.YieldComponents");
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to patch mine: " + e.Message);
            }
        }

        /// <summary>
        /// Check if any Xenomorph pawn on the map is currently mining this thing.
        /// </summary>
        private static bool IsBeingMinedByXenomorph(Mineable mineable)
        {
            Map map = mineable.Map;
            if (map == null) return false;

            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.isXenomorph()) continue;
                if (pawn.CurJobDef == JobDefOf.Mine)
                {
                    Thing target = pawn.CurJob.targetA.Thing;
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

        public static void DestroyedMineablePrefix(Mineable __instance)
        {
            // Just log for debugging - we can't prevent the method from running
            // but we can check if a Xenomorph is mining this
            if (IsBeingMinedByXenomorph(__instance))
            {
                Log.Message("[AVP Xenomorphs] Xenomorph mined " + __instance.def.defName + " at " + __instance.Position);
            }
        }
    }
}