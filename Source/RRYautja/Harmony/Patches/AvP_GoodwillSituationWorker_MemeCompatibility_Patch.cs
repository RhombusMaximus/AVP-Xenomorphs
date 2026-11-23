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
}
