using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x020000A5 RID: 165
	public class JobGiver_WanderHiveLike : JobGiver_Wander
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_WanderHiveLike jobGiver_WanderHiveLike = (JobGiver_WanderHiveLike)base.DeepCopy(resolve);
            jobGiver_WanderHiveLike.wanderRadius = this.wanderRadius;
            return jobGiver_WanderHiveLike;
        }
        // Token: 0x0600041F RID: 1055 RVA: 0x0002CD08 File Offset: 0x0002B108
        public JobGiver_WanderHiveLike()
		{
			this.wanderRadius = 7.5f;
			this.ticksBetweenWandersRange = new IntRange(125, 200);
		}

        private HiveLike FindClosestHiveLike(Pawn pawn)
        {
            ThingDef hiveDef = null;
            List<ThingDef_HiveLike> hivedefs = DefDatabase<ThingDef_HiveLike>.AllDefsListForReading.FindAll(x => x.Faction == pawn.Faction.def);
            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("hivedefs found: {0}", hivedefs.Count));
            foreach (ThingDef_HiveLike hivedef in hivedefs)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("LordToil_HiveLikeRelated found hiveDef: {0} for {1}", hiveDef, pawn));
                if (hivedef.Faction == pawn.Faction.def)
                {
                    hiveDef = hivedef;
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("LordToil_HiveLikeRelated set hiveDef: {0} for {1}", hiveDef, pawn));
                    return (HiveLike)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(hiveDef), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, (Thing x) => x.Faction == pawn.Faction, null, 0, 30, false, RegionType.Set_Passable, false);
                }
            }
            return null;
        }

        // Token: 0x06004220 RID: 1056 RVA: 0x0022CD30 File Offset: 0x0022B130
        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            if (pawn.isXenomorph(out Comp_Xenomorph xenomorph))
            {
                return xenomorph.HiveLoc;
                /*
                if (!pawn.Map.HiveGrid().Hivelist.NullOrEmpty())
                {

                    HiveLike hivelike = pawn.mindState.duty != null && pawn.mindState.duty.focus != null ? pawn.mindState.duty.focus.Thing as HiveLike : null;
                    if (hivelike != null)
                    {
                        return hivelike.Position;
                    }

                    if (hivelike == null || !hivelike.Spawned)
                    {
                        if (XenomorphUtil.HivelikesPresent(pawn.Map))
                        {
                            return XenomorphUtil.ClosestReachableHivelike(pawn).Position;
                        }
                    }
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("JobGiver_WanderHiveLike hivelike.Position: {0}", hivelike.Position));
                }
                */
            }
            else if (pawn.isNeomorph())
            {
                Corpse corpse = (Corpse)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly), 20f);
                if (corpse!=null)
                {
                    return corpse.Position;
                }
            }
            return pawn.Position;
        }

        // Queens sit directly on top of their hive opening when idle. This giver
        // sits low in the think tree (after fight/kidnap/egglaying), so a busy
        // queen never comes here.
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn != null && pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen && pawn.isXenomorph(out Comp_Xenomorph comp) && pawn.Map != null)
            {
                IntVec3 hiveLoc = new IntVec3(comp.HiveX, 0, comp.HiveZ);
                if (hiveLoc.IsValid && hiveLoc.InBounds(pawn.Map) && hiveLoc != IntVec3.Zero)
                {
                    if (hiveLoc != pawn.Position && pawn.CanReach(hiveLoc, PathEndMode.OnCell, Danger.Deadly))
                    {
                        return new Job(JobDefOf.Goto, hiveLoc);
                    }
                    // Already on the opening: stand there for a while.
                    Job wait = new Job(JobDefOf.Wait, 500);
                    return wait;
                }
            }
            return base.TryGiveJob(pawn);
        }
	}
}
