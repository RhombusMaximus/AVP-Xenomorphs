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

                // Patch JobGiver_AITrashColonyClose.TryGiveJob to prevent Xenos from trashing nearby buildings
                var trashCloseMethod = AccessTools.Method(typeof(JobGiver_AITrashColonyClose), "TryGiveJob");
                if (trashCloseMethod != null)
                {
                    harmony.Patch(trashCloseMethod, prefix: new HarmonyMethod(typeof(XenoNoBuildingAttackPatch), nameof(TrashColonyClosePrefix)));
                }

                // Patch JobGiver_AITrashBuildingsDistant.TryGiveJob to prevent Xenos from trashing distant buildings
                var trashDistantMethod = AccessTools.Method(typeof(JobGiver_AITrashBuildingsDistant), "TryGiveJob");
                if (trashDistantMethod != null)
                {
                    harmony.Patch(trashDistantMethod, prefix: new HarmonyMethod(typeof(XenoNoBuildingAttackPatch), nameof(TrashDistantPrefix)));
                }

                // Patch JobGiver_AISapper.TryGiveJob to prevent Xenos from mining through walls (except CutPower raids)
                var sapperMethod = AccessTools.Method(typeof(JobGiver_AISapper), "TryGiveJob");
                if (sapperMethod != null)
                {
                    harmony.Patch(sapperMethod, prefix: new HarmonyMethod(typeof(XenoNoBuildingAttackPatch), nameof(SapperPrefix)));
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
            try
            {
                Pawn pawn = searcher as Pawn;
                if (pawn == null || pawn.Map == null || pawn.Destroyed) return;
                if (!pawn.isXenomorph()) return;

                // Check if this pawn is in the Power Cut lord — if so, allow building attacks
                Lord lord = pawn.GetLord();
                if (lord?.LordJob is LordJob_AssaultColony_CutPower) return; // Power Cut event — allow buildings

                // For normal Xenos, wrap the validator to exclude buildings
                Predicate<IAttackTarget> original = validator;
                validator = (IAttackTarget t) =>
                {
                    try
                    {
                        if (t is Thing thing && thing is Building) return false; // Skip buildings
                        if (t is Thing thing2 && thing2.Map == null) return false; // Skip null-map targets
                        if (original != null) return original(t);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                };
            }
            catch
            {
                // If our patch fails, don't break BestAttackTarget
            }
        }

        /// <summary>
        /// Prevents Xenomorphs from trashing nearby colony buildings via JobGiver_AITrashColonyClose.
        /// Exception: Xenos in a CutPower raid lord are allowed to trash.
        /// </summary>
        public static bool TrashColonyClosePrefix(Pawn pawn, ref Job __result)
        {
            try
            {
                if (pawn == null || !pawn.isXenomorph()) return true; // let original run
                if (pawn.GetLord()?.LordJob is LordJob_AssaultColony_CutPower) return true; // allow for CutPower raids
                __result = null;
                return false; // skip original method
            }
            catch
            {
                return true; // on error, let original run
            }
        }

        /// <summary>
        /// Prevents Xenomorphs from trashing distant colony buildings via JobGiver_AITrashBuildingsDistant.
        /// Exception: Xenos in a CutPower raid lord are allowed to trash.
        /// </summary>
        public static bool TrashDistantPrefix(Pawn pawn, ref Job __result)
        {
            try
            {
                if (pawn == null || !pawn.isXenomorph()) return true; // let original run
                if (pawn.GetLord()?.LordJob is LordJob_AssaultColony_CutPower) return true; // allow for CutPower raids
                __result = null;
                return false; // skip original method
            }
            catch
            {
                return true; // on error, let original run
            }
        }

        /// <summary>
        /// Prevents Xenomorphs from mining through walls via JobGiver_AISapper.
        /// Exception: Xenos in a CutPower raid lord are allowed to sap.
        /// </summary>
        public static bool SapperPrefix(Pawn pawn, ref Job __result)
        {
            try
            {
                if (pawn == null || !pawn.isXenomorph()) return true; // let original run
                if (pawn.GetLord()?.LordJob is LordJob_AssaultColony_CutPower) return true; // allow for CutPower raids
                __result = null;
                return false; // skip original method
            }
            catch
            {
                return true; // on error, let original run
            }
        }
    }
}