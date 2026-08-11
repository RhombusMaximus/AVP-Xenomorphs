using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RRYautja
{
    // Token: 0x0200000C RID: 12
    public class JobDriver_XenomorphTendSelf : JobDriver
    {
        // Token: 0x06000025 RID: 37 RVA: 0x00002690 File Offset: 0x00000890
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000026A3 File Offset: 0x000008A3
        protected override IEnumerable<Toil> MakeNewToils()
        {
            base.AddEndCondition(delegate ()
            {
                if (this.pawn.Faction == Faction.OfPlayer && HealthAIUtility.ShouldBeTendedNowByPlayer(this.pawn))
                {
                    return JobCondition.Ongoing;
                }
                if (this.pawn.Faction != Faction.OfPlayer && this.pawn.health.HasHediffsNeedingTend(false))
                {
                    return JobCondition.Ongoing;
                }
                return JobCondition.Succeeded;
            });
            int ticks = (int)(1f / StatExtension.GetStatValue(this.pawn, StatDefOf.MedicalTendSpeed, true) * 600f);
            yield return ToilEffects.PlaySustainerOrSound(ToilEffects.WithProgressBarToilDelay(Toils_General.Wait(ticks, 0), (TargetIndex)1, false, -0.5f), SoundDefOf.Interact_Tend);
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                Pawn actor = toil.actor;
                float num = (!actor.RaceProps.Animal) ? 500f : 175f;
                float num2 = 0.5f;
                actor.skills.Learn(SkillDefOf.Medicine, num * num2, false);
                HealthShardTendUtility.DoTend(actor, actor, null);
            };
            toil.defaultCompleteMode = (ToilCompleteMode)1;
            yield return toil;
            yield break;
        }
    }
}
