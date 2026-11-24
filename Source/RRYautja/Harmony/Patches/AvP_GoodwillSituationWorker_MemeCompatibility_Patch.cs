using RimWorld;
using Verse;
using HarmonyLib;
using System;

namespace RRYautja
{
    /// <summary>
    /// Vanilla GoodwillSituationWorker_MemeCompatibility.Applies(a, b) dereferences
    /// a.ideos / b.ideos without a null check. Factions that carry no ideology
    /// (like the hidden Xenomorph faction) therefore NRE whenever anything
    /// recalculates goodwill against them — observed ~4,100 logged NREs in one
    /// session, triggered by CharacterEditor's DialogChangeFaction. Guard both
    /// factions here and report "does not apply" instead of throwing.
    /// </summary>
    [HarmonyPatch(typeof(GoodwillSituationWorker_MemeCompatibility), "Applies", new Type[] { typeof(Faction), typeof(Faction) })]
    public static class AvP_GoodwillSituationWorker_MemeCompatibility_Patch
    {
        [HarmonyPrefix]
        public static bool Applies_MemeCompatibility_Prefix(Faction a, Faction b, ref bool __result)
        {
            if (a == null || b == null || a.ideos == null || b.ideos == null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Same guard for GoodwillSituationWorker_SameIdeo.GetNaturalGoodwillOffset:
    /// it dereferences other.ideos.PrimaryIdeo without a null check on the tracker
    /// (vanilla guards the player's PrimaryIdeo but not `other.ideos`). Factions
    /// with no ideology (hidden Xenomorph faction) NRE every goodwill recalc tick.
    /// </summary>
    [HarmonyPatch(typeof(GoodwillSituationWorker_SameIdeo), "GetNaturalGoodwillOffset")]
    public static class AvP_GoodwillSituationWorker_SameIdeo_Patch
    {
        [HarmonyPrefix]
        public static bool GetNaturalGoodwillOffset_SameIdeo_Prefix(Faction other, ref int __result)
        {
            if (other == null || other.ideos == null)
            {
                __result = 0;
                return false;
            }
            return true;
        }
    }
}
