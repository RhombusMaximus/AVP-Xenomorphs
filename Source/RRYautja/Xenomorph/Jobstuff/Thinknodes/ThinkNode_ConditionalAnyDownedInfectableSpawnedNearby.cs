using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x020001EA RID: 490 .skyManager.CurSkyGlow <= 0.5f
    public class ThinkNode_ConditionalAnyDownedInfectableSpawnedNearby : ThinkNode_Conditional
    {
        // Token: 0x060009B8 RID: 2488 RVA: 0x0004E07C File Offset: 0x0004C47C
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalAnyDownedInfectableSpawnedNearby thinkNode_ConditionalPawnKind = (ThinkNode_ConditionalAnyDownedInfectableSpawnedNearby)base.DeepCopy(resolve);
            thinkNode_ConditionalPawnKind.Humanlike = this.Humanlike;
            thinkNode_ConditionalPawnKind.Humanlike = this.Nonhumanlike;
            return thinkNode_ConditionalPawnKind;
        }
        
        public bool Humanlike = true;
        public bool Nonhumanlike = true;

        // Token: 0x060009B3 RID: 2483 RVA: 0x0004DFD8 File Offset: 0x0004C3D8
        protected override bool Satisfied(Pawn pawn)
        {
            bool result;
            if (pawn.Spawned && XenomorphUtil.IsXenomorph(pawn) && pawn.Map.mapPawns.AllPawns.Any((Pawn x) => x.Downed && x.isPotentialHost() && pawn.CanReach(x, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors)))
            {
                List<Pawn> list = pawn.Map.mapPawns.AllPawns.Where((Pawn x) => x.Downed && x.isPotentialHost() && pawn.CanReach(x, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors)).ToList();
                result = !list.NullOrEmpty() ? list.Any<Pawn>(x => x.Spawned) : false;
            }
            else
            {
                result = false;
            }
            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} Result: {1}", this, result));
            return result;
        }

    }
}
