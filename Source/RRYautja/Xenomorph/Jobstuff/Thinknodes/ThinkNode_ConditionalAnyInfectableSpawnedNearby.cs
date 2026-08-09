using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x020001EA RID: 490
    public class ThinkNode_ConditionalAnyInfectableSpawnedNearby : ThinkNode_ConditionalHive
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalAnyInfectableSpawnedNearby thinkNode_ConditionalHive = (ThinkNode_ConditionalAnyInfectableSpawnedNearby)base.DeepCopy(resolve);
            thinkNode_ConditionalHive.NeedsLOS = this.NeedsLOS;
            thinkNode_ConditionalHive.RangePawn = this.RangePawn;
            thinkNode_ConditionalHive.RangeHive = this.RangeHive;
            return thinkNode_ConditionalHive;
        }

        // Token: 0x060009B3 RID: 2483 RVA: 0x0004DFD8 File Offset: 0x0004C3D8
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.Spawned && pawn.isXenomorph())
            {
                List<Pawn> list = pawn.Map.mapPawns.AllPawns.Where(x => !x.Downed && x.isPotentialHost() && pawn.CanReach(x, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors)).ToList();
                if (!list.NullOrEmpty())
                {
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("List is {0} long",list.Count));
                    flagSpawned = list.Any<Pawn>(x => x.Spawned);
                    flagLOS = (this.NeedsLOS && list.Any<Pawn>(x => pawn.CanSee(x))) || (!this.NeedsLOS);
                    flagPawnRange = list.Any<Pawn>(x => XenomorphUtil.DistanceBetween(pawn.Position, x.Position) <= this.RangePawn);
                    if (Tunnel(pawn) != null)
                    {
                        IntVec3 vec3 = Tunnel(pawn).Position;
                        flagHiveRange = list.Any<Pawn>(x => XenomorphUtil.DistanceBetween(vec3, x.Position) <= this.RangeHive);
                    }
                    flagRange = flagPawnRange || flagHiveRange;
                    result = (flagSpawned && flagLOS && flagRange);
                }
                else
                {
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("List is null or empty"));
                }
            }
            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} Result: {1}, flagSpawned: {2}, flagLOS: {3}, flagPawnRange: {4}, flagHiveRange: {5}, flagRange: {6}", this, result, flagSpawned, flagLOS, flagPawnRange, flagHiveRange, flagRange));
            return result;
        }

        private bool flagSpawned = false;
        private bool flagLOS = false;
        private bool flagPawnRange = false;
        private bool flagHiveRange = false;
        private bool flagRange = false;
        private bool result = false;

        public bool NeedsLOS = false;
        public int RangePawn = 99999;
        public int RangeHive = 99999;
    }
}
