using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using System.Linq;

namespace RRYautja
{
    // Token: 0x02000005 RID: 5
    public class CompProperties_MedicalInjector : CompProperties
    {
        // Token: 0x06000015 RID: 21 RVA: 0x0000232F File Offset: 0x0000052F
        public CompProperties_MedicalInjector()
        {
            this.compClass = typeof(CompMedicalInjector);
        }

        // Token: 0x04000007 RID: 7
        public int Uses = 0;

        // Token: 0x04000008 RID: 8
        public ThingDef medicine;
    }
    // Token: 0x02000006 RID: 6
    public class CompMedicalInjector : ThingComp
    {
        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000016 RID: 22 RVA: 0x00002354 File Offset: 0x00000554
        public CompProperties_MedicalInjector Props
        {
            get
            {
                return (CompProperties_MedicalInjector)this.props;
            }
        }
    }

    // Token: 0x02000791 RID: 1937
    public class CompUseEffect_HealthShard : CompUseEffect
    {
        // Token: 0x06002ADC RID: 10972 RVA: 0x001433C0 File Offset: 0x001417C0
        public override void DoEffect(Pawn user)
        {
            Cloakgen injector = (Cloakgen)user.apparel.WornApparel.Find((Apparel x) => x.def.defName.Contains("RRY_Equipment_HunterGauntlet"));
            CompMedicalInjector medicalInjector = injector.TryGetComp<CompMedicalInjector>();
            bool selected = Find.Selector.SelectedObjects.Contains(user);
            int needed = medicalInjector.Props.Uses - injector.uses;
            if (needed>0)
            {
                if (this.parent.stackCount >= needed)
                {
                    injector.uses = medicalInjector.Props.Uses;
                    this.parent.stackCount = this.parent.stackCount - needed;
                }
                else if (this.parent.stackCount<needed)
                {

                    injector.uses = this.parent.stackCount;
                    this.parent.stackCount = 0;
                    this.parent.Destroy();
                }
            }
           
            //    base.DoEffect(user);

        }

        // Token: 0x06002ADD RID: 10973 RVA: 0x00143464 File Offset: 0x00141864
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            bool selected = Find.Selector.SelectedObjects.Contains(p);
            bool flag = GenCollection.Any<Apparel>(p.apparel.WornApparel, (Apparel x) => x.def.defName.Contains("RRY_Equipment_HunterGauntlet"));
            if (flag)
            {
                Cloakgen injector = (Cloakgen)p.apparel.WornApparel.Find((Apparel x) => x.def.defName.Contains("RRY_Equipment_HunterGauntlet"));
                if (injector!=null)
                {
                    CompMedicalInjector medicalInjector = injector.TryGetComp<CompMedicalInjector>();
                    if (injector.uses < medicalInjector.Props.Uses)
                    {
                        return true;
                    }
                    else
                    {
                        return "Injector full";
                    }
                }
                else
                {
                    return "Not wearing Injector";
                }
            }
            else
            {
                return "Not wearing Injector";
            }
        //    return base.CanBeUsedBy(p);
        }

        // Token: 0x04001786 RID: 6022
        private const float XPGainAmount = 50000f;
    }


    public class CompUsable_HealthShard : CompUsable
    {
        // (get) Token: 0x06002942 RID: 10562 RVA: 0x001394F0 File Offset: 0x001378F0
        protected override string FloatMenuOptionLabel(Pawn pawn)
        {
            return string.Format(base.Props.useLabel, this.parent.LabelCap);
        }

        // Token: 0x06002A4A RID: 10826 RVA: 0x00138F4C File Offset: 0x0013734C
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            AcceptanceReport canBeUsed = this.CanBeUsedBy(myPawn);
            if (!canBeUsed.Accepted)
            {
                //    yield break;
                string failReason = canBeUsed.Reason;
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + ((failReason == null) ? string.Empty : (" (" + failReason + ")")), null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.CanReserve(this.parent, 1, -1, null, false))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel(myPawn) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                FloatMenuOption useopt = new FloatMenuOption(this.FloatMenuOptionLabel(myPawn), delegate ()
                {
                    if (myPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                    {
                        foreach (CompUseEffect compUseEffect in this.parent.GetComps<CompUseEffect>())
                        {
                            if (compUseEffect.SelectedUseOption(myPawn))
                            {
                                return;
                            }
                        }
                        this.TryStartUseJob(myPawn, null);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return useopt;
            }
            yield break;
        }

        // Token: 0x06002A4D RID: 10829 RVA: 0x00139094 File Offset: 0x00137494
        private AcceptanceReport CanBeUsedBy(Pawn p)
        {
            List<ThingComp> allComps = this.parent.AllComps;
            for (int i = 0; i < allComps.Count; i++)
            {
                if (allComps[i] is CompUseEffect compUseEffect)
                {
                    AcceptanceReport result = compUseEffect.CanBeUsedBy(p);
                    if (!result.Accepted)
                    {
                        return result;
                    }
                }
            }
            return true;
        }

        // Token: 0x06002944 RID: 10564 RVA: 0x00139525 File Offset: 0x00137925
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.skill = DefDatabase<SkillDef>.GetRandom();
        }
        /*
        // Token: 0x06002945 RID: 10565 RVA: 0x00139539 File Offset: 0x00137939
        public override string TransformLabel(string label)
        {
            return this.skill.LabelCap + " " + label;
        }
        */
        // Token: 0x06002946 RID: 10566 RVA: 0x00139554 File Offset: 0x00137954
        public override bool AllowStackWith(Thing other)
        {
            return true;
        }

        // Token: 0x040016DD RID: 5853
        public SkillDef skill;
    }


    // Token: 0x02000089 RID: 137
    public class JobDriver_RestockHealthShards : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.useDuration = this.job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable_HealthShard>().Props.useDuration;
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
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.None);
            prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                CompUsable_HealthShard compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUsable_HealthShard>();
                compUsable.UsedBy(actor);
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = -1;
    }
}
