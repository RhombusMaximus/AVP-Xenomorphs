using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x020000AC RID: 172
    public class JobGiver_Xenomorph_FleeFire : ThinkNode_JobGiver
    {
        // Token: 0x06000433 RID: 1075 RVA: 0x0002D780 File Offset: 0x0002BB80
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null) return null;
            if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse)
            {
                return null;
            }
            if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn))
            {
                return null;
            }
            Job job2 = this.FleeLargeFireJob(pawn);
            if (job2 != null)
            {
                return job2;
            }
            return null;
        }

        // Token: 0x06000434 RID: 1076 RVA: 0x0002D960 File Offset: 0x0002BD60
        private Job FleeJob(Pawn pawn, Thing danger)
        {
            IntVec3 intVec;
            bool selected = Find.Selector.SelectedObjects.Contains(pawn) && Prefs.DevMode && DebugSettings.godMode;
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.FleeAndCower)
            {
                intVec = pawn.CurJob.targetA.Cell;
            }
            else
            {
                JobGiver_Xenomorph_FleeFire.tmpThings.Clear();
                JobGiver_Xenomorph_FleeFire.tmpThings.Add(danger);
                intVec = CellFinderLoose.GetFleeDest(pawn, JobGiver_Xenomorph_FleeFire.tmpThings, 30f);
                JobGiver_Xenomorph_FleeFire.tmpThings.Clear();
            }
            if (intVec != pawn.Position)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} @:{1} is fleeing from {2} @:{3}",pawn.LabelShortCap, pawn.Position, danger.LabelShortCap, danger.Position));
                
                return new Job(JobDefOf.FleeAndCower, intVec, danger);
            }
            return null;
        }

        // Token: 0x06000435 RID: 1077 RVA: 0x0002D9FC File Offset: 0x0002BDFC
        private Job FleeLargeFireJob(Pawn pawn)
        {
            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Fire);
            if (list.Count < 60)
            {
                return null;
            }
            TraverseParms tp = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Fire closestFire = null;
            float closestDistSq = -1f;
            int firesCount = 0;
            RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, false), delegate (Region x)
            {
                List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.Fire);
                for (int i = 0; i < list2.Count; i++)
                {
                    float num = (float)pawn.Position.DistanceToSquared(list2[i].Position);
                    if (num <= 400f)
                    {
                        if (closestFire == null || num < closestDistSq)
                        {
                            closestDistSq = num;
                            closestFire = (Fire)list2[i];
                        }
                        firesCount++;
                    }
                }
                return closestDistSq <= 100f && firesCount >= 60;
            }, 18, RegionType.Set_Passable);
            if (closestDistSq <= 100f && firesCount >= 60)
            {
                Job job = this.FleeJob(pawn, closestFire);
                if (job != null)
                {
                    return job;
                }
            }
            return null;
        }

        // Token: 0x0400027B RID: 635
        private const int FleeDistance = 30;

        // Token: 0x0400027C RID: 636
        private const int DistToDangerToFlee = 30;

        // Token: 0x0400027D RID: 637
        private const int DistToFireToFlee = 10;

        // Token: 0x0400027E RID: 638
        private const int MinFiresNearbyToFlee = 60;

        // Token: 0x0400027F RID: 639
        private const int MinFiresNearbyRadius = 20;

        // Token: 0x04000280 RID: 640
        private const int MinFiresNearbyRegionsToScan = 18;

        // Token: 0x04000281 RID: 641
        private static List<Thing> tmpThings = new List<Thing>();
    }
}
