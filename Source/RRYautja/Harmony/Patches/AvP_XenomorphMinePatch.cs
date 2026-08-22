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
    /// Uses a static set to track Mineable things being mined by Xenomorphs.
    /// </summary>
    [StaticConstructorOnStartup]
    static class AvP_XenomorphMinePatch
    {
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

                // Patch Mineable.Notify_DestroyedMineable to clean up tracking
                var mineDestroy = AccessTools.Method(typeof(Mineable), "Notify_DestroyedMineable");
                if (mineDestroy != null)
                {
                    harmony.Patch(mineDestroy, postfix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(NotifyDestroyedPostfix)));
                }

                // Patch JobDriver_Mine.OnStart to track Xenomorph mining targets
                // This runs when the pawn starts the mine job, before YieldComponents is called
                var mineJobStart = AccessTools.Method(typeof(JobDriver_Mine), "OnStart");
                if (mineJobStart != null)
                {
                    harmony.Patch(mineJobStart, postfix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(OnStartPostfix)));
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

        public static void OnStartPostfix(JobDriver_Mine __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn != null && pawn.isXenomorph())
            {
                Thing target = __instance.job.targetA.Thing;
                if (target != null && target is Mineable)
                {
                    xenomorphMiningTargets.Add(target);
                }
            }
        }
    }
}