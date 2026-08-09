using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RRYautja
{
    public class CompProperties_Stealth : CompProperties
    {
        public CompProperties_Stealth()
        {
            this.compClass = typeof(Comp_Stealth);
        }
        
    }

    public class Comp_Stealth : ThingComp
    {
        public CompProperties_Stealth Props
        {
            get
            {
                return (CompProperties_Stealth)this.props;
            }
        }

        private FieldInfo _shadowGraphic;
        private FieldInfo _graphicInt;
        private FieldInfo _lastCell;
        private object oldGraphics /* TODO: was PawnGraphicSet */;
        private Graphic_Shadow oldShadow;
        private int lastSpottedTick = -9999;
        private Graphic lastCarriedGraphic;
        private Thing lastCarried;
        public bool hidden = false;
        public bool Hidden = false;
        public PawnKindDef HuggerKindDef = XenomorphDefOf.RRY_Xenomorph_FaceHugger;
        public PawnKindDef RoyaleKindDef = XenomorphDefOf.RRY_Xenomorph_RoyaleHugger;

        public PawnKindDef QueenDef = XenomorphDefOf.RRY_Xenomorph_Queen;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastSpottedTick, "lastSpottedtick", -9999);
            Scribe_References.Look(ref lastCarried, "lastCarried");
            Scribe_Values.Look<bool>(ref this.hidden, "hidden");
            Scribe_Values.Look<bool>(ref this.Hidden, "Hidden");
        }
        
        public Pawn pawn
        {
            get
            {
                return (Pawn)parent;
            }
        }

        public Map map
        {
            get
            {
                return pawn.Map ?? pawn.MapHeld;
            }
        }

        public float MinHideDist
        {
            get
            {
                return Mathf.Max((10 * pawn.BodySize) * pawn.Map.glowGrid.GroundGlowAt(pawn.Position, false), (10 * pawn.BodySize));
            }
        }
        public bool spotted
        {
            get
            {
                if (map.mapPawns.AllPawnsSpawned.Any(x => GenSight.LineOfSight(pawn.Position, x.Position, map) && pawn.Position.DistanceTo(x.Position) > MinHideDist))
                {
                    return true;
                }
                return false;
            }
        }

        public override void CompTick()
        {
            if (pawn.ageTracker.CurLifeStage != XenomorphDefOf.RRY_XenomorphFullyFormed)
            {
                if (pawn.CurJobDef == JobDefOf.Ingest)
                {
                    hidden = false;
                }
                else if (pawn.CurJobDef != JobDefOf.Ingest)
                {
                    hidden = true;
                }
            }
            if (pawn.Faction == null)
            {
                if (Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph) != null)
                {
                    pawn.SetFaction(Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph));
                }
            }
            base.CompTick();
            if (Hidden)
            {
                if (!hidden)
                {
                    MakeInvisible();
                    hidden = true;
                }
                if (pawn.Downed || pawn.Dead || (pawn.pather != null && pawn.pather.WillCollideNextCell))
                {
                    MakeVisible();
                    hidden = false;
                    if (pawn.pather != null)
                    {
                        AlertXenomorph(pawn, pawn.pather.nextCell.GetFirstPawn(pawn.Map));
                    }
                    else
                    {
                        AlertXenomorph(pawn, null);
                    }
                }
                /*
                if (pawn.pather != null && GetLastCell(pawn.pather).GetDoor(pawn.Map) != null)
                {
                    GetLastCell(pawn.pather).GetDoor(pawn.Map).StartManualCloseBy(pawn);
                }
                */
                if (pawn.Map != null && lastSpottedTick < Find.TickManager.TicksGame - 125)
                {
                    lastSpottedTick = Find.TickManager.TicksGame;
                    int num = 0;
                    while (num < 20)
                    {
                        IntVec3 c = pawn.Position + GenRadial.RadialPattern[num];
                        Room room = RegionAndRoomQuery.RoomAt(c, pawn.Map);
                        if (c.InBounds(pawn.Map))
                        {
                            if (RegionAndRoomQuery.RoomAt(c, pawn.Map) == room)
                            {
                                List<Thing> thingList = c.GetThingList(pawn.Map);
                                foreach (Thing thing in thingList)
                                {
                                    Pawn observer = thing as Pawn;
                                    if (observer != null && observer != pawn && observer.Faction != null && (observer.Faction.HostileTo(pawn.Faction)))
                                    {
                                        float observerSight = observer.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
                                        observerSight *= 0.805f + (pawn.Map.glowGrid.GroundGlowAt(pawn.Position) / 4);
                                        if (observer.RaceProps.Animal)
                                        {
                                            observerSight *= 0.9f;
                                        }
                                        observerSight = Math.Min(2f, observerSight);
                                        float thiefMoving = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving);
                                        float spotChance = 0.8f * thiefMoving / observerSight;
                                        if (Rand.Value > spotChance)
                                        {
                                            MakeVisible();
                                            hidden = false;
                                            AlertXenomorph(pawn, observer);
                                        }
                                    }
                                    else if (observer == null)
                                    {
                                        if (thing is Building_Turret turret && turret.Faction != null && turret.Faction.IsPlayer)
                                        {
                                            float thiefMoving = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving);
                                            float spotChance = 0.99f * thiefMoving;
                                            if (Rand.Value > spotChance)
                                            {
                                                MakeVisible();
                                                hidden = false;
                                                //pawn.health.RemoveHediff(this);
                                                AlertXenomorph(pawn, turret);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        num++;
                    }
                    Thing holding = pawn.carryTracker.CarriedThing;
                    if (lastCarried != holding)
                    {
                        if (lastCarried != null)
                        {
                            SetGraphicInt(lastCarried, lastCarriedGraphic);
                        }
                        if (holding != null)
                        {
                            lastCarried = holding;
                            lastCarriedGraphic = holding.Graphic;
                            SetGraphicInt(lastCarried, new Graphic_Invisible());
                        }
                    }
                }
            }
            else
            {
                if (hidden)
                {
                    MakeVisible();
                    hidden = false;
                }
                List<Pawn> thingList = map.mapPawns.AllPawns.Where(x => x != pawn && !x.isXenomorph() && GenSight.LineOfSight(x.Position, pawn.Position, map, true, null, 0, 0) && pawn.Position.DistanceTo(x.Position) <= MinHideDist && !x.Downed && !x.Dead).ToList();
                if (thingList.NullOrEmpty() && lastSpottedTick < Find.TickManager.TicksGame - 125)
                {
                    MakeInvisible();
                    hidden = true;
                }
            }
        }

        public void AlertXenomorph(Pawn pawn, Thing observer)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            /*
            if (raidOnAlert)
            {
                List<Pawn> thisPawn = new List<Pawn>
                {
                    pawn
                };
                IncidentParms parms = new IncidentParms
                {
                    faction = pawn.Faction,
                    spawnCenter = pawn.Position,
                    raidStrategy = RaidStrategyDefOf.ImmediateAttack
                };
                parms.raidStrategy.Worker.MakeLords(parms, thisPawn);
                pawn.Map.avoidGrid.Regenerate();
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            }
            */
            if (observer != null)
            {
                //   Find.LetterStack.ReceiveLetter("LetterLabelThief".Translate(), "ThiefRevealed".Translate(observer.LabelShort, pawn.Faction.Name, pawn.Named("PAWN")), LetterDefOf.ThreatSmall, pawn, null);
            }
            else
            {
                //    Find.LetterStack.ReceiveLetter("LetterLabelThief".Translate(), "ThiefInjured".Translate(pawn.Faction.Name, pawn.Named("PAWN")), LetterDefOf.NegativeEvent, pawn, null);
            }
        }
        
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            Pawn other = null;
            Pawn pawn = base.parent as Pawn;
            if (dinfo.Instigator != null)
            {
                other = dinfo.Instigator as Pawn;
            }
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            hidden = false;
        }
        
        public Thought_Memory GiveObservedThought()
        {
            string concept = string.Format("RRY_Concept_{0}s", pawn.def.label);
            string thought = string.Format("RRY_Observed_{0}", pawn.def.label);
            ConceptDef conceptDef = null;
            ThoughtDef thoughtDef = null;
            Thought_MemoryObservation observation = null;
            thoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(thought);
            conceptDef = DefDatabase<ConceptDef>.GetNamedSilentFail(concept);
            lastSpottedTick = Find.TickManager.TicksGame;
            if (conceptDef != null && !pawn.isXenomorph() && !pawn.isNeomorph())
            {
                if (PlayerKnowledgeDatabase.IsComplete(conceptDef))
                {
                    if (thoughtDef != null)
                    {
                        observation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(thoughtDef);
                    }
                    //   LessonAutoActivator.TeachOpportunity(conceptDef, OpportunityType.Important);
                }
                else
                {
                    thoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(thought);
                    if (thoughtDef != null)
                    {
                        observation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(thoughtDef);
                    }
                }
            }
            else
            {
                thoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(thought);
                if (thoughtDef != null)
                {
                    observation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(thoughtDef);
                }
            }
            if (observation != null)
            {
                observation.Target = this.parent;
                return observation;
            }

            return null;
        }

        public void SetShadowGraphic(PawnRenderer _this, Graphic_Shadow newValue)
        {
            if (_shadowGraphic == null)
            {
                _shadowGraphic = typeof(PawnRenderer).GetField("shadowGraphic", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_shadowGraphic == null)
                {
                    Log.ErrorOnce("Unable to reflect PawnRenderer.shadowGraphic!", 0x12348765);
                    // TODO(1.5+): PawnRenderer.shadowGraphic may have been renamed or removed in RimWorld 1.5+.
                    return;
                }
            }
            _shadowGraphic.SetValue(_this, newValue);
        }

        private Graphic_Shadow GetShadowGraphic(PawnRenderer _this)
        {
            if (_shadowGraphic == null)
            {
                _shadowGraphic = typeof(PawnRenderer).GetField("shadowGraphic", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_shadowGraphic == null)
                {
                    Log.ErrorOnce("Unable to reflect PawnRenderer.shadowGraphic!", 0x12348765);
                    // TODO(1.5+): PawnRenderer.shadowGraphic may have been renamed or removed in RimWorld 1.5+.
                    return null;
                }
            }
            return (Graphic_Shadow)_shadowGraphic.GetValue(_this);
        }

        private void SetGraphicInt(Thing _this, Graphic newValue)
        {
            if (_graphicInt == null)
            {
                _graphicInt = typeof(Thing).GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_graphicInt == null)
                {
                    Log.ErrorOnce("Unable to reflect Thing.graphicInt!", 0x12348765);
                }
            }
            _graphicInt.SetValue(_this, newValue);
        }

        private IntVec3 GetLastCell(Pawn_PathFollower _this)
        {
            if (_lastCell == null)
            {
                _lastCell = typeof(Pawn_PathFollower).GetField("lastCell", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_lastCell == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_PathFollower.lastCell!", 0x12348765);
                }
            }
            return (IntVec3)_lastCell.GetValue(_this);
        }

        public void MakeInvisible()
        {
            // oldGraphics = pawn.Drawer.renderer.graphics; // 1.6: PawnGraphicSet removed
            oldShadow = GetShadowGraphic(pawn.Drawer.renderer);
            // pawn.Drawer.renderer.graphics = new object // PawnGraphicSet_Invisible removed in 1.6(pawn); // 1.6: PawnGraphicSet removed
            ShadowData shadowData = new ShadowData
            {
                volume = new Vector3(0, 0, 0),
                offset = new Vector3(0, 0, 0)
            };
            SetShadowGraphic(pawn.Drawer.renderer, new Graphic_Shadow(shadowData));
            pawn.stances.CancelBusyStanceHard();
            if (lastCarried != null && lastCarried == pawn.carryTracker.CarriedThing)
            {
                lastCarriedGraphic = pawn.carryTracker.CarriedThing.Graphic;
                SetGraphicInt(pawn.carryTracker.CarriedThing, new Graphic_Invisible());
            }
            if (Find.Selector.SelectedObjects.Contains(pawn))
            {
                Find.Selector.SelectedObjects.Remove(pawn);
            }
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Chestbursters))
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Chestbursters, OpportunityType.Important);
            }
        }

        public void MakeVisible()
        {
            // if (oldGraphics != null) pawn.Drawer.renderer.graphics = oldGraphics; // 1.6: PawnGraphicSet removed
            if (oldShadow != null) SetShadowGraphic(pawn.Drawer.renderer, oldShadow);
            Thing holding = pawn.carryTracker.CarriedThing;
            if (holding != null)
            {
                SetGraphicInt(holding, lastCarriedGraphic);
            }
            else if (lastCarried != null)
            {
                SetGraphicInt(lastCarried, lastCarriedGraphic);
            }
            // pawn.Drawer.renderer.graphics.ResolveAllGraphics(); // 1.6: PawnGraphicSet removed
            //     Log.Message(string.Format("removing xeno hidden from {0}", pawn.LabelShortCap));

            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Runners) && pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Runner)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Runners, OpportunityType.Important);
            }
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Drones) && pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Drone)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Drones, OpportunityType.Important);
            }
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Warriors) && pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Warrior)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Warriors, OpportunityType.Important);
            }
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Predaliens) && pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Predalien)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Predaliens, OpportunityType.Important);
            }
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Queens) && pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Queens, OpportunityType.Important);
            }
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Neomorphs) && pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Neomorph)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_Neomorphs, OpportunityType.Important);
            }
        }
        private static List<Pawn> tmpPredatorCandidates = new List<Pawn>();
        public PawnKindDef host;
        public int ticksSinceHeal;
    }
}
