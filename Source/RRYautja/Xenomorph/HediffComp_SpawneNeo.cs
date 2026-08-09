using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000D5A RID: 3418
    public class HediffCompProperties_NeoSpawner : HediffCompProperties
    {
        // Token: 0x06004C0D RID: 19469 RVA: 0x00237094 File Offset: 0x00235494
        public HediffCompProperties_NeoSpawner()
        {
            this.compClass = typeof(HediffComp_NeoSpawner);
        }

        // Token: 0x040033C3 RID: 13251
        public PawnKindDef pawnKindDef;
        public float severityPerDay;
    }
    public class HediffComp_NeoSpawner : HediffComp
    {
        public HediffCompProperties_NeoSpawner Props
        {
            get
            {
                return (HediffCompProperties_NeoSpawner)this.props;
            }
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref this.lastCoughTick, "thislastCoughTick");
            Scribe_Values.Look<int>(ref this.lastCoughStage, "thislastCoughStage");
            Scribe_Values.Look<int>(ref this.timesCoughed, "thistimesCoughed");
            Scribe_Values.Look<int>(ref this.timesCoughedBlood, "thistimesCoughedBlood");
            Scribe_Values.Look<float>(ref this.lastCoughSeverity, "thislastCoughSeverity");
            Scribe_Values.Look<int>(ref this.spawnCount, "spawnCount");
        }

        public int spawnCount = 0;
        bool logonce = false;
        int lastCoughTick = 0;
        int nextCoughTick = 0; 
        int lastCoughStage=0;
        int timesCoughed = 0;
        int timesCoughedBlood = 0;
        float lastCoughSeverity=0;

        List<string> Coughlist = new List<string>
        {
            "quietly",
            "abruptly",
            "loudly",
            "painfully",
            "violently"
        };

        public IntVec3 MyPos
        {
            get
            {
                return Pawn.Position != null ? Pawn.Position : Pawn.PositionHeld;
            }
        }

        public Map MyMap
        {
            get
            {
                return Pawn.Map ?? Pawn.MapHeld;
            }
        }

        public int countToSpawn
        {
            get
            {
                if (spawnCount==0)
                {
                    spawnCount = Rand.RangeSeeded(1, (int)Pawn.BodySize, AvPConstants.AvPSeed);
                }
                return spawnCount;
            }
            set
            {
                spawnCount = value;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            DeathActionWorker daw = this.Pawn.def.race.DeathActionWorker;
            this.Pawn.def.race.deathAction.workerClass = typeof(DeathActionWorker_Simple);
            if (parent.pawn.Spawned)
            {
                if (parent.pawn.Map != null)
                {
                    if (parent.def == XenomorphDefOf.RRY_HiddenNeomorphImpregnation || parent.def == XenomorphDefOf.RRY_NeomorphImpregnation && this.parent.pawn.Faction == Faction.OfPlayer)
                    {
                        string text = TranslatorFormattedStringExtensions.Translate("Xeno_Neospores_Added", base.parent.pawn.LabelShortCap, parent.Part.LabelShort);
                        MoteMaker.ThrowText(base.parent.pawn.Position.ToVector3(), base.parent.pawn.Map, text, 3f);
                    }

                    if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Embryo) && Pawn.Spawned && Pawn.IsColonist && (this.parent.def == XenomorphDefOf.RRY_XenomorphImpregnation || this.parent.def == XenomorphDefOf.RRY_NeomorphImpregnation) && MyMap != null)
                    {
                        LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Embryo, OpportunityType.Important);
                    }
                }

            }
        }

        public void DoNeoCough()
        {
            lastCoughSeverity = parent.Severity;
            lastCoughStage = parent.CurStageIndex;
            lastCoughTick = parent.ageTicks; // Find.TickManager.TicksGame; // parent.ageTicks; //
            nextCoughTick = lastCoughTick + Rand.RangeInclusiveSeeded(3000, 24000, AvPConstants.AvPSeed);
            float chance = ((0.25f + lastCoughSeverity)) + (((timesCoughedBlood*2) + timesCoughed)/10);
            if (Rand.ChanceSeeded(chance, AvPConstants.AvPSeed))
            {
                if (this.parent.pawn.Faction == Faction.OfPlayer)
                {
                    string text = TranslatorFormattedStringExtensions.Translate("Xeno_Neospores_Cough", base.parent.pawn.LabelShortCap, Coughlist[Rand.RangeInclusiveSeeded(timesCoughed, Coughlist.Count, AvPConstants.AvPSeed)]);
                    //    Log.Message(text);
                    MoteMaker.ThrowText(base.parent.pawn.Position.ToVector3(), base.parent.pawn.Map, text, 3f);
                }
                if (Rand.ChanceSeeded(chance, AvPConstants.AvPSeed))
                {
                    if (this.parent.pawn.Faction == Faction.OfPlayer)
                    {
                        string text = TranslatorFormattedStringExtensions.Translate("Xeno_Neospores_Cough_Blood");
                        //    Log.Message(text);
                        MoteMaker.ThrowText(base.parent.pawn.Position.ToVector3(), base.parent.pawn.Map, text, 3f);
                    }
                    parent.pawn.health.DropBloodFilth();
                    timesCoughedBlood++;
                }
                else
                {
                    timesCoughed++;
                }
                
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            bool selected = Find.Selector.SingleSelectedThing == parent.pawn;
            if (parent.CurStageIndex >= parent.def.stages.Count - 3 && this.Pawn.Map == null) return;
            base.CompPostTick(ref severityAdjustment);
            if (base.Pawn.IsHashIntervalTick(200))
            {
                float num = this.SeverityChangePerDay();
                num *= 0.00333333341f;
                severityAdjustment += num;
            }
            if (parent.ageTicks> nextCoughTick && Pawn.Map != null && Pawn.Spawned)
            {
                DoNeoCough();

            }

            if (parent.CurStageIndex == parent.def.stages.Count - 2)
            {
                if (parent.Part != parent.pawn.RaceProps.body.corePart) parent.Part = parent.pawn.RaceProps.body.corePart;
                if (!this.logonce && this.parent.pawn.Downed)
                {
                    if (parent.pawn.InBed()&&parent.pawn.CurrentBed()is Building_XenomorphCocoon)
                    {
                        Pawn.CurrentBed().Destroy();
                    }
                    string text = TranslatorFormattedStringExtensions.Translate("Xeno_Chestburster_PreEmerge", this.Pawn.LabelShort);
                //    Log.Message(text);
                    MoteMaker.ThrowText(base.parent.pawn.Position.ToVector3(), base.parent.pawn.Map, text, 5f);
                    this.logonce = true;

                }
#if DEBUG
            //    if (selected) Log.Message(string.Format("Pre Death stage: {0}", parent.CurStage.label));
#endif
                int num = Find.TickManager.TicksGame % 300 * 2;
#if DEBUG
            //    if (selected) Log.Message(string.Format("num: {0}", num));
#endif
                if (num < 90)
                {
                    Pawn.Drawer.renderer.wiggler.downedAngle += 0.35f;
                }
                else if (num < 390 && num >= 300)
                {
                    Pawn.Drawer.renderer.wiggler.downedAngle -= 0.35f;
                }
                else if (num < 270 && num >= 180)
                {
                    Pawn.Drawer.renderer.wiggler.downedAngle += 0.35f;
                }
                else if (num < 570 && num >= 510)
                {
                    Pawn.Drawer.renderer.wiggler.downedAngle -= 0.35f;
                }
            }
        }

        public Pawn NeomorphSpawnRequest()
        {
            bool selected = Find.Selector.SingleSelectedThing == parent.pawn && Prefs.DevMode;
            Gender gender = Gender.None;
            PawnKindDef pawnKindDef = XenomorphDefOf.RRY_Xenomorph_Neomorph;
            if (Prefs.DevMode)
            {
             //    Log.Message(string.Format("spawning: {0}", pawnKindDef.label));
                parent.pawn.resultingXenomorph();
            }
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, Find.FactionManager.FirstFactionOfDef(pawnKindDef.defaultFactionDef), PawnGenerationContext.NonPlayer, -1, true, true, false, false, true, 20f, false, fixedGender: gender);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            return pawn;
        }

        public Color HostBloodColour
        {
            get
            {
                Color color = base.parent.pawn.def.race.BloodDef.graphic.color;
                color.a = 1f;
                return color;
            }
        } 

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            IntVec3 spawnLoc = !base.Pawn.Dead ? base.parent.pawn.Position : base.parent.pawn.PositionHeld;
            Map spawnMap = !base.Pawn.Dead ? base.parent.pawn.Map : base.parent.pawn.MapHeld;
            this.Pawn.def.race.deathAction.workerClass = typeof(DeathActionWorker_Simple);
            bool fullterm = this.parent.CurStageIndex >= this.parent.def.stages.Count - 3;
            if (!fullterm)
            {
            //    Log.Message(string.Format("died  before reaching fullterm, no spawning"));
                return;
            }
            else
            {
                if (spawnMap == null || spawnLoc == null)
                {
                //    Log.Message(string.Format("spawnMap or spawnLoc is null, no spawning"));
                    return;
                }
                else
                {
                    for (int i = 0; i < countToSpawn; i++)
                    {
                        Pawn pawn = NeomorphSpawnRequest();
                    //    Log.Message(string.Format("Xenomorph to hatch: {0}", pawn.LabelShortCap));
                        pawn.ageTracker.CurKindLifeStage.bodyGraphicData.colorTwo = HostBloodColour;
                        pawn.Notify_ColorChanged();
                        pawn.ageTracker.AgeBiologicalTicks = 0;
                        pawn.ageTracker.AgeChronologicalTicks = 0;
                        pawn.needs.food.CurLevel = 0;
                        Comp_Xenomorph _Xenomorph = pawn.TryGetComp<Comp_Xenomorph>();
                        if (_Xenomorph != null)
                        {
                            _Xenomorph.host = base.parent.pawn.kindDef;
                        }
                        GenSpawn.Spawn(pawn, spawnLoc, spawnMap, 0);
                    }
                    Vector3 vector = spawnLoc.ToVector3Shifted();
                    for (int i = 0; i < 101; i++)
                    {
                        if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 2f, 3.TicksToSeconds()))
                        {
                            FleckMaker.ThrowDustPuffThick(new Vector3(vector.x, 0f, vector.z)
                            {
                                y = AltitudeLayer.MoteOverhead.AltitudeFor()
                            }, spawnMap, 1.5f, new Color(HostBloodColour.r, HostBloodColour.g, HostBloodColour.b, HostBloodColour.a));
                        }
                        if (i == 100)
                        {

                        }
                        if (i % 10 == 0)
                        {
                            FilthMaker.TryMakeFilth(spawnLoc + GenAdj.AdjacentCellsAndInside.RandomElement(), this.Pawn.MapHeld, this.Pawn.RaceProps.BloodDef, this.Pawn.LabelIndefinite(), 1);
                        }
                    }
                    ThingDef motedef = DefDatabase<ThingDef>.GetNamedSilentFail("Mote_BlastExtinguisher");
                    MoteMaker.ThrowExplosionCell(spawnLoc, MyMap, motedef, HostBloodColour);
                    // GenAdj.AdjacentCellsAndInside[i];
                    for (int i2 = 0; i2 < GenAdj.AdjacentCellsAndInside.Length; i2++)
                    {
                        FilthMaker.TryMakeFilth(spawnLoc + GenAdj.AdjacentCellsAndInside[i2], this.Pawn.MapHeld, this.Pawn.RaceProps.BloodDef, this.Pawn.LabelIndefinite(), 1);
                    }
                    string text = TranslatorFormattedStringExtensions.Translate("Xeno_Chestburster_Emerge", base.parent.pawn.LabelShort, this.parent.Part.LabelShort);
                    MoteMaker.ThrowText(spawnLoc.ToVector3(), spawnMap, text, 5f);
                    if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Chestbursters) && MyMap != null)
                    {
                        LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Chestbursters, OpportunityType.Important);
                    }
                    Pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamedSilentFail("RRY_PostBurstWound"), this.parent.Part);
                    Pawn.health.RemoveHediff(this.parent);
                }
            }
             
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

        private static float DustMoteSpawnMTB = 0.2f;
    }
}
