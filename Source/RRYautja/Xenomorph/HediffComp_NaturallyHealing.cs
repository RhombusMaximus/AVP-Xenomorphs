using System;
using System.Linq;
using Verse;

namespace RRYautja
{
    public class HediffCompProperties_NaturallyHealing : HediffCompProperties
    {
        // Token: 0x06000003 RID: 3 RVA: 0x00002203 File Offset: 0x00000403
        public HediffCompProperties_NaturallyHealing()
        {
            this.compClass = typeof(HediffComp_NaturallyHealing);
        }

        // Token: 0x04000001 RID: 1
        public int healIntervalTicks = 60;
    }
    // Token: 0x02000005 RID: 5
    public class HediffComp_NaturallyHealing : HediffComp
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000005 RID: 5 RVA: 0x00002240 File Offset: 0x00000440
        public HediffCompProperties_NaturallyHealing Props
        {
            get
            {
                return (HediffCompProperties_NaturallyHealing)this.props;
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x0000225D File Offset: 0x0000045D
        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksSinceHeal, "ticksSinceHeal", 0, false);
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002274 File Offset: 0x00000474
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            this.ticksSinceHeal++;
            bool flag = this.ticksSinceHeal > this.Props.healIntervalTicks;
            if (flag)
            {
                bool flag2 = base.Pawn.health.hediffSet.HasNaturallyHealingInjury();
                if (flag2)
                {
                    this.ticksSinceHeal = 0;
                    float num = 8f;
                    Hediff_Injury hediff_Injury = GenCollection.RandomElement<Hediff_Injury>(from x in base.Pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                                                                                             where HediffUtility.CanHealNaturally(x)
                                                                                             select x);
                    hediff_Injury.Heal(num * base.Pawn.HealthScale * 0.01f);
                    string text = string.Format("{0} healed.", base.Pawn.LabelCap);
                }
            }
        }

        // Token: 0x04000002 RID: 2
        public int ticksSinceHeal;
    }
}
