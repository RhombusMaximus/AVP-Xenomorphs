using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x02000113 RID: 275
    public class JobGiver_XenomorphManhunter : ThinkNode_JobGiver
    {
        // Token: 0x060005B7 RID: 1463 RVA: 0x00037A28 File Offset: 0x00035E28
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null, false) == null)
            {
                return null;
            }
            Pawn pawn2 = this.FindPawnTarget(pawn);

#if DEBUG
            bool selected = Find.Selector.SingleSelectedThing == pawn;
            if (selected) Log.Message(string.Format("{0} hunting {1}", pawn, pawn2));
#endif
            if (pawn2 != null && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return this.MeleeAttackJob(pawn, pawn2);
            }
            Building building = this.FindTurretTarget(pawn);
            if (building != null)
            {
                return this.MeleeAttackJob(pawn, building);
            }
            if (pawn2 != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), null, PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out IntVec3 loc))
                    {
                        //    Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap, false);
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
                attackDoorIfTargetLost = true
            };
        }

        // Token: 0x060005B9 RID: 1465 RVA: 0x00037BC0 File Offset: 0x00035FC0
        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)XenomorphTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat, (Thing x) => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser && !((Pawn)x).health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection) && !((Pawn)x).health.hediffSet.HasHediff(XenomorphDefOf.RRY_HiddenXenomorphImpregnation) && !((Pawn)x).health.hediffSet.HasHediff(XenomorphDefOf.RRY_XenomorphImpregnation) && ((Pawn)x).kindDef != XenomorphDefOf.RRY_Xenomorph_Drone && ((Pawn)x).kindDef != XenomorphDefOf.RRY_Xenomorph_FaceHugger && ((Pawn)x).kindDef != XenomorphDefOf.RRY_Xenomorph_Queen && ((Pawn)x).kindDef != XenomorphDefOf.RRY_Xenomorph_Runner && ((Pawn)x).kindDef != XenomorphDefOf.RRY_Xenomorph_Warrior, 0f, 9999f, default(IntVec3), float.MaxValue, true, true);
        }

        // Token: 0x060005BA RID: 1466 RVA: 0x00037C14 File Offset: 0x00036014
        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)XenomorphTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building, 0f, 70f, default(IntVec3), float.MaxValue, false, true);
        }

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
