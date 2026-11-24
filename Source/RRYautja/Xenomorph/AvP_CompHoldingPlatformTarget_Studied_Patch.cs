using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// CompHoldingPlatformTarget.StudiedAtHoldingPlatform gates the Capture
    /// gizmo/float menu on vanilla entity checks. For Pawn targets it requires
    /// either an entity fleshType (Fleshbeast/EntityFlesh/EntityMechanical) —
    /// animals with any other flesh type are refused outright.
    ///
    /// Xenomorphs/Neomorphs use their own flesh types (RRY_Xenomorph/RRY_Neomorph),
    /// so without this prefix they can carry CompHoldingPlatformTarget but never
    /// show the Capture option. We skip the vanilla check for our races;
    /// EverStudiable (CompStudiable.AnomalyKnowledge > 0) is still satisfied via
    /// the race-level knowledgeCategory/anomalyKnowledge XML patch.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class AvP_CompHoldingPlatformTarget_Studied_Patch
    {
        static AvP_CompHoldingPlatformTarget_Studied_Patch()
        {
            try
            {
                if (!ModsConfig.AnomalyActive)
                {
                    return;
                }
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.entitycapture");
                var prop = AccessTools.Property(typeof(CompHoldingPlatformTarget), "StudiedAtHoldingPlatform");
                if (prop != null && prop.GetGetMethod() != null)
                {
                    harmony.Patch(prop.GetGetMethod(),
                        prefix: new HarmonyMethod(typeof(AvP_CompHoldingPlatformTarget_Studied_Patch), nameof(StudiedAtHoldingPlatformPrefix)));
                    Log.Message("[AVP Xenomorphs] Patched CompHoldingPlatformTarget.StudiedAtHoldingPlatform for entity capture of Xenomorphs/Neomorphs");
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] Could not find CompHoldingPlatformTarget.StudiedAtHoldingPlatform — capture patch not applied (Anomaly active?)");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init entity capture patch: " + e.Message);
            }
        }

        public static bool StudiedAtHoldingPlatformPrefix(CompHoldingPlatformTarget __instance, ref bool __result)
        {
            try
            {
                if (__instance.parent is Pawn pawn && pawn.RaceProps != null && pawn.RaceProps.FleshType != null)
                {
                    if (pawn.RaceProps.FleshType == XenomorphRacesDefOf.RRY_Xenomorph
                        || pawn.RaceProps.FleshType == XenomorphRacesDefOf.RRY_Neomorph)
                    {
                        __result = true;
                        return false;
                    }
                }
            }
            catch
            {
                // Never throw out of the property — fall through to vanilla.
            }
            return true;
        }
    }
}
