using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    /// <summary>
    /// Prevents Xenomorphs from targeting colony buildings in normal combat.
    /// The Power Cut event uses a separate Lord (LordJob_AssaultColony_CutPower)
    /// with its own duty that targets power buildings — that still works.
    /// </summary>
    [StaticConstructorOnStartup]
    static class XenoNoBuildingAttackPatch
    {
        static XenoNoBuildingAttackPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.nobuildingattack");
                // Patch AttackTargetFinder.BestAttackTarget to filter out buildings for Xenos
                var method = AccessTools.Method(typeof(AttackTargetFinder), "BestAttackTarget");
                if (method != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(XenoNoBuildingAttackPatch), nameof(BestAttackTargetPrefix)));
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init no-building-attack patch: " + e.Message);
            }
        }

        /// <summary>
        /// For Xenomorph pawns that are NOT in the Power Cut lord,
        /// filter out buildings from the attack target search.
        /// </summary>
        public static void BestAttackTargetPrefix(IAttackTargetSearcher searcher, ref Predicate<IAttackTarget> validator)
        {
            Pawn pawn = searcher as Pawn;
            if (pawn == null) return;
            if (pawn.Map == null) return;
            if (!pawn.isXenomorph()) return;

            // Check if this pawn is in the Power Cut lord — if so, allow building attacks
            Lord lord = pawn.GetLord();
            if (lord?.LordJob is LordJob_AssaultColony_CutPower) return; // Power Cut event — allow buildings

            // For normal Xenos, wrap the validator to exclude buildings
            Predicate<IAttackTarget> original = validator;
            validator = (IAttackTarget t) =>
            {
                if (t is Thing thing && thing is Building) return false; // Skip buildings
                if (original != null) return original(t);
                return true;
            };
        }
    }
}