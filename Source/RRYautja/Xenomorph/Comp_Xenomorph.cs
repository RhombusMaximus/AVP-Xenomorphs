using HarmonyLib;
using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RRYautja
{
    public class CompProperties_Xenomorph : CompProperties
    {
        public CompProperties_Xenomorph()
        {
            this.compClass = typeof(Comp_Xenomorph);
        }

        public int healIntervalTicks = 60;
    }

    public class Comp_Xenomorph : ThingComp, IObservedThoughtGiver
    {
        public CompProperties_Xenomorph Props
        {
            get
            {
                return (CompProperties_Xenomorph)this.props;
            }
        }
        private int lastSpottedTick = -9999;
        private Thing lastCarried;
        public bool hidden = false;
        public bool Hidden = false;
        public int healIntervalTicks = 60;
        public int HiveX = -1;
        public int HiveZ = -1;
        public int delay = -1;

        public ThingDef HostDef = null;

        public PawnKindDef HuggerKindDef = XenomorphDefOf.RRY_Xenomorph_FaceHugger;
        public PawnKindDef RoyaleKindDef = XenomorphDefOf.RRY_Xenomorph_RoyaleHugger;

        public PawnKindDef QueenDef = XenomorphDefOf.RRY_Xenomorph_Queen;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastSpottedTick, "lastSpottedtick", -9999);
            Scribe_References.Look(ref lastCarried, "lastCarried");
            Scribe_Values.Look<int>(ref this.ticksSinceHeal, "ticksSinceHeal");
            Scribe_Values.Look<int>(ref this.HiveX, "HiveX", -1);
            Scribe_Values.Look<int>(ref this.HiveZ, "HiveZ", -1);
            Scribe_Defs.Look<PawnKindDef>(ref this.host, "hostRef");
            Scribe_Values.Look<bool>(ref this.hidden, "hidden");
            Scribe_Values.Look<bool>(ref this.Hidden, "Hidden");
            Scribe_Deep.Look<ThingDef>(ref this.HostDef, "HostDef");
        }

        public IntVec3 HiveLoc
        {
            get
            {
                if (HiveX < 1 || HiveZ < 1)
                {
                    //    Log.Message(string.Format("Finding new Hive loc for {0} @ {1}", pawn.NameShortColored, pawn.Position));
                    if (map!=null)
                    {
                        if (InfestationLikeCellFinder.TryFindCell(out IntVec3 hive, out IntVec3 vec3, map, true, false, true))
                        {
                            HiveX = hive.x;
                            HiveZ = hive.z;
                        }
                    }
                }
                //    Log.Message(string.Format("Finding new Hive loc for {0} @ {1}", pawn.NameShortColored, pawn.Position));
                if (map != null)
                {
                    if (map.areaManager.Home[new IntVec3(HiveX, 0, HiveZ)])
                    {
                        if (InfestationLikeCellFinder.TryFindCell(out IntVec3 hive, out IntVec3 vec3, map, true, false, true))
                        {
                            HiveX = hive.x;
                            HiveZ = hive.z;
                        }
                    }
                }
                return new IntVec3(HiveX, 0, HiveZ);
            }
            set
            {
                HiveX = value.x;
                HiveZ = value.z;
            }
        }

        public Pawn pawn
        {
            get
            {
                return (Pawn)parent;
            }
        }

        public Lord CurLord
        {
            get
            {
                return pawn.GetLord();
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
                return (10 * pawn.BodySize);
            }
        }
        public float MinHideLight
        {
            get
            {
                return 1f-Mathf.Max((0.5f * pawn.BodySize), 0f);
            }
        }
        public bool CanHide
        {
            get
            {
                if (pawn.Map == null)
                {
                    return true;
                }
                if (pawn.Dead || pawn.Downed || GenAI.InDangerousCombat(pawn))
                {
                    return false;
                }
                return pawn.Map.glowGrid.GroundGlowAt(pawn.Position, true) < MinHideLight;
            }
        }
        public bool spotted
        {
            get
            {
                if (pawn.Map == null)
                {
                    return false;
                }
                if (!CanHide)
                {
                    return true;
                }

                List<Pawn> pawns = map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => !x.isXenomorph() && pawn.Position.InHorDistOf(x.Position, MinHideDist));
                if (!pawns.NullOrEmpty())
                {
                    if (pawns.Any(x => GenSight.LineOfSight(pawn.Position, x.Position, map)))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Lord XenoLord
        {
            get
            {
                Lord lord = null;
                LordJob job = null;
                List<Lord> Hivelords = new List<Lord>();
                List<Lord> lords = new List<Lord>();
                if (map == null)
                {
                //    Log.Message("XenoLord map == null");
                    return lord;
                }
                if (map.lordManager.lords.Any(x => x.faction == pawn.Faction))
                {
                //    Log.Message("XenoLord lords exist");
                    lords = map.lordManager.lords.Where(x => x.faction == pawn.Faction).ToList();
                }
                if (CurLord!=null)
                {
                //    Log.Message("XenoLord CurLord != null");
                    if (LordReplaceable(CurLord))
                    {
                    //    Log.Message("XenoLord CurLord LordReplaceable");
                        if (!map.HiveGrid().Hivelist.NullOrEmpty())
                        {
                        //    Log.Message("XenoLord Hives on map");
                            HiveLike hive = (HiveLike)map.HiveGrid().Hivelist.RandomElement();
                            if (hive!=null)
                            {
                            //    Log.Message("XenoLord Hive");
                                if (hive.Lord!=null)
                                {
                                //    Log.Message("XenoLord Lord");
                                    lord = hive.Lord;
                                }
                            }
                        }
                        else
                        {
                        //    Log.Message("XenoLord NO Hives on map");
                            for (int i = 0; i < lords.Count-1; i++)
                            {
                                Lord l = lords[i];
                            //    Log.Message(string.Format("XenoLord {0} of {1}", i+1, lords.Count));
                                if (l != null)
                                {
                                //    Log.Message(string.Format("XenoLord {0} != null LordJob: {1}", l, l.LordJob));
                                    if (l.LordJob is LordJob_DefendAndExpandHiveLike j)
                                    {
                                    //    Log.Message(string.Format("XenoLord {0} LordJob_DefendAndExpandHiveLike", l, lords.Count));
                                        lord = l;
                                        job = j;
                                        Pawn Hivequeen = null;
                                        if (l.ownedPawns.Any(x => x.kindDef == QueenDef))
                                        {
                                        //    Log.Message(string.Format("XenoLord {0} Hivequeen", l, lords.Count));
                                            Hivequeen = l.ownedPawns.Find(x => x.kindDef == QueenDef);
                                        }
                                        if (pawn.kindDef != XenomorphDefOf.RRY_Xenomorph_Queen || (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen && Hivequeen != null))
                                        {
                                        //    Log.Message(string.Format("XenoLord {0} Hivelords.Add", l, lords.Count));
                                            Hivelords.Add(l);
                                        }
                                    }

                                }

                            }
                            if (lord == null)
                            {
                            //    Log.Message(string.Format("XenoLord lord = null"));
                                IntVec3 c;
                                if (XenomorphKidnapUtility.TryFindGoodHiveLoc(pawn, out c, null, true, false, true))
                                {
                                //    Log.Message(string.Format("XenoLord TryFindGoodHiveLoc for {0} Cell Found: {1}, Allow: Fogged, Digging", pawn.LabelShortCap, c));
                                    LordJob newJob = new LordJob_DefendAndExpandHiveLike(false, pawn.Faction, c, 40f);
                                    pawn.CreateNewLord(c, newJob);
                                    this.HiveLoc = c;
                                }
                                else
                                if (XenomorphKidnapUtility.TryFindGoodHiveLoc(pawn, out c, null, true, true, true))
                                {
                                //    Log.Message(string.Format("XenoLord TryFindGoodHiveLoc for {0} Cell Found: {1}, Allow: Fogged, Unroofed, Digging", pawn.LabelShortCap, c));
                                    LordJob newJob = new LordJob_DefendAndExpandHiveLike(false, pawn.Faction, c, 40f);
                                    pawn.CreateNewLord(c, newJob);
                                    this.HiveLoc = c;
                                }
                                else
                                {
                                //    Log.Message(string.Format("XenoLord TryFindGoodHiveLoc Cell Not Found"));
                                }
                                if (HiveLoc == c)
                                {
                                //    Log.Message(string.Format("scoreAt {0} == {1}", HiveLoc, InfestationLikeCellFinder.GetScoreAt(HiveLoc, map, true, false, true)));
                                }
                            }
                        }
                    }
                    else
                    {
                    //    Log.Message(string.Format("XenoLord lord = CurLord"));
                        lord = CurLord;
                    }
                }
                else
                {
                //    Log.Message("XenoLord CurLord == null");
                    if (!map.HiveGrid().Hivelist.NullOrEmpty())
                    {
                    //    Log.Message("XenoLord Hivelist");
                        List<HiveLike> hives = new List<HiveLike>();
                        map.HiveGrid().Hivelist.ForEach(x=> hives.Add(((HiveLike)x)));
                        bool anyHiveHasLord = hives.Any(x => x.Lord != null);
                        HiveLike hive = anyHiveHasLord ? hives.Where(x=> x.Lord !=null).RandomElement() : hives.RandomElement();
                        if (hive != null)
                        {
                        //    Log.Message("XenoLord Hive");
                            if (hive.Lord != null)
                            {
                            //    Log.Message("XenoLord Lord");
                                lord = hive.Lord;
                                pawn.SwitchToLord(lord);
                            }
                            else
                            {
                            //    Log.Message("XenoLord no Lord");
                                if (lords.Count() == 0 && hive.Position != IntVec3.Invalid)
                                {
                                //    Log.Message("XenoLord no Lords");
                                    LordJob
                                        newJob = new LordJob_DefendAndExpandHiveLike(false, pawn.Faction, hive.Position, 40f);
                                    lord = pawn.CreateNewLord(hive.Position, newJob);
                                }
                            }
                        }
                    }
                }
                return lord;
            }
        }

        private bool LordReplaceable(Lord lord)
        {
            if (lord == null)
            {
                return true;
            }
            bool isDefendPoint = lord != null ? lord.LordJob is LordJob_DefendPoint : true;
            bool isAssaultColony = lord != null ? lord.LordJob is LordJob_AssaultColony : true;
            bool hostsPresent = map.mapPawns.AllPawnsSpawned.Any(x => x.isPotentialHost() && !x.isCocooned() && IsAcceptablePreyFor(pawn, x, true) && x.Faction.HostileTo(pawn.Faction));
            bool LordReplaceable = (isDefendPoint || (isAssaultColony && hostsPresent && !GenHostility.AnyHostileActiveThreatTo(pawn.Map, pawn.Faction)));
            return LordReplaceable;
        }

        public override void CompTick()
        {
            if (delay>-1 && map != null)
            {
                delay--;
                if (delay==0)
                {
                    Lord lord = XenoLord;
                    if (lord!=null)
                    {
                        delay = -1;
                    }
                }
            }
            if (pawn.Dead)
            {
                if (hidden)
                {
                    hidden = false;
                }
                return;
            }
            if (pawn.ageTracker.CurLifeStage!=XenomorphDefOf.RRY_XenomorphFullyFormed)
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
            if (pawn.Faction==null)
            {
                if (Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph) != null)
                {
                    pawn.SetFaction(Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph));
                }
            }
            base.CompTick();
            this.ticksSinceHeal++;
            bool flag = this.ticksSinceHeal > this.healIntervalTicks;
            if (flag)
            {
                bool flag2 = ((Pawn)base.parent).health.hediffSet.HasNaturallyHealingInjury();
                if (flag2)
                {
                    this.ticksSinceHeal = 0;
                    float num = 8f;
                    Hediff_Injury hediff_Injury = GenCollection.RandomElement<Hediff_Injury>(from x in ((Pawn)base.parent).health.hediffSet.GetHediffs<Hediff_Injury>()
                                                                                             where HediffUtility.CanHealNaturally(x)
                                                                                             select x);
                    hediff_Injury.Heal(num * ((Pawn)base.parent).HealthScale * 0.01f);
                //    Traverse.Create(hediff_Injury).Property(name: "BleedRate").SetValue(hediff_Injury.BleedRate*0.95);
                }
            }
            /*
            if (!true)
            {
                return;
            }
            */
            if (Hidden)
            {
                if (!hidden)
                {
                    //MakeInvisible();
                    hidden = true;
                }
                if (pawn.Downed || pawn.Dead || (pawn.pather != null && pawn.pather.WillCollideNextCell))
                {
                 //   MakeVisible();
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
                if (pawn.Map != null && lastSpottedTick < Find.TickManager.TicksGame - 250)
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
                                        float TargetMoving = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving);
                                        float spotChance = 0.8f * TargetMoving / observerSight;
                                        if (Rand.Value > spotChance || spotted)
                                        {
                                        //    MakeVisible();
                                            hidden = false;
                                            AlertXenomorph(pawn, observer);
                                        }
                                    }
                                    else if (observer == null)
                                    {
                                        if (thing is Building_Turret turret && turret.Faction != null && turret.Faction.IsPlayer)
                                        {
                                            float TargetMoving = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving);
                                            float spotChance = 0.99f * TargetMoving;
                                            if (Rand.Value > spotChance || spotted)
                                            {
                                            //    MakeVisible();
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
                    /*
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
                    */
                }
            }
            else
            {
                if (hidden && (spotted || !CanHide))
                {
                //    MakeVisible();
                    hidden = false;
                }
                if (CanHide)
                {
                    List<Pawn> thingList = map.mapPawns.AllPawns.Where(x => x != pawn && !x.isXenomorph() && GenSight.LineOfSight(x.Position, pawn.Position, map, true, null, 0, 0) && pawn.Position.DistanceTo(x.Position) <= MinHideDist && !x.Downed && !x.Dead).ToList();
                    if (thingList.NullOrEmpty() && lastSpottedTick < Find.TickManager.TicksGame - 125)
                    {
                        //MakeInvisible();
                        hidden = true;
                    }
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

        public static void doClot(Pawn pawn, BodyPartRecord part = null)
        {
            var i = 5;
            foreach (var hediff in pawn.health.hediffSet.hediffs.Where(x => x.Bleeding).OrderByDescending(x => x.BleedRate))
            {
                if (Rand.ChanceSeeded(0.25f, AvPConstants.AvPSeed))
                {
                    hediff.Tended(Math.Min(Rand.Value + Rand.Value + Rand.Value, 1f));
                }
                i--;

                if (i <= 0) return;
            }
        }

        public void TrySealWounds()
        {
            IEnumerable<Hediff> wounds = (from hd in pawn.health.hediffSet.hediffs
                                          where hd.Bleeding
                                          select hd);

            if (wounds != null)
            {
                foreach (Hediff wound in wounds)
                {
                    HediffWithComps hediffWithComps = wound as HediffWithComps;
                    if (hediffWithComps != null)
                    {
                        HediffComp_TendDuration hediffComp_TendDuration = hediffWithComps.TryGetComp<HediffComp_TendDuration>();

                        //Equivalent to Glitterworld Medicine.
                        hediffComp_TendDuration.tendQuality = 2.0f;                    //Sets the tending quality.
                        hediffComp_TendDuration.tendTicksLeft = Find.TickManager.TicksGame; //Sets the last tend tick.

                        pawn.health.Notify_HediffChanged(wound);
                    }
                }
            }
        }
        
        public void TryRegrowBodyparts()
        {
            //Iterate through the whole body from Core.
            //Stop at first and try to regrow there.
            foreach (BodyPartRecord part in pawn.GetFirstMatchingBodyparts(pawn.RaceProps.body.corePart, HediffDefOf.MissingBodyPart, XenomorphDefOf.RRY_Hediff_Xenomorph_ProtoBodypart, hediff => hediff is Hediff_AddedPart))
            {
                //Get the bodypart it is on.
                Hediff missingHediff = pawn.health.hediffSet.hediffs.First(hediff => hediff.Part == part && hediff.def == HediffDefOf.MissingBodyPart);

                if (missingHediff != null)
                {
                    //Remove the missing body part.
                    pawn.health.RemoveHediff(missingHediff);

                    //Insert fake body part.
                    pawn.health.AddHediff(XenomorphDefOf.RRY_Hediff_Xenomorph_ProtoBodypart, part);
                    pawn.health.hediffSet.DirtyCache();
                }
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            /*
            Pawn other = null;
            Pawn pawn = base.parent as Pawn;
            if (dinfo.Instigator!=null)
            {
                other = dinfo.Instigator as Pawn;
            }
            */
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            /*
            if (pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Xenomorph_Hidden))
            {
                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_Hediff_Xenomorph_Hidden));
            }
            */

        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            bool acidburns = true;
            if (base.parent is Pawn pawn && pawn != null)
            {
                if (dinfo.Def!=null)
                {
                    if (dinfo.Def.hediff == DefDatabase<HediffDef>.GetNamedSilentFail("CP_CQCTakedownHediff"))
                    {
                        absorbed = true;
                        return;
                    }
                    if (dinfo.Def.isRanged)
                    {
                        absorbed = false;
                        return;
#if DEBUG
                        //    Log.Message(string.Format("{0} is ranged: {1}", dinfo.Weapon.LabelCap, dinfo.Def.isRanged));
#endif
                    }
                    if (!dinfo.Def.isRanged)
                    {
                        if (dinfo.Instigator !=null && dinfo.Instigator is Pawn Instigator && Instigator != pawn && Instigator.AdjacentTo8WayOrInside(pawn))
                        {
                            if (dinfo.Def.makesBlood)
                            {
                                if (dinfo.Weapon is ThingDef WeaponDef && WeaponDef != null)
                                {
                                    if (WeaponDef.IsWeapon)
                                    {
                                        if (WeaponDef == Instigator.equipment.Primary.def && Instigator.equipment.Primary is ThingWithComps Weapon && Instigator.equipment.PrimaryEq is CompEquippable WeaponEQ)
                                        {
                                            float resistance = Weapon.GetStatValue(AvPDefOf.RRY_AcidResistance);
                                            acidburns = resistance != 1f;
                                            if (acidburns)
                                            {
                                                int dmg = (int)(Rand.Range(0, 5) * (1 - resistance));
                                                Weapon.HitPoints -= dmg;
                                                if (Weapon.HitPoints <= 0)
                                                {
                                                    Weapon.Destroy();
                                                }
                                            }
                                        }
                                    }
                                }
                                if (Rand.Chance(0.25f) && Instigator.Map != null)
                                {
                                    FilthMaker.TryMakeFilth(Instigator.Position, Instigator.Map, XenomorphDefOf.RRY_FilthBloodXenomorph_Active, pawn.LabelIndefinite(), 1);
                                }
                            }
                        }
                    }
                    else
                    {
#if DEBUG
                     //   Log.Message(string.Format("{0} is unknown", dinfo.Weapon.LabelCap));
#endif
                    }
                }
            }
            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }

        // Token: 0x06000186 RID: 390 RVA: 0x0000E940 File Offset: 0x0000CD40
        public Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap, bool findhost = false)
        {
            bool selected = Find.Selector.SelectedObjects.Contains(predator) && Prefs.DevMode;
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
            {
                return null;
            }
            bool flag = false;
            float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
            if (summaryHealthPercent < 0.5f)
            {
                flag = true;
            }
            tmpPredatorCandidates.Clear();
            int maxRegionsToScan = GetMaxRegionsToScan(predator, forceScanWholeMap);
            if (predator.jobs.debugLog) predator.jobs.DebugLogEvent(string.Format("Xenomorph BestPawnToHuntForPredator maxRegionsToScan: {0}", maxRegionsToScan));
            if (maxRegionsToScan < 0)
            {
                tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned.Where(x=> (x.isPotentialHost(out string fail) || !findhost)));

            }
            else
            {
                TraverseParms traverseParms = TraverseParms.For(predator, Danger.Deadly, TraverseMode.ByPawn, false);
                RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, true), delegate (Region x)
                {
                    List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (((Pawn)list[j]).isPotentialHost() || !findhost)
                        {
                            tmpPredatorCandidates.Add((Pawn)list[j]);
                        }
                    }
                    return false;
                }, 999999, RegionType.Set_Passable);
            }
            Pawn pawn = null;
            float num = 0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            for (int i = 0; i < tmpPredatorCandidates.Count; i++)
            {
                Pawn pawn2 = tmpPredatorCandidates[i];
                if (predator.jobs.debugLog) predator.jobs.DebugLogEvent(string.Format("predator List contains {0}", pawn2.NameShortColored));
                if (predator.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
                {
                    if (predator != pawn2)
                    {
                        if (!flag || pawn2.Downed)
                        {
                            if (IsAcceptablePreyFor(predator, pawn2, findhost))
                            {
                                if (predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                                {
                                    if (!pawn2.IsForbidden(predator))
                                    {
                                        if (!tutorialMode || pawn2.Faction != Faction.OfPlayer)
                                        {
                                            float preyScoreFor = GetPreyScoreFor(predator, pawn2, findhost);
                                            if (preyScoreFor > num || pawn == null)
                                            {
                                                num = preyScoreFor;
                                                pawn = pawn2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            tmpPredatorCandidates.Clear();
            if (selected)
            {
                if (pawn != null)
                {
                //    Log.Message(string.Format("{0} hunting: {1}, @: {2}", predator.LabelShortCap, pawn.LabelShortCap, pawn.Position));
                }
                else
                {
                //    Log.Message(string.Format("{0} Could not find a suitable pawn to hunt", predator.LabelShortCap));
                }
            }
            return pawn;
        }

        // Token: 0x06000187 RID: 391 RVA: 0x0000EB14 File Offset: 0x0000CF14
        public bool IsAcceptablePreyFor(Pawn predator, Pawn prey, bool findhost)
        {
            if (prey.isXenomorph())
            {
                return false;
            }
            if (prey.isCocooned())
            {
                return false;
            }
            if (findhost)
            {
                if (prey.Downed && prey.Awake())
                {
                    return false;
                }
                if (!prey.isPotentialHost())
                {
                    return false;
                }
                if (prey.isNeomorph())
                {
                    return false;
                }
            }
            if (!findhost)
            {
                if (prey.isPotentialHost())
                {
                    return false;
                }
            }
            /*
            if (!prey.RaceProps.canBePredatorPrey)
            {
                return false;
            }
            */
            /*
            if (!prey.RaceProps.IsFlesh)
            {
                return false;
            }
            */
            /*
            if (!Find.Storyteller.difficulty.predatorsHuntHumanlikes && prey.RaceProps.Humanlike)
            {
                return false;
            }
            */
            if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
            {
                return false;
            }
            if (!prey.Downed)
            {
                if (prey.GetStatValue(StatDefOf.MeleeDPS) > 2f * predator.GetStatValue(StatDefOf.MeleeDPS))
                {
                    return false;
                }
                float num = prey.GetStatValue(StatDefOf.MeleeDPS) * (prey.def.race.baseHealthScale * prey.health.summaryHealth.SummaryHealthPercent) * prey.ageTracker.CurLifeStage.bodySizeFactor;
                float numa = prey.GetStatValue(StatDefOf.MeleeDPS);
                float numb = prey.HealthScale;
                float numc = prey.GetStatValue(StatDefOf.MeleeHitChance);
                float numd = prey.GetStatValue(StatDefOf.MeleeDodgeChance);
                num = ((num + numa) * numc) * (1 - numd) * numb;
                bool selected = Find.Selector.SelectedObjects.Contains(predator) && Prefs.DevMode;
                if (selected)
                {
                //    Log.Message(string.Format("prey: {0} @: {1}", prey.LabelCap, prey.Position));
                //    Log.Message(string.Format("num: {0} = MeleeDPS: {1} * (baseHealthScale * SummaryHealthPercent): {2} * bodySizeFactor: {3}", num, prey.GetStatValue(StatDefOf.MeleeDPS), (prey.def.race.baseHealthScale * prey.health.summaryHealth.SummaryHealthPercent), prey.ageTracker.CurLifeStage.bodySizeFactor));
                //    Log.Message(string.Format("numa: {0} = MeleeDPS: {1} * MeleeHitChance: {2} * MeleeDodgeChance: {3}", numa, prey.GetStatValue(StatDefOf.MeleeDPS) , prey.GetStatValue(StatDefOf.MeleeHitChance) , prey.GetStatValue(StatDefOf.MeleeDodgeChance)));
                //    Log.Message(string.Format("numb: {0} = HealthScale: {1}", numb, prey.HealthScale));
                }
                float num2 = predator.GetStatValue(StatDefOf.MeleeDPS) * (predator.def.race.baseHealthScale * predator.health.summaryHealth.SummaryHealthPercent) * predator.ageTracker.CurLifeStage.bodySizeFactor;
                float num2a = predator.GetStatValue(StatDefOf.MeleeDPS);
                float num2b = predator.HealthScale;
                float num2c = predator.GetStatValue(StatDefOf.MeleeHitChance);
                float num2d = predator.GetStatValue(StatDefOf.MeleeDodgeChance);
                num2 = ((num2 + num2a) * num2c) * (1 - num2d) * num2b;
                if (selected)
                {
                //    Log.Message(string.Format("num2: {0} = MeleeDPS: {1} * SummaryHealthPercent: {2} * bodySizeFactor: {3}", num2, predator.GetStatValue(StatDefOf.MeleeDPS), (predator.def.race.baseHealthScale * predator.health.summaryHealth.SummaryHealthPercent), predator.ageTracker.CurLifeStage.bodySizeFactor));
                //    Log.Message(string.Format("num2a: {0} = MeleeDPS: {1} * MeleeHitChance: {2} * MeleeDodgeChance: {3}", num2a, predator.GetStatValue(StatDefOf.MeleeDPS), predator.GetStatValue(StatDefOf.MeleeHitChance), predator.GetStatValue(StatDefOf.MeleeDodgeChance)));
                //    Log.Message(string.Format("num2b: {0} = HealthScale: {1}", num2b, predator.HealthScale));
                }
                if (num >= num2)
                {
                    float num3 = map.mapPawns.AllPawns.Where(x => x.isXenomorph() && x.def == predator.def).Count();
                    num2 *= num3;
                    if (num >= num2)
                    {
                        return false;
                    }
                }
            }
            return (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (predator.Faction == null || prey.HostFaction == null || predator.HostileTo(prey)) && (predator.Faction != Faction.OfPlayer || prey.Faction != Faction.OfPlayer) && (!predator.RaceProps.herdAnimal || predator.def != prey.def);
        }

        // Token: 0x06000180 RID: 384 RVA: 0x0000E414 File Offset: 0x0000C814
        private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
        {
            if (getter.RaceProps.Humanlike)
            {
                return -1;
            }
            if (forceScanWholeMap)
            {
                return -1;
            }
            if (getter.Faction == Faction.OfPlayer)
            {
                return 100;
            }
            return 30;
        }

        // Token: 0x06000188 RID: 392 RVA: 0x0000ECA4 File Offset: 0x0000D0A4
        public static float GetPreyScoreFor(Pawn predator, Pawn prey, bool findhost)
        {
            float offset = XenomorphUtil.TotalSpawnedXenomorphPawnCount(prey.Map);
            float num = (prey.def.race.baseHealthScale * prey.health.summaryHealth.SummaryHealthPercent) / (predator.def.race.baseHealthScale * predator.health.summaryHealth.SummaryHealthPercent);
            float num2 = prey.health.summaryHealth.SummaryHealthPercent;
            float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor / predator.ageTracker.CurLifeStage.bodySizeFactor;
            float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
            if (prey.Downed)
            {
                num2 = Mathf.Min(num2, 0.2f);
            }
            float num3 = -lengthHorizontal - 56f * num2 * num2 * num * bodySizeFactor;
            float num4 = -56f * num2 * num2 * num * bodySizeFactor;
            if (prey.isHost())
            {
                if (prey.isXenoHost())
                {
                    num3 -= 350f;
                }
                if (prey.isNeoHost() && findhost)
                {
                    num3 -= 350f;
                }
            }
            if (prey.isNeomorph())
            {
                num3 -= 350f;
            }
            if (prey.isPotentialHost())
            {
                num3 += 20f;
            }
            if (prey.RaceProps.Humanlike)
            {
                num3 -= 35f;
            }
            num3 += offset*3;
            bool selected = Find.Selector.SelectedObjects.Contains(predator) && Prefs.DevMode;
            if (selected)
            {
            //    Log.Message(string.Format("{0} found: {1} @: {2}\nPreyScore: {3}, BFPreyScore: {4}, isXenoHost: {5}, isNeoHost: {6}, isXenomorph: {7}, isNeomorph: {8}, isPotentialHost: {9}, Humanlike: {10}", predator.LabelShortCap, prey.LabelShortCap, prey.Position, num3, num4, prey.isXenoHost(), prey.isNeoHost(), prey.isXenomorph(), prey.isNeomorph(), prey.isPotentialHost(), prey.RaceProps.Humanlike));
            }
            return num3;
        }
        
        public Thought_Memory GiveObservedThought(Pawn observer)
        {
            DefDatabase<ThoughtDef>.AllDefs.Any(x => x.defName.Contains("RRY_Observed_Xenomorph") && x.defName.Contains(pawn.LabelCap));
            string concept = string.Format("RRY_Concept_{0}s", pawn.def.label);
            string thought = DefDatabase<ThoughtDef>.AllDefs.Any(x => x.defName.Contains("RRY_Observed_Xenomorph") && x.defName.Contains(pawn.LabelCap)) ? DefDatabase<ThoughtDef>.AllDefs.First(x => x.defName.Contains("RRY_Observed_Xenomorph") && x.defName.Contains(pawn.LabelCap)).defName : string.Format("RRY_Observed_Xenomorph");
            ConceptDef conceptDef = null;
            ThoughtDef thoughtDef = null;
            Thought_MemoryObservation observation = null;
            thoughtDef = DefDatabase<ThoughtDef>.GetNamedSilentFail(thought);
            conceptDef = DefDatabase<ConceptDef>.GetNamedSilentFail(concept);
            lastSpottedTick = Find.TickManager.TicksGame;
            if (thoughtDef != null)
            {
                observation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(thoughtDef);
            }
            if (conceptDef != null)
            {
                if (true)
                {

                }
            }
            if (observation != null)
            {
                observation.Target = this.parent;
                return observation;
            }

            return null;
        }
        /*
        public void SetShadowGraphic(PawnRenderer _this, Graphic_Shadow newValue)
        {
            if (_shadowGraphic == null)
            {
                _shadowGraphic = typeof(PawnRenderer).GetField("shadowGraphic", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_shadowGraphic == null)
                {
                    Log.ErrorOnce("Unable to reflect PawnRenderer.shadowGraphic!", 0x12348765);
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
            if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_FaceHugger)
            {

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
        */
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (pawn.ageTracker.CurLifeStage != XenomorphDefOf.RRY_XenomorphFullyFormed)
            {
                hidden = true;
            }
            MapComponent_HiveGrid hiveGrid = map.HiveGrid();
            if (hiveGrid != null)
            {
                if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_FaceHugger)
                {
                    /*
                    if (!hiveGrid.Dronelist.Contains(pawn))
                    {
                        hiveGrid.Dronelist.Add(pawn);
                    }
                    */
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Runner)
                {
                    if (!hiveGrid.Runnerlist.Contains(pawn))
                    {
                        hiveGrid.Runnerlist.Add(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone)
                {
                    if (!hiveGrid.Dronelist.Contains(pawn))
                    {
                        hiveGrid.Dronelist.Add(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Warrior)
                {
                    if (!hiveGrid.Warriorlist.Contains(pawn))
                    {
                        hiveGrid.Warriorlist.Add(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Predalien)
                {
                    if (!hiveGrid.Predalienlist.Contains(pawn))
                    {
                        hiveGrid.Predalienlist.Add(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen)
                {
                    if (!hiveGrid.Queenlist.Contains(pawn))
                    {
                        hiveGrid.Queenlist.Add(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Thrumbomorph)
                {
                    if (!hiveGrid.HiveGuardlist.Contains(pawn))
                    {
                        hiveGrid.HiveGuardlist.Add(pawn);
                    }
                }
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {

            MapComponent_HiveGrid hiveGrid = map.HiveGrid();
            if (hiveGrid != null)
            {
                if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_FaceHugger)
                {
                    /*
                    if (hiveGrid.Dronelist.Contains(pawn))
                    {
                        hiveGrid.Dronelist.Remove(pawn);
                    }
                    */
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Runner)
                {
                    if (hiveGrid.Runnerlist.Contains(pawn))
                    {
                        hiveGrid.Runnerlist.Remove(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone)
                {
                    if (hiveGrid.Dronelist.Contains(pawn))
                    {
                        hiveGrid.Dronelist.Remove(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Warrior)
                {
                    if (hiveGrid.Warriorlist.Contains(pawn))
                    {
                        hiveGrid.Warriorlist.Remove(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Predalien)
                {
                    if (hiveGrid.Predalienlist.Contains(pawn))
                    {
                        hiveGrid.Predalienlist.Remove(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen)
                {
                    if (hiveGrid.Queenlist.Contains(pawn))
                    {
                        hiveGrid.Queenlist.Remove(pawn);
                    }
                }
                else if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Thrumbomorph)
                {
                    if (hiveGrid.HiveGuardlist.Contains(pawn))
                    {
                        hiveGrid.HiveGuardlist.Remove(pawn);
                    }
                }
            }
            base.PostDeSpawn(map);
        }

        private static List<Pawn> tmpPredatorCandidates = new List<Pawn>();
        public PawnKindDef host;
        public int ticksSinceHeal;

        public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
        {
            return null;
        }
    }
}
