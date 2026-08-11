using RimWorld;
using Verse;
using HarmonyLib;

namespace RRYautja
{
    // Prevents stripping of cocooned pawns
    [HarmonyPatch(typeof(Pawn), "AnythingToStrip")]
    public static class AvP_Pawn_AnythingToStrip_Patch
    {
        [HarmonyPostfix]
        public static void IgnoreCocooned(Pawn __instance, ref bool __result)
        {
            __result = __result && !(__instance.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Cocooned));
        }
    }
}