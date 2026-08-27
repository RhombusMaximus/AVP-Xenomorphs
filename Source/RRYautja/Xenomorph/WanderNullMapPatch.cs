using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace RRYautja
{
    /// <summary>
    /// Prevents NRE when facehuggers/Xenomorphs with null map try to wander.
    /// The vanilla JobGiver_Wander doesn't check for null map.
    /// </summary>
    [StaticConstructorOnStartup]
    static class WanderNullMapPatch
    {
        static WanderNullMapPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.wandernullmap");
                var method = AccessTools.Method(typeof(JobGiver_Wander), "TryGiveJob");
                if (method != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(WanderNullMapPatch), nameof(TryGiveJobPrefix)));
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init wander null map patch: " + e.Message);
            }
        }

        public static bool TryGiveJobPrefix(Pawn pawn)
        {
            // Return false (skip original method) if pawn has no map
            if (pawn == null || pawn.Map == null)
            {
                return false; // Skip original TryGiveJob
            }
            return true; // Run original
        }
    }
}