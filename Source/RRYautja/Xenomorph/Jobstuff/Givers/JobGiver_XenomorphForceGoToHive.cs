using System;
using RimWorld;
using RRYautja.ExtensionMethods;

namespace Verse.AI
{
    // Token: 0x02000AF6 RID: 2806
    public class JobGiver_XenomorphForceGoToHive : ThinkNode_JobGiver
    {
        // Token: 0x06003EB6 RID: 16054 RVA: 0x001D7638 File Offset: 0x001D5A38
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_XenomorphForceGoToHive jobGiver_ForceGoTo = (JobGiver_XenomorphForceGoToHive)base.DeepCopy(resolve);
            jobGiver_ForceGoTo.defaultLocomotion = this.defaultLocomotion;
            jobGiver_ForceGoTo.jobMaxDuration = this.jobMaxDuration;
            jobGiver_ForceGoTo.canBashDoors = this.canBash;
            jobGiver_ForceGoTo.forceCanDig = this.forceCanDig;
            jobGiver_ForceGoTo.forceCanDigIfAnyHostileActiveThreat = this.forceCanDigIfAnyHostileActiveThreat;
            jobGiver_ForceGoTo.forceCanDigIfCantReachMapEdge = this.forceCanDigIfCantReachMapEdge;
            jobGiver_ForceGoTo.failIfCantJoinOrCreateCaravan = this.failIfCantJoinOrCreateCaravan;
            return jobGiver_ForceGoTo;
        }

        // Token: 0x06003EB7 RID: 16055 RVA: 0x001D76A8 File Offset: 0x001D5AA8
        protected override Job TryGiveJob(Pawn pawn)
        {
            bool flag = false;
            if (this.forceCanDig || (pawn.mindState.duty != null && pawn.mindState.duty.canDig) || (this.forceCanDigIfCantReachMapEdge && !pawn.CanReachMapEdge()) || (this.forceCanDigIfAnyHostileActiveThreat && pawn.Faction != null && GenHostility.AnyHostileActiveThreatTo(pawn.Map, pawn.Faction)))
            {
                flag = true;
            }
            IntVec3 c;
            if (pawn.xenomorph().HiveLoc == null)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} no hiveloc", pawn));
                return null;
            }
            if (!pawn.xenomorph().HiveLoc.IsValid)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} no hiveloc", pawn));
                return null;
            }
            if (pawn.xenomorph().HiveLoc == IntVec3.Zero)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} hiveloc zero", pawn));
                return null;
            }
            c = pawn.xenomorph().HiveLoc;
            
            if (!c.IsValid)
            {
                if(pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} no c", pawn));
                return null;
            }
            if (c == IntVec3.Zero)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} c zero", pawn));
                return null;
            }
            
            if (flag)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, c, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), null, PathEndMode.OnCell))
                {
                    IntVec3 cellBeforeBlocker;
                    Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                    if (thing != null)
                    {
                        if (!thing.def.defName.Contains("Xenomorph_Hive"))
                        {
                            Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true, true);
                            if (job != null)
                            {
                                return job;
                            }
                        }
                    }
                }
            }
            /*
        //    Log.Message("TryGiveJob 6");
        //    Log.Message(string.Format("TryGiveJob 6 {0}, {1}", pawn.Map, c));
            if (c.GetFirstBuilding(pawn.Map)!=null)
            {
            //    Log.Message(string.Format("TryGiveJob 6 {0}, {1} Building == {2}", pawn.Map, c, c.GetFirstBuilding(pawn.Map)));
                if (c.GetFirstBuilding(pawn.Map).def == XenomorphDefOf.RRY_Xenomorph_Hive)
                {
                //    Log.Message("TryGiveJob 6 1");
                    return null;
                }
            }
            else
            {
            //    Log.Message(string.Format("TryGiveJob 6 {0}, {1} Building == Null", pawn.Map, c));
                if (c.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive) != null)
                {
                //    Log.Message(string.Format("TryGiveJob 6 {0}, {1} Building == {2}", pawn.Map, c, c.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive)));
                    if (c.GetFirstBuilding(pawn.Map).def == )
                    {
                    //    Log.Message("TryGiveJob 6 1");
                        return null;
                    }
                }
                else
                {
                //    Log.Message(string.Format("TryGiveJob 6 {0}, {1} Building == Null", pawn.Map, c));
                }
            }
        //    Log.Message("TryGiveJob 7");
            */
            return new Job(JobDefOf.Goto, c)
            {
                exitMapOnArrival = false,
                failIfCantJoinOrCreateCaravan = this.failIfCantJoinOrCreateCaravan,
                locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, this.defaultLocomotion, LocomotionUrgency.Jog),
                expiryInterval = this.jobMaxDuration,
                canBashDoors = this.canBash
            };
        }

        // Token: 0x040027D7 RID: 10199
        protected LocomotionUrgency defaultLocomotion;

        // Token: 0x040027D8 RID: 10200
        protected int jobMaxDuration = 999999;

        // Token: 0x040027D9 RID: 10201
        protected bool canBash = true;

        // Token: 0x040027DA RID: 10202
        protected bool forceCanDig = true;

        // Token: 0x040027DB RID: 10203
        protected bool forceCanDigIfAnyHostileActiveThreat;

        // Token: 0x040027DC RID: 10204
        protected bool forceCanDigIfCantReachMapEdge;

        // Token: 0x040027DD RID: 10205
        protected bool failIfCantJoinOrCreateCaravan;
    }
}
