using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Reflection;
using UnityEngine;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    // Token: 0x02000188 RID: 392
    public class Hediff_Hidden_Xeno : HediffWithComps
    {
        private FieldInfo _shadowGraphic;
        private FieldInfo _graphicInt;
        private FieldInfo _lastCell;
        private bool hidden;

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

        public bool Hidden
        {
            get
            {
                if (pawn.isXenomorph())
                {
                    Comp_Xenomorph _Xenomorph = pawn.TryGetComp<Comp_Xenomorph>();
                    return _Xenomorph.Hidden;
                }
                if (pawn.isNeomorph())
                {
                    Comp_Neomorph _Nenomorph = pawn.TryGetComp<Comp_Neomorph>();
                    return _Nenomorph.Hidden;
                }
                return false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastSpottedTick, "lastSpottedtick", -9999);
            Scribe_Values.Look(ref raidOnAlert, "raidOnAlert", false);
            Scribe_Values.Look(ref hidden, "hidden", false);
            Scribe_References.Look(ref lastCarried, "lastCarried");
            // PawnGraphicSet
            /*
            Scribe_Values.Look(ref oldGraphics, "oldGraphics");
            Scribe_Values.Look(ref oldShadow, "oldShadow");
            */
        }

        public bool raidOnAlert = false;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (pawn.ageTracker.CurLifeStage != XenomorphDefOf.RRY_XenomorphFullyFormed)
            {
                MakeInvisible();
            }
        }

        public override void Tick()
        {
            if (Hidden)
            {
                if (!hidden)
                {
                    MakeInvisible();
                    hidden = true;
                }
                if (!pawn.Spawned)
                {
                    pawn.health.RemoveHediff(this);
                }
                if (pawn.Downed || pawn.Dead || (pawn.pather != null && pawn.pather.WillCollideWithPawnOnNextPathCell()))
                {
                    pawn.health.RemoveHediff(this);
                    if (pawn.pather != null)
                    {
                        AlertXenomorph(pawn, pawn.pather.nextCell.GetFirstPawn(pawn.Map));
                    }
                    else
                    {
                        AlertXenomorph(pawn, null);
                    }
                }
                if (pawn.pather != null && GetLastCell(pawn.pather).GetDoor(pawn.Map) != null)
                {
                    GetLastCell(pawn.pather).GetDoor(pawn.Map).StartManualCloseBy(pawn);
                }
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
                                    if (observer != null && observer != pawn && observer.Faction != null && (observer.Faction.IsPlayer || observer.Faction.HostileTo(pawn.Faction)))
                                    {
                                        float observerSight = observer.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
                                        observerSight *= 0.805f + (pawn.Map.glowGrid.GameGlowAt(pawn.Position) / 4);
                                        if (observer.RaceProps.Animal)
                                        {
                                            observerSight *= 0.9f;
                                        }
                                        observerSight = Math.Min(2f, observerSight);
                                        float thiefMoving = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving);
                                        float spotChance = 0.8f * thiefMoving / observerSight;
                                        if (Rand.Value > spotChance)
                                        {
                                            if (pawn.isXenomorph())
                                            {
                                                Comp_Xenomorph _Xenomorph = pawn.TryGetComp<Comp_Xenomorph>();
                                                _Xenomorph.Hidden = false;
                                            }
                                            if (pawn.isNeomorph())
                                            {
                                                Comp_Neomorph _Nenomorph = pawn.TryGetComp<Comp_Neomorph>();
                                                _Nenomorph.Hidden = false;
                                            }
                                            //pawn.health.RemoveHediff(this);
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
                                                if (pawn.isXenomorph())
                                                {
                                                    Comp_Xenomorph _Xenomorph = pawn.TryGetComp<Comp_Xenomorph>();
                                                    _Xenomorph.Hidden = false;
                                                }
                                                if (pawn.isNeomorph())
                                                {
                                                    Comp_Neomorph _Nenomorph = pawn.TryGetComp<Comp_Neomorph>();
                                                    _Nenomorph.Hidden = false;
                                                }
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
            }
        }

        public void MakeInvisible()
        {
            oldGraphics = pawn.Drawer.renderer.graphics;
            oldShadow = GetShadowGraphic(pawn.Drawer.renderer);
            pawn.Drawer.renderer.graphics = new PawnGraphicSet_Invisible(pawn);
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
            if (oldGraphics != null) pawn.Drawer.renderer.graphics = oldGraphics;
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
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            //     Log.Message(string.Format("removing xeno hidden from {0}", pawn.LabelShortCap));
            if (true)
            {

            }
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

        public override void PostRemoved()
        {
            base.PostRemoved();
            this.MakeVisible();
        }

        public void AlertXenomorph(Pawn pawn, Thing observer)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
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
            if (observer != null)
            {
                //   Find.LetterStack.ReceiveLetter("LetterLabelThief".Translate(), "ThiefRevealed".Translate(observer.LabelShort, pawn.Faction.Name, pawn.Named("PAWN")), LetterDefOf.ThreatSmall, pawn, null);
            }
            else
            {
                //    Find.LetterStack.ReceiveLetter("LetterLabelThief".Translate(), "ThiefInjured".Translate(pawn.Faction.Name, pawn.Named("PAWN")), LetterDefOf.NegativeEvent, pawn, null);
            }
        }


        private PawnGraphicSet oldGraphics;
        private Graphic_Shadow oldShadow;
        private int lastSpottedTick = -9999;
        private Graphic lastCarriedGraphic;
        private Thing lastCarried;
    }
}
