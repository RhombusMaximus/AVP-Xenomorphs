using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x02000113 RID: 275
    public class JobGiver_FacehuggerHunt : ThinkNode_JobGiver
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_FacehuggerHunt jobGiver_XenosKidnap = (JobGiver_FacehuggerHunt)base.DeepCopy(resolve);
            jobGiver_XenosKidnap.HuntingRange = this.HuntingRange;
            return jobGiver_XenosKidnap;
        }
        private float HuntingRange = 20f;

        // Token: 0x060005B7 RID: 1463 RVA: 0x00037A28 File Offset: 0x00035E28
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!JobGiverTickThrottle.ShouldRun(pawn)) return null;
            if (pawn?.Map == null) return null;
            Comp_Facehugger _Facehugger = pawn.TryGetComp<Comp_Facehugger>();
            if (_Facehugger == null) return null;
            bool selected = Find.Selector.SingleSelectedThing == pawn;
            if (pawn.TryGetAttackVerb(null, false) == null)
            {
                return null;
            }
            Pawn pawn2 = FindPawnTarget(pawn);
        //    Pawn pawn2 = BestPawnToHuntForPredator(pawn, forceScanWholeMap);
            if (pawn2 != null && !pawn2.Dead && _Facehugger.Impregnations<_Facehugger.maxImpregnations && pawn2.isPotentialHost() && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {

#if DEBUG
            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0}@{1} hunting {2}@{3}", pawn.Label, pawn.Position, pawn2.Label, pawn2.Position));
#endif
                return this.MeleeAttackJob(pawn, pawn2);
            }
            if (pawn2 != null && pawn2.isPotentialHost())
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), null, PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out IntVec3 loc))
                    {
                        Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap);
                        return null;
                    }
                    IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
                    if (randomCell == pawn.Position)
                    {
                        return new Job(JobDefOf.Wait, 30, false);
                    }
                    return new Job(JobDefOf.Goto, randomCell);
                }
            }
            return null;
        }

        // Token: 0x060005B8 RID: 1464 RVA: 0x00037B7C File Offset: 0x00035F7C
        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            return new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(420, 900),
                attackDoorIfTargetLost = false,
                killIncappedTarget=true
            };
        }

        // Token: 0x060005B9 RID: 1465 RVA: 0x00037BC0 File Offset: 0x00035FC0
        private Pawn FindPawnTarget(Pawn pawn)
        {
            if (pawn?.Map == null) return null;
            Predicate<Thing> validator = delegate (Thing x)
            {
                Pawn p = (Pawn)x;
                if (p == null) return false;
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0}\nisInfectablePawn: {1}, Alive: {2}", pawn.LabelShortCap, p.isPotentialHost(), !p.Dead));
                return p.isPotentialHost();
            };
            Pawn pawn2 = (Pawn)XenomorphTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, validator, 0f, HuntingRange, pawn.Position, float.MaxValue, true, true);
            /*
            while (pawn2==null && HuntingRange<50)
            {
                HuntingRange += 10;
            //    Log.Message(string.Format("{0}@{1} hunting failed, extending radius to {2}", pawn.Label, pawn.Position, HuntingRange));
                pawn2 = (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, validator, 0f, HuntingRange, pawn.Position, float.MaxValue, true, true);
            //    return pawn2;
            }
            */
            if (pawn2 == null) pawn2 = BestPawnToHuntForPredator(pawn, forceScanWholeMap);
            if (pawn2 != null)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0}@{1} hunting {2}@{3}", pawn.Label, pawn.Position, pawn2.Label, pawn2.Position));
            }
            return pawn2 ?? null;
        }

        // Token: 0x060005BA RID: 1466 RVA: 0x00037C14 File Offset: 0x00036014
        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)XenomorphTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building, 0f, 70f, default(IntVec3), float.MaxValue, false, true);
        }

        // RimWorld.FoodUtility
        private static Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap)
        {
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
            {
                return null;
            }
            bool flag = false;
            float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
            if (summaryHealthPercent < 0.25f)
            {
                flag = true;
            }
            tmpPredatorCandidates.Clear();
            int maxRegionsToScan = GetMaxRegionsToScan(predator, forceScanWholeMap);
            if (maxRegionsToScan < 0)
            {
                tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
            }
            else
            {
                TraverseParms traverseParms = TraverseParms.For(predator, Danger.Deadly, TraverseMode.ByPawn, false);
                RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, true), delegate (Region x)
                {
                    List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                    for (int j = 0; j < list.Count; j++)
                    {
                        Pawn p = ((Pawn)list[j]);
                        if (p.isPotentialHost())
                        {
                            tmpPredatorCandidates.Add(p);
                        }
                        else
                        {
                            if (predator.jobs.debugLog) predator.jobs.DebugLogEvent(string.Format("{0} cannae hunt {2} XenoInfection:{1} IsXenos:{3}", predator.Label, XenomorphUtil.IsInfectedPawn(p), ((Pawn)list[j]).Label, XenomorphUtil.IsXenomorph(p)));
                        }
                    }
                    return false;
                }, 999999, RegionType.Set_Passable);
            }
            Pawn pawn = null;
            float num = 0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            for (int i = 0; i < tmpPredatorCandidates.Count; i++)
            {
                Pawn pawn2 = tmpPredatorCandidates[i];
                if (predator.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
                {
                    if (predator != pawn2)
                    {
                        if (!flag || pawn2.Downed)
                        {
                            if (IsAcceptablePreyFor(predator, pawn2))
                            {
                                if (predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                                {
                                    if (!pawn2.IsForbidden(predator))
                                    {
                                        if (!tutorialMode || pawn2.Faction != Faction.OfPlayer)
                                        {
                                            float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2);
                                            // Prioritize downed pawns (easy targets, can't fight back)
                                            if (pawn2.Downed)
                                            {
                                                preyScoreFor *= 5f; // Downed targets get 5x score boost
                                            }
                                            // Prioritize animals over humanlike pawns
                                            if (!pawn2.RaceProps.Humanlike)
                                            {
                                                preyScoreFor *= 3f; // Animals get 3x score boost
                                            }
                                            if (preyScoreFor > num || pawn == null)
                                            {
                                                num = preyScoreFor;
                                                pawn = pawn2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            tmpPredatorCandidates.Clear();
            return pawn;
        }
        
        public static bool IsAcceptablePreyFor(Pawn predator, Pawn prey)
        {

            if (predator.jobs.debugLog) predator.jobs.DebugLogEvent(string.Format("{0} hunting {1}? XenoInfection:{2} IsXenos:{3}", predator.Label, prey.Label, XenomorphUtil.IsInfectedPawn(prey), XenomorphUtil.IsXenomorph(prey)));
            if (XenomorphUtil.IsInfectedPawn(prey))
            {
                if (predator.jobs.debugLog) predator.jobs.DebugLogEvent(string.Format("{0} cant hunt {1} cause XenoInfection:{2}", predator.Label, prey.Label, XenomorphUtil.IsInfectedPawn(prey)));
                return false;
            }
            if (XenomorphUtil.IsXenomorph(prey))
            {
                if (predator.jobs.debugLog) predator.jobs.DebugLogEvent(string.Format("{0} cant hunt {1} cause IsXenos:{2}", predator.Label, prey.Label, XenomorphUtil.IsXenomorph(prey)));
                return false;
            }
            if (!prey.RaceProps.IsFlesh)
            {
                return false;
            }
            if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
            {
                return false;
            }
            if (prey.BodySize < 0.3f && !prey.RaceProps.Humanlike)
            {
                return false;
            }
            /*
            if (!prey.Downed)
            {
                if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
                {
                    return false;
                }
                float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.ageTracker.CurLifeStage.bodySizeFactor;
                float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor;
                if (num >= num2)
                {
                    return false;
                }
            }
            */
            if (predator.jobs.debugLog) predator.jobs.DebugLogEvent(string.Format("{0}@{1} can hunt {2}@{3}", predator.Label, predator.Position, prey.Label, prey.Position));
            return (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (predator.Faction == null || prey.HostFaction == null || predator.HostileTo(prey)) && (predator.Faction != Faction.OfPlayer || prey.Faction != Faction.OfPlayer) && (!predator.RaceProps.herdAnimal || predator.def != prey.def) && (!prey.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection) && !prey.health.hediffSet.HasHediff(XenomorphDefOf.RRY_HiddenXenomorphImpregnation) && !prey.health.hediffSet.HasHediff(XenomorphDefOf.RRY_XenomorphImpregnation));
        }

        private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
        {
            if (getter.RaceProps.Humanlike)
            {
                return -1;
            }
            if (forceScanWholeMap)
            {
                return -1;
            }
            if (getter.Faction == Faction.OfPlayer)
            {
                return 100;
            }
            return 30;
        }

        private static List<Pawn> tmpPredatorCandidates = new List<Pawn>();

        public bool forceScanWholeMap = false;

        // Token: 0x040002FD RID: 765
        private const float WaitChance = 0.75f;

        // Token: 0x040002FE RID: 766
        private const int WaitTicks = 90;

        // Token: 0x040002FF RID: 767
        private const int MinMeleeChaseTicks = 420;

        // Token: 0x04000300 RID: 768
        private const int MaxMeleeChaseTicks = 900;

        // Token: 0x04000301 RID: 769
        private const int WanderOutsideDoorRegions = 9;
    }
}
