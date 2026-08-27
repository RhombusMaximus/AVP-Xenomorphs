using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
    public class ThinkNode_ConditionalBleeding : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return false;
            return pawn.health.hediffSet.BleedRateTotal > 0.001f;
        }
    }

    public class ThinkNode_Conditional_ThreeQuatHealthBleeding : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || pawn?.health?.summaryHealth == null) return false;
            return pawn.health.hediffSet.BleedRateTotal > 0.001f && pawn.health.summaryHealth.SummaryHealthPercent <= 0.75f && pawn.health.summaryHealth.SummaryHealthPercent >= 0.51f;
        }
    }

    public class ThinkNode_Conditional_HalfHealthBleeding : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || pawn?.health?.summaryHealth == null) return false;
            return pawn.health.hediffSet.BleedRateTotal > 0.001f && pawn.health.summaryHealth.SummaryHealthPercent <= 0.5f && pawn.health.summaryHealth.SummaryHealthPercent >= 0.251f;
        }
    }

    public class ThinkNode_Conditional_QuatHealthBleeding : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || pawn?.health?.summaryHealth == null) return false;
            return pawn.health.hediffSet.BleedRateTotal > 0.001f && pawn.health.summaryHealth.SummaryHealthPercent <= 0.25f;
        }
    }

    public class ThinkNode_Conditional_OverHealth : ThinkNode_Conditional
    {
        public float pawnHealth;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_Conditional_OverHealth node = (ThinkNode_Conditional_OverHealth)base.DeepCopy(resolve);
            node.pawnHealth = this.pawnHealth;
            return node;
        }

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || pawn?.health?.summaryHealth == null) return false;
            return pawn.health.summaryHealth.SummaryHealthPercent >= pawnHealth && pawn.health.hediffSet.PainTotal < pawnHealth;
        }
    }

    public class ThinkNode_Conditional_UnderHealth : ThinkNode_Conditional
    {
        public float pawnHealth;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_Conditional_UnderHealth node = (ThinkNode_Conditional_UnderHealth)base.DeepCopy(resolve);
            node.pawnHealth = this.pawnHealth;
            return node;
        }

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.summaryHealth == null) return false;
            return pawn.health.summaryHealth.SummaryHealthPercent <= pawnHealth;
        }
    }

    public class ThinkNode_Conditional_OverBleed : ThinkNode_Conditional
    {
        public float pawnBleedRate;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_Conditional_OverBleed node = (ThinkNode_Conditional_OverBleed)base.DeepCopy(resolve);
            node.pawnBleedRate = this.pawnBleedRate;
            return node;
        }

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return false;
            return pawn.health.hediffSet.BleedRateTotal >= pawnBleedRate;
        }
    }

    public class ThinkNode_Conditional_UnderBleed : ThinkNode_Conditional
    {
        public float pawnBleedRate;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_Conditional_UnderBleed node = (ThinkNode_Conditional_UnderBleed)base.DeepCopy(resolve);
            node.pawnBleedRate = this.pawnBleedRate;
            return node;
        }

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return false;
            return pawn.health.hediffSet.BleedRateTotal <= pawnBleedRate;
        }
    }
}