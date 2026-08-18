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
        // Track which Mineable things are being mined by Xenomorphs
        public static HashSet<Thing> xenomorphMiningTargets = new HashSet<Thing>();

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

                // Patch JobDriver_Mine.OnDestroy to track Xenomorph mining targets
                var mineDestroy = AccessTools.Method(typeof(Mineable), "Notify_DestroyedMineable");
                if (mineDestroy != null)
                {
                    harmony.Patch(mineDestroy, postfix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(NotifyDestroyedPostfix)));
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to patch mine: " + e.Message);
            }
        }

        public static bool YieldComponentsPrefix(Mineable __instance, ref IEnumerable<Thing> __result)
        {
            if (xenomorphMiningTargets.Contains(__instance))
            {
                __result = new List<Thing>();
                return false;
            }
            return true;
        }

        public static void NotifyDestroyedPostfix(Mineable __instance)
        {
            xenomorphMiningTargets.Remove(__instance);
        }
    }

    /// <summary>
    /// Patch JobDriver_Mine to track when Xenomorphs start mining a target.
    /// This runs at the start of the mine job, before YieldComponents is called.
    /// </summary>
    [HarmonyPatch(typeof(JobDriver_Mine))]
    [HarmonyPatch("MakeNewToils")]
    static class AvP_MineJobTrackerPatch
    {
        static void Postfix(JobDriver_Mine __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn != null && pawn.isXenomorph())
            {
                Thing target = __instance.job.targetA.Thing;
                if (target != null && target is Mineable)
                {
                    AvP_XenomorphMinePatch.xenomorphMiningTargets.Add(target);
                }
            }
        }
    }
}