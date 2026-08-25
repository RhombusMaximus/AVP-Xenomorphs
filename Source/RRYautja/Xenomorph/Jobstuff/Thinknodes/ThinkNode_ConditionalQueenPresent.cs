using RRYautja;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Linq;

namespace RimWorld
{
    public class ThinkNode_ConditionalQueenPresent : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
            PawnKindDef Queen = XenomorphDefOf.RRY_Xenomorph_Queen;
            return pawn.Map.mapPawns.AllPawnsSpawned.Any(x => x.kindDef == Queen);
        }
    }
    public class ThinkNode_ConditionalQueenAbsent : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return true;
            PawnKindDef Queen = XenomorphDefOf.RRY_Xenomorph_Queen;
            return !pawn.Map.mapPawns.AllPawnsSpawned.Any(x => x.kindDef == Queen);
        }
    }

    public class ThinkNode_ConditionalHivePresent : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
            var hive = XenomorphUtil.ClosestReachableHivelike(pawn);
            return hive != null && !hive.DestroyedOrNull();
        }
    }
    public class ThinkNode_ConditionalHiveAbsent : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return true;
            var hive = XenomorphUtil.ClosestReachableHivelike(pawn);
            return hive == null || hive.DestroyedOrNull();
        }
    }

    public class ThinkNode_ConditionalNotDefendPoint : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            Lord lord = pawn?.GetLord();
            if (lord == null) return true;
            return !(lord.LordJob is LordJob_DefendPoint);
        }
    }

    public class ThinkNode_ConditionalDefendPoint : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            Lord lord = pawn?.GetLord();
            if (lord == null) return false;
            return lord.LordJob is LordJob_DefendPoint;
        }
    }

}