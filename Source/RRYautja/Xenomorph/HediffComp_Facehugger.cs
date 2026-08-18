using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RRYautja
{
    public class HediffCompProperties_XenoFacehugger : HediffCompProperties
    {
        // Token: 0x06004C0D RID: 19469 RVA: 0x00237094 File Offset: 0x00235494
        public HediffCompProperties_XenoFacehugger()
        {
            this.compClass = typeof(HediffComp_XenoFacehugger);
        }

        // Token: 0x040033C3 RID: 13251
        public bool killHost = false;
        public float severityPerDay;
    }
    // Token: 0x02000D5B RID: 3419
    public class HediffComp_XenoFacehugger : HediffComp, IThingHolder
    {
        // Token: 0x17000BE6 RID: 3046
        // (get) Token: 0x06004C0F RID: 19471 RVA: 0x002370CE File Offset: 0x002354CE
        public HediffCompProperties_XenoFacehugger Props
        {
            get
            {
                return (HediffCompProperties_XenoFacehugger)this.props;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref this.royaleHugger, "royaleHugger", false);
            Scribe_Values.Look<bool>(ref this.killhugger, "killhugger");
            Scribe_Values.Look<int>(ref this.previousImpregnations, "previousImpregnations", 0);
            Scribe_Defs.Look<PawnKindDef>(ref this.instigatorKindDef, "InstigatorKindDef");
            Scribe_Defs.Look<HediffDef>(ref this.heDiffDeff, "heDiffDeff");
            Scribe_Deep.Look<Pawn>(ref this.instigator, "instigator");//, Props.pawn);
        }

        private string FacehuggerTexpath = "Things/Pawn/Xenomorph/Xenomorph_FaceHugger_Mask";
        private string RoyalhuggerTexpath = "Things/Pawn/Xenomorph/Xenomorph_FaceHuggerRoyal_Mask";

        public PawnKindDef HuggerKindDef { get { return XenomorphDefOf.RRY_Xenomorph_FaceHugger; } }
        public PawnKindDef RoyaleKindDef { get { return XenomorphDefOf.RRY_Xenomorph_RoyaleHugger; } }

        public PawnKindDef instigatorKindDef;
        private HediffDef _heDiffDeff; public HediffDef heDiffDeff { get { return _heDiffDeff ?? (_heDiffDeff = XenomorphDefOf.RRY_XenomorphImpregnation); } set { _heDiffDeff = value; } }
        public int timer = 0;
        public int timer2 = 0;
        public int previousImpregnations;
        public bool isImpregnated = false;
        public bool hasImpregnated = false;
        public DamageInfo dInfo;
        private ThingOwner innerContainer;
        protected bool contentsKnown;
        public bool killHost = false;
        public float severityPerDay;
        public bool royaleHugger;
        public Pawn instigator;
        public bool killhugger = false;

        public HediffComp_XenoFacehugger()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        // Token: 0x060024F3 RID: 9459 RVA: 0x00116D17 File Offset: 0x00115117
        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        // Token: 0x060024F4 RID: 9460 RVA: 0x00116D1F File Offset: 0x0011511F
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public Pawn Instigator
        {
            get
            {
                return instigator;
            }
        }
        public PawnKindDef InstigatorKindDef
        {
            get
            {
                return instigatorKindDef;
            }
        }

        public bool RoyaleHugger
        {
            get
            {
                return royaleHugger;
            }
        }

        public bool spawnLive
        {
            get
            {
                return RoyaleHugger && (previousImpregnations < maxImpregnations);
            }
        }

        public string TexPath
        {
            get
            {
                return RoyaleHugger ? RoyalhuggerTexpath : FacehuggerTexpath;
            }
        }

        public int maxImpregnations
        {
            get
            {
                if (RoyaleHugger)
                {
                    return 2;
                }
                return 1;
            }
        }

        public PawnKindDef pawnKindDef
        {
            get
            {
                return RoyaleHugger ? RoyaleKindDef : HuggerKindDef;
            }
        }

        public Thing ContainedThing
        {
            get
            {
                return (this.innerContainer.Count != 0) ? this.innerContainer[0] : null;
            }
        }


        public IThingHolder ParentHolder => this.Pawn;

        public virtual bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true);
        }
        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
                flag = true;
            }
            else
            {
                flag = this.innerContainer.TryAdd(thing, true);
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    this.contentsKnown = true;
                }
                return true;
            }
            return false;
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            string tex = TexPath;
            base.CompPostTick(ref severityAdjustment);
            if (base.Pawn.IsHashIntervalTick(200))
            {
                float num = this.SeverityChangePerDay();
                num *= 0.00333333341f;
                severityAdjustment += num;
            }
            if ((this.parent.CurStageIndex == 1 || this.parent.CurStage.label == "impregnation") && !this.parent.pawn.health.hediffSet.HasHediff(heDiffDeff))
            {
                timer++;
                if (timer >= 600 && !isImpregnated)
                {
#if DEBUG
                //    Log.Message("pre impreg checking Severity");
#endif
                    if (Rand.Chance(this.parent.Severity))
                    {
#if DEBUG
                    //    Log.Message("adding embryo");
#endif
                        parent.pawn.health.AddHediff(heDiffDeff, parent.pawn.RaceProps.body.corePart);
                        Hediff hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(heDiffDeff);
                        HediffComp_XenoSpawner _XenoSpawner = hediff.TryGetComp<HediffComp_XenoSpawner>();
                        _XenoSpawner.royaleHugger = RoyaleHugger;
                        _XenoSpawner.Impregnations = previousImpregnations;
                        hasImpregnated = true;
                        if (base.Pawn.health.hediffSet.HasHediff(heDiffDeff) && !isImpregnated)
                        {
                            isImpregnated = true;
                            previousImpregnations++;
                        }
                    }
                    timer = 0;
                }
            }
            else if ((this.parent.CurStageIndex == 2 || this.parent.CurStage.label == "post impregnation"))
            {
                timer2++;
                if (timer2 >= 600)
                {
#if DEBUG
                //    Log.Message("post impreg checking Severity");
#endif
                    if (Rand.Chance(this.parent.Severity))
                    {
#if DEBUG
                    //    Log.Message("removing Facehugger");
#endif
                        Pawn.health.hediffSet.hediffs.Remove(this.parent);
                        this.CompPostPostRemoved();

                    }
                    timer2 = 0;
                }
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            Pawn.health.RemoveHediff(this.parent);
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Facehuggers) && Pawn.Spawned && Pawn.IsColonist)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Facehuggers, OpportunityType.Important);
            }
        }

        public override void CompPostPostRemoved()
        {
            Thing hostThing = Pawn;
            Pawn hostPawn = Pawn;
            Map spawnMap = !Pawn.Dead ? Pawn.Map : Pawn.MapHeld;
            IntVec3 spawnLoc = !Pawn.Dead ? Pawn.Position : Pawn.PositionHeld;
            foreach (IntVec3 loc in GenRadial.RadialCellsAround(spawnLoc, 1, false))
            {
                if (loc.Standable(spawnMap))
                {
                    spawnLoc = loc;
                    Rand.Chance(0.5f);
                    break;
                }
            }
            bool spawnLive = this.spawnLive;
            hostPawn.health.AddHediff(XenomorphDefOf.RRY_Hediff_Anesthetic);
        //    if ((hostPawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_XenomorphImpregnation) && !hasImpregnated))
            if (!hasImpregnated)
            {
            spawnLive = true;
            }
            Pawn pawn;
            if (Instigator != null)
            {
            //    Log.Message("using instigator");
                pawn = instigator;
            }
            else
            {
                if (this.innerContainer.Any(x=>x is Pawn))
                {
                //    Log.Message("using innerContainer");
                    pawn = (Pawn)this.innerContainer.First(x => x is Pawn);
                }
                else
                {
                //    Log.Message("using PawnGenerator");
                    PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest(pawnKindDef, null, PawnGenerationContext.NonPlayer, -1, true, false, true, false, true, 0f, true);
                    pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
                }
            }
            if (spawnLive == true)
            {
            //    Log.Message("using spawnLive");
                Comp_Facehugger _Facehugger = pawn.TryGetComp<Comp_Facehugger>();
                if (_Facehugger!=null)
                {
                    _Facehugger.Impregnations = previousImpregnations;
                }
                if (!pawn.Spawned)
                {
                    GenSpawn.Spawn(pawn, spawnLoc, spawnMap, WipeMode.Vanish);
                }
                pawn.jobs.ClearQueuedJobs();
            //    pawn.jobs.curJob = new Verse.AI.Job(JobDefOf.FleeAndCower, hostPawn);
                if (killhugger)
                {
                    pawn.Kill(null);
                }
            }
            else
            {
            //    Log.Message("using spawnDead");
                if (!pawn.Spawned)
                {
                    GenSpawn.Spawn(pawn, spawnLoc, spawnMap, WipeMode.Vanish);
                }
                Comp_Facehugger _Facehugger = pawn.TryGetComp<Comp_Facehugger>();
            //    pawn.jobs.ClearQueuedJobs();
            //    pawn.jobs.curJob = new Verse.AI.Job(JobDefOf.FleeAndCower, hostPawn);
                _Facehugger.Impregnations = previousImpregnations;
                if (killhugger)
                {
                    pawn.Kill(null);
                }
                // pawn.Kill(null);
            }
            string text = TranslatorFormattedStringExtensions.Translate("Xeno_Facehugger_Detach", base.parent.pawn.LabelShort);
            if (!base.Pawn.Dead) MoteMaker.ThrowText(spawnLoc.ToVector3(), spawnMap, text, 5f);
        }

        // Token: 0x06004C89 RID: 19593 RVA: 0x002379D6 File Offset: 0x00235DD6
        protected virtual float SeverityChangePerDay()
        {
            return this.Props.severityPerDay;
        }

        // Token: 0x06004C8A RID: 19594 RVA: 0x002379E4 File Offset: 0x00235DE4
        public override string CompDebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompDebugString());
            if (!base.Pawn.Dead)
            {
                stringBuilder.AppendLine("severity/day: " + this.SeverityChangePerDay().ToString("F3"));
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        // Token: 0x04003401 RID: 13313
        protected const int SeverityUpdateInterval = 200;
    }

}
