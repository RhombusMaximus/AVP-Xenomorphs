using RRYautja;
using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x020001BE RID: 446
    public class ThinkNode_ConditionalFullyGrown : ThinkNode_Conditional
    {
        // Token: 0x06000956 RID: 2390 RVA: 0x0004D678 File Offset: 0x0004BA78
        protected override bool Satisfied(Pawn pawn)
        {
            LifeStageDef stage = pawn.ageTracker.CurLifeStage;
#if DEBUG
            bool selected = Find.Selector.SingleSelectedThing == pawn;
        //    if (selected&&pawn.kindDef!=XenomorphDefOf.RRY_Xenomorph_FaceHugger&&stage == pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def) Log.Message(string.Format("ThinkNode_ConditionalFullyGrown {0} \nCurLifeStage:{1} FinalLifeStage:{2}", pawn.Label, stage, pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def));
#endif
            return stage == pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def;
        }

    }

    public class ThinkNode_ConditionalNotGrown : ThinkNode_Conditional
    {
        // Token: 0x06000956 RID: 2390 RVA: 0x0004D678 File Offset: 0x0004BA78
        protected override bool Satisfied(Pawn pawn)
        {
            LifeStageDef stage = pawn.ageTracker.CurLifeStage;
#if DEBUG
            bool selected = Find.Selector.SingleSelectedThing == pawn;
         //   if (selected&&stage != pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def) Log.Message(string.Format("ThinkNode_ConditionalNotGrown {0} \nCurLifeStage:{1} FinalLifeStage:{2}", pawn.Label, stage, pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def));
#endif
            return stage != pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def;
        }

    }

    public class ThinkNode_ConditionalFacehuggerFertile : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null) return false;
            Comp_Facehugger _Facehugger = pawn.TryGetComp<Comp_Facehugger>();
            if (_Facehugger == null) return false;
            // Don't hunt if already attached to a host (impregnating)
            if (_Facehugger.Impregnations >= _Facehugger.maxImpregnations) return false;
            // Don't hunt if currently in a melee attack job (attached to host)
            if (pawn.jobs?.curJob != null && pawn.jobs.curJob.def == JobDefOf.AttackMelee) return false;
            // Don't hunt if downed/stunned
            if (pawn.Downed || pawn.stances?.FullBodyStunned == true) return false;
            return true;
        }
    }
}
