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
    /// Xenomorphs dissolve rock with resin instead of producing rubble.
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
                // Patch Mineable.YieldComponents to return empty when mined by Xenomorph
                var original = AccessTools.Method(typeof(Mineable), "YieldComponents");
                if (original != null)
                {
                    harmony.Patch(original, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(YieldComponentsPrefix)));
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to patch Mineable.YieldComponents: " + e.Message);
            }
        }

        /// <summary>
        /// Check if the thing being mined is being mined by a Xenomorph.
        /// If so, suppress all yield (no chunks, no rubble).
        /// </summary>
        public static bool YieldComponentsPrefix(Mineable __instance, ref IEnumerable<Thing> __result)
        {
            // Check if any Xenomorph pawn is currently mining this thing
            Map map = __instance.Map;
            if (map == null) return true;

            // Find any pawn with a Mine job targeting this thing
            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.CurJobDef == JobDefOf.Mine && pawn.jobs.curDriver != null)
                {
                    var job = pawn.CurJob;
                    if (job != null && job.targetA.Thing == __instance && pawn.isXenomorph())
                    {
                        // Suppress yield - return empty list
                        __result = new List<Thing>();
                        return false;
                    }
                }
            }

            return true;
        }
    }
}