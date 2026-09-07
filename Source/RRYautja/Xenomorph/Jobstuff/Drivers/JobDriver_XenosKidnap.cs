using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x02000072 RID: 114
    public class JobDriver_XenosKidnap : JobDriver_XenoTakeAndCocoon
    {
        // Token: 0x06000325 RID: 805 RVA: 0x0001F264 File Offset: 0x0001D664
        public override string GetReport()
        {
            if (this.Takee == null || this.pawn.HostileTo(this.Takee))
            {
                return base.GetReport();
            }
            return JobDefOf.Rescue.reportString.Replace("TargetA", this.Takee.LabelShort);
        }

        // Token: 0x06000326 RID: 806 RVA: 0x0001F2B8 File Offset: 0x0001D6B8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => this.Takee == null || this.Takee.Dead || (!this.Takee.Downed && this.Takee.Awake() && this.Takee.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) >= 0.2f));
            foreach (Toil t in base.MakeNewToils())
            {
                yield return t;
            }
            yield break;
        }
    }
}
