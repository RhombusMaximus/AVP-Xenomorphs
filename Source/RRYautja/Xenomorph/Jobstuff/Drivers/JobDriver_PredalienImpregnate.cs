using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RRYautja;

namespace RimWorld
{
    // Token: 0x02000089 RID: 137
    public class JobDriver_PredalienImpregnate : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 300, false);
        }

        public Pawn Victim
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public HediffDef heDiffDeff { get { return XenomorphDefOf.RRY_XenomorphImpregnation; } }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.useDuration = (int)(300 * Victim.BodySize);
        }

        // Token: 0x06000392 RID: 914 RVA: 0x00024590 File Offset: 0x00022990
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x06000393 RID: 915 RVA: 0x000245C8 File Offset: 0x000229C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.A);
            prepare.NPCWithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                Victim.health.AddHediff(heDiffDeff, Victim.RaceProps.body.corePart);
                Hediff hediff = Victim.health.hediffSet.GetFirstHediffOfDef(heDiffDeff);
                HediffComp_XenoSpawner _XenoSpawner = hediff.TryGetComp<HediffComp_XenoSpawner>();
                _XenoSpawner.countToSpawn = Rand.RangeInclusive(1, 4);
                _XenoSpawner.predalienImpregnation = true;

            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = -1;
    }
}
