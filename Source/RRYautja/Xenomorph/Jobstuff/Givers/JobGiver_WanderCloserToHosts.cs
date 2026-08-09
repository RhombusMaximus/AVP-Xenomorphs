using Verse;
using Verse.AI;
using RimWorld;
using RRYautja.ExtensionMethods;
using System;

namespace RRYautja
{
    // RRYautja.JobGiver_WanderCloserToHosts
    public class JobGiver_WanderCloserToHosts : JobGiver_Wander
    {
        // Token: 0x06003EF2 RID: 16114 RVA: 0x001D7E5D File Offset: 0x001D625D
        public JobGiver_WanderCloserToHosts()
        {
            this.wanderRadius = 7f;
            this.locomotionUrgency = LocomotionUrgency.Walk;
            this.ticksBetweenWandersRange = new IntRange(125, 200);
        }

        // Token: 0x06003EF3 RID: 16115 RVA: 0x001D7E89 File Offset: 0x001D6289
        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            bool anyPotentialHosts = !pawn.Map.ViableHosts().NullOrEmpty();
            if (anyPotentialHosts)
            {
                bool anyReachablePotentialHosts = pawn.Map.ViableHosts().Any(x => pawn.CanReach(x, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors));
                if (anyReachablePotentialHosts)
                {
                    Pawn potentialHost = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), float.MaxValue, (x => ((Pawn)x).isPotentialHost())) ?? null;
                    if (potentialHost != null)
                    {
                        Predicate<IntVec3> validator = delegate (IntVec3 y)
                        {
                            return XenomorphUtil.DistanceBetween(y, potentialHost.Position) < XenomorphUtil.DistanceBetween(y, pawn.Position);
                        };
                        if (RCellFinder.TryFindRandomCellNearWith(pawn.Position, validator, pawn.Map, out IntVec3 lc, 6, (int)wanderRadius))
                        {
                            return lc;
                        }
                    }
                }
            }
            return pawn.Position;
        }
    }
}
