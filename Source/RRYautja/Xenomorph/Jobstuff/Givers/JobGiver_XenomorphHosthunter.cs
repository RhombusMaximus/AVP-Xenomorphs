using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x02000113 RID: 275
    public class JobGiver_XenomorphHosthunter : ThinkNode_JobGiver
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_XenomorphHosthunter jobGiver_XenomorphHosthunter = (JobGiver_XenomorphHosthunter)base.DeepCopy(resolve);
            jobGiver_XenomorphHosthunter.HuntingRange = this.HuntingRange;
            jobGiver_XenomorphHosthunter.MinMeleeChaseTicks = this.MinMeleeChaseTicks;
            jobGiver_XenomorphHosthunter.MaxMeleeChaseTicks = this.MaxMeleeChaseTicks;
            jobGiver_XenomorphHosthunter.Gender = this.Gender;
            jobGiver_XenomorphHosthunter.requireLOS = this.requireLOS;
            return jobGiver_XenomorphHosthunter;
        }

        public HiveLike hive = null;

        // Token: 0x040002FF RID: 767
        private int MinMeleeChaseTicks = 420;

        // Token: 0x04000300 RID: 768
        private int MaxMeleeChaseTicks = 900;

        private float HuntingRange = 9999f;

        // Token: 0x040002FD RID: 765
        private const float WaitChance = 0.75f;

        // Token: 0x040002FE RID: 766
        private const int WaitTicks = 90;

        // Token: 0x04000301 RID: 769
        private const int WanderOutsideDoorRegions = 9;

        private Gender Gender = Gender.None;

        private bool requireLOS = true;
        // Token: 0x060005B7 RID: 1463 RVA: 0x00037A28 File Offset: 0x00035E28
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = null;
            if (pawn == null || pawn.Map == null) return job;
            bool healthy = (pawn.health?.summaryHealth?.SummaryHealthPercent > 0.75f && pawn.health?.hediffSet?.BleedRateTotal < 0.15f);
            if (!healthy)
            {
                return job;
            }
            if (pawn.GetLord() != null && pawn.GetLord() is Lord lord)
            {
                if (lord.LordJob is LordJob_DefendAndExpandHiveLike hivejob)
                {
                    if (lord.CurLordToil is LordToil_DefendAndExpandHiveLike hivetoil)
                    {
                        if (hivetoil.Data.assignedHiveLikes.TryGetValue(pawn) != null)
                        {
                            hive = hivetoil.Data.assignedHiveLikes.TryGetValue(pawn);
                        }
                    }
                }
                else
                if (lord.CurLordToil is LordToil_DefendHiveLikeAggressively hivetoilA)
                {
                    if (hivetoilA.Data.assignedHiveLikes.TryGetValue(pawn) != null)
                    {
                        hive = hivetoilA.Data.assignedHiveLikes.TryGetValue(pawn);
                    }
                }
            }

        //    bool aggressive;
            Comp_Xenomorph _Xenomorph = pawn.TryGetComp<Comp_Xenomorph>();
            if (pawn.TryGetAttackVerb(null, false) == null)
            {
                return job;
            }


            Pawn pawn2 = null;
            if (hive!=null)
            {
                if (_Xenomorph != null)
                {
                    if (!hive.hiveDormant)
                    {
                        pawn2 = _Xenomorph.BestPawnToHuntForPredator(pawn, false, true);
                        requireLOS = false;
                        HuntingRange = 40;
                    }
                    else
                    {
                        HuntingRange = 20;
                    }
                }
                else
                {

                }
            }
            if (pawn2 == null)
            {
                pawn2 = this.FindPawnTarget(pawn);
            }
            if (pawn2 != null)
            {
                if (pawn2.isPotentialHost())
                {
                    if (pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    {
                        job = this.MeleeAttackJob(pawn, pawn2);
                    }
                    else
                    {
                        using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, true), null, PathEndMode.OnCell))
                        {
                            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("pawnPath.TotalCost: {0}", pawnPath.TotalCost));

                            if (!pawnPath.Found)
                            {
                                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("pawnPath.Found NOT Found"));
                                job = null;
                            }
                            else
                            if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out IntVec3 loc))
                            {
                                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("pawnPath.TryFindLastCellBeforeBlockingDoor NOT Found"));
                                //    Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap, false);
                                job = null;
                            }
                            /*
                            else
                            {
                                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("pawnPath.TryFindLastCellBeforeBlockingDoor NOT Found"));
                                IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, true), null, null, RegionType.Set_Passable).RandomCell;
                                if (randomCell == pawn.Position)
                                {
                                    job = new Job(JobDefOf.Wait, 30, false);
                                }
                                else
                                {
                                    job = new Job(JobDefOf.Goto, randomCell);
                                }
                            }
                            */
                        }
                    }
                }
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0}: {1} Job Found: {2}: {3}", this, pawn, job != null, job));
            }
            else
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0}: {1} No target Found: {2}: {3}", this, pawn, job != null, job));
            }

            return job;
        }

        // Token: 0x060005B8 RID: 1464 RVA: 0x00037B7C File Offset: 0x00035F7C
        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            return new Job(JobDefOf.AttackMelee, target)
            {
                maxNumMeleeAttacks = 1,
                expiryInterval = Rand.Range(MinMeleeChaseTicks, MaxMeleeChaseTicks) * (int)Math.Max(1, (pawn.Position.DistanceTo(target.Position) / 5)),
                killIncappedTarget = false,
                attackDoorIfTargetLost = true
            };
        }

        // Token: 0x060005B9 RID: 1465 RVA: 0x00037BC0 File Offset: 0x00035FC0
        private Pawn FindPawnTarget(Pawn pawn)
        {
            Pawn pawnt = null;
            float HuntingRange = this.HuntingRange;
            bool requireLOS = this.requireLOS;
            bool selected = Find.Selector.SingleSelectedThing == pawn;
            if (pawn == null || pawn.Map == null) return null;
            if (!pawn.isXenomorph(out Comp_Xenomorph xenomorph))
            {
                return null;
            }
            if (pawn.Map.skyManager == null) return null;
            if (pawn.Map.skyManager.CurSkyGlow<0.5f)
            {
                HuntingRange = HuntingRange * 2;
                requireLOS = false;
            }
            // Filter pawns WITHOUT CanReach (expensive pathfinding) — do that later only on candidates
            List<Pawn> candidates = pawn.Map.mapPawns.AllPawns.Where((Pawn x) =>
                x != null && x.health != null && x.health.hediffSet != null
                && !x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Cocooned)
                && (!x.Downed || !x.Awake())
                && x.isPotentialHost()
                && !pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Anesthetic)
                && (this.Gender == Gender.None || x.gender == this.Gender)).ToList();
            // Now do CanReach only on the filtered candidates (much fewer than all pawns)
            List<Pawn> list = candidates.Where(x => pawn.CanReach(x, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors)).ToList();
            if (!list.NullOrEmpty())
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("Xeno found {0} possible targets", list.Count));
                if (list.Any(x => xenomorph.IsAcceptablePreyFor(pawn, x, true)))
                {
                    list = list.Where(x => xenomorph.IsAcceptablePreyFor(pawn, x, true) && (pawn.CanSee(x) || !requireLOS)).ToList();
                }
                else
                {
                    list = new List<Pawn>();
                }
                if (!list.NullOrEmpty())
                {
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("Xeno found {0} Acceptable Prey", list.Count));
                    Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), HuntingRange, (x => x is Pawn p && list.Contains(p)));//(Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing x) => x is Pawn p && XenomorphUtil.isInfectablePawn(p) && !p.Downed, 0f, 9999f, default(IntVec3), float.MaxValue, true, true);
                    if (pawn2!=null)
                    {
                        pawnt = pawn2;
                    }
                    else
                    {
                        if (pawn.jobs.debugLog)
                        {
                            Pawn pawn3 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => x is Pawn p && list.Contains(p)));//(Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing x) => x is Pawn p && XenomorphUtil.isInfectablePawn(p) && !p.Downed, 0f, 9999f, default(IntVec3), float.MaxValue, true, true);
                            pawn.jobs.DebugLogEvent(string.Format("Xeno found no reachable targets in range\nrequireLOS: {0}, Gender: {1}, Range: {2}, Closest: {3}", requireLOS, Gender, HuntingRange, pawn3.Position.DistanceTo(pawn.Position)));
                        }
                    }
                }
                else
                {
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("Xeno found no Acceptable Prey to hunt\nrequireLOS: {0}, Gender: {1}, Range: {2}", requireLOS, Gender, HuntingRange));
                }
            }
            else
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("Xeno found no targets hunt\nrequireLOS: {0}, Gender: {1}, Range: {2}", requireLOS, Gender, HuntingRange));
            }
            if (pawnt != null)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("Xeno hunting {0}", pawnt));
                return pawnt;
            }
            else
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("Xeno found no target hunt"));
                return null;
            }
        }
        
        public bool forceScanWholeMap = true;
        
    }
}
