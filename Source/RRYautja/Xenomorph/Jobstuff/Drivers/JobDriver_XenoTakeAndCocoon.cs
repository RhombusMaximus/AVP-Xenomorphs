using RRYautja;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x02000082 RID: 130
    public class JobDriver_XenoTakeAndCocoon : JobDriver
    {
        // Token: 0x170000B6 RID: 182
        // (get) Token: 0x06000371 RID: 881 RVA: 0x0001EEFC File Offset: 0x0001D2FC
        protected Pawn Takee
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public HediffDef heCocDeff { get { return XenomorphDefOf.RRY_Hediff_Cocooned; } }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.useDuration = (int)(300 * Takee.BodySize);
        }

        // Token: 0x06000372 RID: 882 RVA: 0x0001EF20 File Offset: 0x0001D320
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.job.targetA;
            Job job = this.job;
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, ReservationLayerDefOf.Floor);
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x06000373 RID: 883 RVA: 0x0001EF58 File Offset: 0x0001D358
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            yield return carryToCell;
            /*
            yield return new Toil
            {
                initAction = delegate ()
                {
                    if (this.pawn.Position.OnEdge(this.pawn.Map) || this.pawn.Map.exitMapGrid.IsExitCell(this.pawn.Position))
                    {
                        this.pawn.ExitMap(true, CellRect.WholeMap(base.Map).GetClosestEdge(this.pawn.Position));
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            */
        //    yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
            if (Takee == null || Takee.Dead)
            {
                yield break;
            }
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, false);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.A);
            prepare.NPCWithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        //    prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            prepare.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
            prepare.PlaySustainerOrSound(() => SoundDefOf.Vomit);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                Pawn Infectable = (Pawn)actor.CurJob.targetA.Thing;
                Infectable.health.AddHediff(heCocDeff);

            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }
        
        private int useDuration = -1;
    }
}
