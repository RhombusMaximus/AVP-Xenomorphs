using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace RRYautja
{
    /// <summary>
    /// Prevents NRE in JobDriver_Wait.CheckForAutoAttack when facehuggers
    /// have null map or other references. The Wait job's auto-attack check
    /// crashes on pawns with null map.
    /// </summary>
    [StaticConstructorOnStartup]
    static class WaitAutoAttackPatch
    {
        static WaitAutoAttackPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.waitautoattack");
                var method = AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack");
                if (method != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(WaitAutoAttackPatch), nameof(CheckForAutoAttackPrefix)));
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init wait auto-attack patch: " + e.Message);
            }
        }

        public static bool CheckForAutoAttackPrefix(JobDriver_Wait __instance)
        {
            // Skip auto-attack check if the pawn has no map
            var pawn = __instance?.pawn;
            if (pawn == null || pawn.Map == null)
            {
                return false; // Skip original method
            }
            return true; // Run original
        }
    }
}