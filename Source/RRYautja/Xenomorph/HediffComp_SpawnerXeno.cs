using RimWorld;
using RRYautja.ExtensionMethods;
using RRYautja.settings;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using System.Linq;

namespace RRYautja
{
    // Token: 0x02000D5A RID: 3418
    public class HediffCompProperties_XenoSpawner : HediffCompProperties
    {
        // Token: 0x06004C0D RID: 19469 RVA: 0x00237094 File Offset: 0x00235494
        public HediffCompProperties_XenoSpawner()
        {
            this.compClass = typeof(HediffComp_XenoSpawner);
        }
        
        public PawnKindDef pawnKindDef;
        public List<PawnKindDef> pawnKindDefs;
        public List<float> pawnKindWeights;
        public float severityPerDay;
    }

    public class HediffComp_XenoSpawner : HediffComp
    {
        public HediffCompProperties_XenoSpawner Props
        {
            get
            {
                return (HediffCompProperties_XenoSpawner)this.props;
            }
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref this.Impregnations, "Impregnations", 0);
            Scribe_Values.Look<int>(ref this.countToSpawn, "countToSpawn", 1);
            Scribe_Values.Look<bool>(ref this.royaleHugger, "royaleHugger");
            Scribe_Values.Look<bool>(ref this.bursted, "bursted");
            Scribe_Values.Look<bool>(ref this.predalienImpregnation, "predalienImpregnation",false);
        }

        bool logonce = false;
        bool bursted = false;
        public int countToSpawn = 1;
        public bool royaleHugger;
        public bool predalienImpregnation;
        public int Impregnations;

        public bool RoyaleHugger
        {
            get
            {
                return royaleHugger;
            }
        }

        public bool RoyaleEmbryo
        {
            get
            {
            //    Log.Message(string.Format("RoyaleHugger: {0} && Impregnations: {1} < maxImpregnations: {2} == {3}", RoyaleHugger, Impregnations, maxImpregnations, RoyaleHugger && Impregnations < maxImpregnations));
                return RoyaleHugger && Impregnations < maxImpregnations;
            }
        }

        public int maxImpregnations
        {
            get
            {
                if (RoyaleHugger)
                {
                    return 1;
                }
                return 0;
            }
        }
        
        public IntVec3 MyPos
        {
            get
            {
                return Pawn.Position != null ? Pawn.Position : Pawn.Position;
            }
        }

        public Map MyMap
        {
            get
            {
                return Pawn.Map ?? Pawn.MapHeld;
            }
        }

        public bool RoyalPresent
        {
            get
            {
                bool selected = Find.Selector.SelectedObjects.Contains(Pawn) && Prefs.DevMode;
                Func<Pawn, bool> validator = delegate (Pawn t)
                {
                    bool SelfFlag = t != Pawn;
                    bool RoyalHuggerInfection = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection).TryGetComp<HediffComp_XenoFacehugger>().RoyaleHugger);
                    bool RoyalImpregnation = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_XenomorphImpregnation) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_XenomorphImpregnation).TryGetComp<HediffComp_XenoSpawner>().RoyaleHugger);
                    bool RoyalHiddenImpregnation = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_HiddenXenomorphImpregnation) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_HiddenXenomorphImpregnation).TryGetComp<HediffComp_XenoSpawner>().RoyaleHugger);
                    return SelfFlag || RoyalHuggerInfection || RoyalImpregnation || RoyalHiddenImpregnation;
                };
                return MyMap.mapPawns.AllPawnsSpawned.Any((Func<Pawn, bool>)validator);
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
                    if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Embryo) && Pawn.Spawned && Pawn.IsColonist && (this.parent.def == XenomorphDefOf.RRY_XenomorphImpregnation || this.parent.def == XenomorphDefOf.RRY_NeomorphImpregnation) && MyMap != null)
                    {
                        LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Embryo, OpportunityType.Important);
                    }
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

        public Pawn XenomorphSpawnRequest()
        {
            // Check for queen on map
            bool queenPresent = MyMap?.mapPawns.AllPawnsSpawned.Any(p => p.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen) ?? false;

            // Find the first matching spawn def (sorted by priority)
            PawnKindDef pawnKindDef = null;
            foreach (var def in DefDatabase<XenomorphSpawnDef>.AllDefs.OrderBy(d => d.priority))
            {
                if (def.Matches(Pawn, RoyaleEmbryo, predalienImpregnation, queenPresent))
                {
                    pawnKindDef = def.PickKind();
                    if (pawnKindDef != null)
                    {
                        AvPDebug.Log("Spawn", $"{Pawn.LabelShort} -> {pawnKindDef.LabelCap} via {def.defName} (host: {Pawn.def.label}, bodySize: {Pawn.BodySize})");
                        break;
                    }
                }
            }

            // Fallback
            if (pawnKindDef == null)
            {
                pawnKindDef = XenomorphDefOf.RRY_Xenomorph_Drone;
                AvPDebug.Warning($"No matching XenomorphSpawnDef for {Pawn.LabelShort}, falling back to Drone");
            }

            // Generate the pawn
            Gender gender = pawnKindDef == XenomorphDefOf.RRY_Xenomorph_Queen ? Gender.Female : Gender.None;
            Faction xenoFaction = Find.FactionManager?.FirstFactionOfDef(pawnKindDef.defaultFactionDef);
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, xenoFaction,
                PawnGenerationContext.NonPlayer, -1, true, true, false, false, true, 20f, false, fixedGender: gender);

            Pawn pawn = PawnGenerator.GeneratePawn(request);

            // Store host race info on Comp_Xenomorph (replaces old XenomorphPawn.HostRace)
            Comp_Xenomorph _Xenomorph = pawn.TryGetComp<Comp_Xenomorph>();
            if (_Xenomorph != null)
            {
                _Xenomorph.host = Pawn.kindDef;
                _Xenomorph.HostDef = Pawn.def;
            }

            return pawn;
        }

        public Color HostBloodColour
        {
            get
            {
                Color color = Pawn.def.race.BloodDef.graphic.color;
                color.a = 1f;
                return color;
            }
        } 

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            IntVec3 spawnLoc = !base.Pawn.Dead ? base.parent.pawn.Position : base.parent.pawn.PositionHeld;
            Map spawnMap = !base.Pawn.Dead ? base.parent.pawn.Map : base.parent.pawn.MapHeld;
            this.Pawn.def.race.deathAction.workerClass = typeof(DeathActionWorker_Simple);
            bool fullterm = this.parent.CurStageIndex > this.parent.def.stages.Count - 3;
            if (!fullterm || bursted || spawnMap == null || spawnLoc == null)
            {
                return;
            }
            else
            {
                bursted = true;
                if (spawnMap == null || spawnLoc == null)
                {
                    return;
                }
                else
                {
                    if (countToSpawn == 0) countToSpawn++;
                    for (int i = 0; i < countToSpawn; i++)
                    {
                        Pawn pawn = XenomorphSpawnRequest();
                        // Ensure faction is set
                        if (pawn.Faction == null)
                        {
                            Faction xenoFaction = Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph);
                            if (xenoFaction != null)
                            {
                                pawn.SetFaction(xenoFaction);
                            }
                        }
                        pawn.ageTracker.AgeBiologicalTicks = 0;
                        pawn.ageTracker.AgeChronologicalTicks = 0;
                        Comp_Xenomorph _Xenomorph = pawn.TryGetComp<Comp_Xenomorph>();
                        if (_Xenomorph != null)
                        {
                            _Xenomorph.host = base.parent.pawn.kindDef;
                        }
                        GenSpawn.Spawn(pawn, spawnLoc, spawnMap, WipeMode.Vanish);
                        AvPDebug.Log("Spawn", "Chestburster spawned: " + pawn.LabelShort + " faction=" + (pawn.Faction?.Name ?? "null"));
                        // Assign a Lord so chestburster-spawned Xenos don't all do the same job
                        var existingLord = spawnMap.lordManager.lords.FirstOrDefault(l => l.faction == pawn.Faction);
                        if (existingLord != null)
                        {
                            existingLord.AddPawn(pawn);
                        }
                        else
                        {
                            var newLord = Verse.AI.Group.LordMaker.MakeNewLord(pawn.Faction, new LordJob_DefendAndExpandHiveLike(false), spawnMap, null);
                            newLord.AddPawn(pawn);
                        }
                    }
                    Vector3 vector = spawnLoc.ToVector3Shifted();
                    for (int i = 0; i < 101; i++)
                    {
                        if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 2f, 3.TicksToSeconds()))
                        {
                            FleckMaker.ThrowDustPuffThick(new Vector3(vector.x, 0f, vector.z)
                            {
                                y = AltitudeLayer.MoteOverhead.AltitudeFor()
                            }, spawnMap, 1.5f, HostBloodColour);
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
                    if (motedef != null && MyMap != null)
                    {
                        try { MoteMaker.ThrowExplosionCell(spawnLoc, MyMap, motedef, HostBloodColour); } catch { }
                    }
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
            if (RoyaleHugger)
            {
                return this.Props.severityPerDay/5;
            }
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
