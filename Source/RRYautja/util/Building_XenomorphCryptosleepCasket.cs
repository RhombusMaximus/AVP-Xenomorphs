using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RRYautja
{
    public class ThingDef_CryptosleepCasket : ThingDef
    {
        public FactionDef Faction;
        public List<PawnKindDef> PawnKinds = new List<PawnKindDef>();
        public HediffDef infectionDef;
        public ThingDef spawnThing;
        public BodyPartRecord part;
    }
    // Token: 0x020006BF RID: 1727
    public class Building_XenomorphCryptosleepCasket : Building_CryptosleepCasket
    {
        public ThingDef_CryptosleepCasket Def
        {
            get
            {
                return this.def as ThingDef_CryptosleepCasket;
            }
        }

        public Faction OfFaction
        {
            get
            {
                if (Def.Faction!=null)
                {
                    return Find.FactionManager.FirstFactionOfDef(Def.Faction);
                }
                return null;
            }
        }

        public FactionDef OfFactionDef
        {
            get
            {
                if (Def.Faction != null)
                {
                    return Def.Faction;
                }
                return null;
            }
        }

        public List<PawnKindDef> OfPawnKinds
        {
            get
            {
                if (Def.PawnKinds.Count > 0)
                {
                    PawnKinds = Def.PawnKinds;
                }
                else if (Def.Faction != null)
                {
                    var list = (from def in DefDatabase<PawnKindDef>.AllDefs
                                where ((def.defaultFactionDef == OfFaction.def && def.defaultFactionDef != null) || (def.defaultFactionDef == null && OfFaction.def.pawnGroupMakers.Any(pgm => pgm.options.Any(opt => opt.kind == def) && pgm.kindDef != PawnGroupKindDefOf.Trader && pgm.kindDef != PawnGroupKindDefOf.Peaceful))) && def.isFighter
                                select def).ToList();
                    if (list.Count > 0)
                    {
                        PawnKinds = list;
                    }
                }
                //Log.Message(string.Format("PawnKinds: {0}", PawnKinds.ToString()));
                //Log.Message(string.Format("PawnKinds.Count: {0}", PawnKinds.Count));
                return PawnKinds;
            }
        }

        public List<PawnKindDef> PawnKinds = new List<PawnKindDef>();

        // Token: 0x060024B7 RID: 9399 RVA: 0x00117923 File Offset: 0x00115D23
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Values.Look<bool>(ref this.beenfilled, "beenfilled", false, false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!this.HasAnyContents && !this.beenfilled)
            {
                MakeCasketContents(this);
                this.beenfilled = true;
            }
        }

        // Token: 0x060024B8 RID: 9400 RVA: 0x00117940 File Offset: 0x00115D40
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (dinfo.Def == DamageDefOf.Flame)
            {
                absorbed = true;
            }
            if (absorbed)
            {
                return;
            }
            int fullHP = this.MaxHitPoints;
            int halfHP = this.MaxHitPoints / 2;
            int quarterHP = this.MaxHitPoints / 4;
            int currentHP = this.HitPoints;
        //    Log.Message(string.Format("{1} < {2} = {0}", currentHP < quarterHP, currentHP , quarterHP));
            if (currentHP <quarterHP && !this.contentsKnown && this.innerContainer.Count > 0 && dinfo.Def.harmsHealth && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
            {
                bool flag = false;
                foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
                {
                    if (thing is Pawn pawn)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    this.EjectContents();
                }
            }
            absorbed = false;
        }

        // Token: 0x060024B9 RID: 9401 RVA: 0x00117A10 File Offset: 0x00115E10
        public override void EjectContents()
        {
            List<Thing> list = new List<Thing>();
            if (!this.contentsKnown)
            {
                list.AddRange(this.innerContainer);
                list.AddRange(this.UnopenedCasketsInGroup().SelectMany((Building_XenomorphCryptosleepCasket c) => c.innerContainer));
            }
            bool contentsKnown = this.contentsKnown;
            ThingDef filth_Slime = ThingDefOf.Filth_Slime;
            foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
            {
                if (thing is Pawn pawn)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    pawn.filth.GainFilth(filth_Slime);
                    if (pawn.RaceProps.IsFlesh)
                    {
                        pawn.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null, null);
                    }
                }
            }
            if (!base.Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }
            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, (x => GridsUtility.GetFirstBuilding(x, this.Map) == null));
            this.contentsKnown = true;
            if (!contentsKnown)
            {
            //    ThingDef filth_Slime = ThingDefOf.Filth_Slime;
                FilthMaker.TryMakeFilth(base.Position, base.Map, filth_Slime, Rand.Range(8, 12));
                this.SetFaction(null, null);
                /*
                foreach (Building_XenomorphCryptosleepCasket building_AncientCryptosleepCasket in this.UnopenedCasketsInGroup())
                {
                    building_AncientCryptosleepCasket.EjectContents();
                }
                */
                List<Pawn> source = list.OfType<Pawn>().ToList<Pawn>();
                IEnumerable<Pawn> enumerable = from p in source
                                               where p.RaceProps.Humanlike && p.GetLord() == null && p.Faction == Faction.OfAncientsHostile
                                               select p;
                if (enumerable.Any<Pawn>())
                {
                    LordMaker.MakeNewLord(Faction.OfAncientsHostile, new LordJob_AssaultColony(Faction.OfAncientsHostile, false, false, false, false, false), base.Map, enumerable);
                }
            }
        }

        // Token: 0x060024BA RID: 9402 RVA: 0x00117B54 File Offset: 0x00115F54
        private IEnumerable<Building_XenomorphCryptosleepCasket> UnopenedCasketsInGroup()
        {
            yield return this;
            if (this.groupID != -1)
            {
                foreach (Thing t in base.Map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_XenomorphCryptosleepCasket))
                {
                    Building_XenomorphCryptosleepCasket casket = t as Building_XenomorphCryptosleepCasket;
                    if (casket.groupID == this.groupID && !casket.contentsKnown)
                    {
                        yield return casket;
                    }
                }
            }
            yield break;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.def.Minifiable) // && base.Faction == Faction.OfPlayer)
            {
                yield return InstallationDesignatorDatabase.DesignatorFor(this.def);
            }
            foreach (var item in base.GetGizmos().AsEnumerable())
            {
                yield return item;
            }
        }

        // Token: 0x040014BF RID: 5311
        public int groupID = -1;
        public bool beenfilled;

        // Token: 0x0600000E RID: 14 RVA: 0x00002F10 File Offset: 0x00001110
        private void MakeCasketContents(Building_XenomorphCryptosleepCasket casket)
        {
            if (OfPawnKinds.Count > 0)
            {
                GenerateFromPawnkindList(casket);
            }
            else
            {
                int num = Rand.RangeInclusive(0, 100);
                bool flag = num < 10;
                if (flag)
                {
                    Building_XenomorphCryptosleepCasket.GenerateFriendlyAnimal(casket);
                }
                else
                {
                    bool flag2 = num < 20;
                    if (flag2)
                    {
                        Building_XenomorphCryptosleepCasket.GenerateFriendlySpacer(casket);
                    }
                    else
                    {
                        bool flag3 = num < 35;
                        if (flag3)
                        {
                            Building_XenomorphCryptosleepCasket.GenerateIncappedSpacer(casket);
                        }
                        else
                        {
                            bool flag4 = num < 50;
                            if (flag4)
                            {
                                Building_XenomorphCryptosleepCasket.GenerateSlave(casket);
                            }
                            else
                            {
                                bool flag5 = num < 65;
                                if (flag5)
                                {
                                    Building_XenomorphCryptosleepCasket.GenerateHalfEatenAncient(casket);
                                }
                                else
                                {
                                    Building_XenomorphCryptosleepCasket.GenerateAngryAncient(casket);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002F8C File Offset: 0x0000118C
        private static void GenerateFriendlyAnimal(Building_XenomorphCryptosleepCasket pod)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(Faction.OfPlayer.def);
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomAnimalForSpawn(), faction, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            if (Rand.Chance(0.25f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                pawn.health.AddHediff(XenomorphDefOf.RRY_FaceHuggerInfection, part);
            }
            bool flag = !pod.TryAcceptThing(pawn, false);
            if (flag)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002F8C File Offset: 0x0000118C
        private static void GenerateUnfriendlyAnimal(Building_XenomorphCryptosleepCasket pod)
        {
            Faction faction = null;
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomAnimalForSpawn(), faction, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            if (Rand.Chance(0.5f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                pawn.health.AddHediff(XenomorphDefOf.RRY_FaceHuggerInfection, part);
            }
            bool flag = !pod.TryAcceptThing(pawn, false);
            if (flag)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002F8C File Offset: 0x0000118C
        private static void GenerateUnfriendlyXenomorph(Building_XenomorphCryptosleepCasket pod)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph);
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomXenomorphForSpawn(), faction, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            bool flag = !pod.TryAcceptThing(pawn, false);
            if (flag)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00003024 File Offset: 0x00001224
        private static void GenerateFriendlySpacer(Building_XenomorphCryptosleepCasket pod)
        {
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomSpacerPawnForSpawn(), Faction.OfAncients, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            if (Rand.Chance(0.25f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                pawn.health.AddHediff(XenomorphDefOf.RRY_FaceHuggerInfection, part);
            }
            else if (Rand.Chance(0.25f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Torso);
                HediffDef hediff = Rand.Chance(0.75f) ? XenomorphDefOf.RRY_HiddenXenomorphImpregnation : XenomorphDefOf.RRY_HiddenNeomorphImpregnation;
                pawn.health.AddHediff(hediff, part);
            }

            Building_XenomorphCryptosleepCasket.GiveRandomLootInventoryForTombPawn(pawn);
            bool flag = !pod.TryAcceptThing(pawn, false);
            if (flag)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000030B4 File Offset: 0x000012B4
        private static void GenerateIncappedSpacer(Building_XenomorphCryptosleepCasket pod)
        {
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomSpacerPawnForSpawn(), Faction.OfAncients, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            HealthUtility.DamageUntilDowned(pawn, true);
            if (Rand.Chance(0.25f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                pawn.health.AddHediff(XenomorphDefOf.RRY_FaceHuggerInfection, part);
            }
            else if (Rand.Chance(0.25f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Torso);
                HediffDef hediff = Rand.Chance(0.75f) ? XenomorphDefOf.RRY_HiddenXenomorphImpregnation : XenomorphDefOf.RRY_HiddenNeomorphImpregnation;
                pawn.health.AddHediff(hediff, part);
            }
            Building_XenomorphCryptosleepCasket.GiveRandomLootInventoryForTombPawn(pawn);
            bool flag = !pod.TryAcceptThing(pawn, false);
            if (flag)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x06000012 RID: 18 RVA: 0x0000314C File Offset: 0x0000134C FindRandomPreSpacerPawnForSpawn
        private static void GenerateSlave(Building_XenomorphCryptosleepCasket pod)
        {
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomPreSpacerPawnForSpawn(), Faction.OfAncients, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            HealthUtility.DamageUntilDowned(pawn, true);
            if (Rand.Chance(0.75f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                pawn.health.AddHediff(XenomorphDefOf.RRY_FaceHuggerInfection, part);
            }
            Building_XenomorphCryptosleepCasket.GiveRandomLootInventoryForTombPawn(pawn);
            bool flag = Rand.Value < 0.5f;
            if (flag)
            {
                HealthUtility.DamageUntilDead(pawn);
            }
            bool flag2 = !pod.TryAcceptThing(pawn, false);
            if (flag2)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x06000013 RID: 19 RVA: 0x00003200 File Offset: 0x00001400
        private static void GenerateAngryAncient(Building_XenomorphCryptosleepCasket pod)
        {
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomSpacerPawnForSpawn(), Faction.OfAncientsHostile, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            if (Rand.Chance(0.15f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                pawn.health.AddHediff(XenomorphDefOf.RRY_FaceHuggerInfection, part);
            }
            else if (Rand.Chance(0.15f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Torso);
                HediffDef hediff = Rand.Chance(0.75f) ? XenomorphDefOf.RRY_HiddenXenomorphImpregnation : XenomorphDefOf.RRY_HiddenNeomorphImpregnation;
                pawn.health.AddHediff(hediff, part);
            }
            Building_XenomorphCryptosleepCasket.GiveRandomLootInventoryForTombPawn(pawn);
            bool flag = !pod.TryAcceptThing(pawn, false);
            if (flag)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00003290 File Offset: 0x00001490
        private static void GenerateHalfEatenAncient(Building_XenomorphCryptosleepCasket pod)
        {
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(Building_XenomorphCryptosleepCasket.FindRandomSpacerPawnForSpawn(), Faction.OfAncients, (PawnGenerationContext)2, (pod.Map != null ? pod.Map.Tile : pod.MapHeld.Tile), false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            HediffSet hediffSet = pawn.health.hediffSet;
            int num = Rand.Range(5, 10);
            for (int i = 0; i < num; i++)
            {
                BodyPartRecord bodyPartRecord = GenCollection.RandomElementByWeight<BodyPartRecord>(Building_XenomorphCryptosleepCasket.HittablePartsViolence(hediffSet), (BodyPartRecord x) => x.coverageAbs);
                DamageDef bite = DamageDefOf.Bite;
                float num2 = (float)Rand.Range(3, 8);
                float num3 = 999f;
                BodyPartRecord bodyPartRecord2 = bodyPartRecord;
                DamageInfo damageInfo;
                damageInfo = new DamageInfo(bite, num2, num3, -1f, null, bodyPartRecord2, null, 0, null);
                pawn.TakeDamage(damageInfo);
            }
            if (Rand.Chance(0.75f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                pawn.health.AddHediff(XenomorphDefOf.RRY_FaceHuggerInfection, part);
            }
            else if (Rand.Chance(0.5f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Torso);
                HediffDef hediff = Rand.Chance(0.75f) ? XenomorphDefOf.RRY_HiddenXenomorphImpregnation : XenomorphDefOf.RRY_HiddenNeomorphImpregnation;
                pawn.health.AddHediff(hediff, part);
            }
            Building_XenomorphCryptosleepCasket.GiveRandomLootInventoryForTombPawn(pawn);
            List<Pawn> list = new List<Pawn>();
            float value = Rand.Value;
            bool flag = (double)value < 0.1;
            int num4;
            if (flag)
            {
                list.Add(PawnGenerator.GeneratePawn(XenomorphDefOf.RRY_Xenomorph_Warrior, null));
                num4 = 1;
            }
            else
            {
                bool flag2 = (double)value < 0.35;
                if (flag2)
                {
                    list.Add(PawnGenerator.GeneratePawn(XenomorphDefOf.RRY_Xenomorph_Drone, null));
                    num4 = 1;
                }
                else
                {
                    num4 = Rand.Range(3, 6);
                    for (int j = 0; j < num4; j++)
                    {
                        list.Add(PawnGenerator.GeneratePawn(XenomorphDefOf.RRY_Xenomorph_FaceHugger , null));
                    }
                }
            }
            for (int k = 0; k < num4; k++)
            {
                Pawn pawn2 = list[k];
                bool flag3 = !pod.TryAcceptThing(pawn2, false);
                if (flag3)
                {
                    Find.WorldPawns.PassToWorld(pawn2, (PawnDiscardDecideMode)2);
                    break;
                }
                //pawn2.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, null, false);
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00003024 File Offset: 0x00001224
        private void GenerateFromPawnkindList(Building_XenomorphCryptosleepCasket pod)
        {
            PawnGenerationRequest pawnGenerationRequest;
            pawnGenerationRequest = new PawnGenerationRequest(OfPawnKinds.RandomElement(), OfFaction, (PawnGenerationContext)2, -1, false, false, false, false, true, false, 1f, false, true, true, false, true, false, false);
            Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
            if (Def.infectionDef!=null)
            {
                BodyPartRecord part = null;
                if (Def.infectionDef == XenomorphDefOf.RRY_FaceHuggerInfection)
                {
                    part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Head);
                }
                else if (Def.infectionDef != null && (Def.infectionDef == XenomorphDefOf.RRY_HiddenNeomorphImpregnation || Def.infectionDef == XenomorphDefOf.RRY_HiddenXenomorphImpregnation|| Def.infectionDef == XenomorphDefOf.RRY_XenomorphImpregnation))
                {
                    part = pawn.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Torso);
                }
                pawn.health.AddHediff(Def.infectionDef, part);
            }
            bool flag = Def.spawnThing!=null;
            if (flag)
            {
                ThingDef namedSilentFail = Def.spawnThing;
                Building_XenomorphCryptosleepCasket.MakeIntoContainer(this.innerContainer, namedSilentFail, 1);
            }

            //    Building_XenomorphCryptosleepCasket.GiveRandomLootInventoryForTombPawn(pawn);
            bool flag2 = !pod.TryAcceptThing(pawn, false);
            if (flag2)
            {
                Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode)2);
            }
        }

        // Token: 0x06000015 RID: 21 RVA: 0x000034AC File Offset: 0x000016AC
        private static void GiveRandomLootInventoryForTombPawn(Pawn p)
        {
            float value = Rand.Value;
            bool flag = value < 0.05f;
            if (flag)
            {
                ThingDef namedSilentFail = DefDatabase<ThingDef>.GetNamedSilentFail("Gun_RailgunMKI");
                bool flag2 = namedSilentFail == null;
                if (flag2)
                {
                    namedSilentFail = DefDatabase<ThingDef>.GetNamedSilentFail("Gun_ChargeLance");
                }
                Building_XenomorphCryptosleepCasket.MakeIntoContainer(p.inventory.innerContainer, namedSilentFail, 1);
            }
            bool flag3 = value > 0.35f && value < 0.6f;
            if (flag3)
            {
                Building_XenomorphCryptosleepCasket.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentSpacer, Rand.Range(-1, 5));
            }
            bool flag4 = value < 0.45f;
            if (flag4)
            {
                Building_XenomorphCryptosleepCasket.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Gold, Rand.Range(10, 50));
            }
            else
            {
                bool flag5 = value < 0.65f;
                if (flag5)
                {
                    Building_XenomorphCryptosleepCasket.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Uranium, Rand.Range(5, 60));
                }
                else
                {
                    Building_XenomorphCryptosleepCasket.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Plasteel, Rand.Range(10, 50));
                }
            }
            Building_XenomorphCryptosleepCasket.MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentIndustrial, Rand.Range(-1, 6));
        }

        // Token: 0x06000016 RID: 22 RVA: 0x000035D8 File Offset: 0x000017D8
        private static void MakeIntoContainer(ThingOwner container, ThingDef def, int count)
        {
            bool flag = def == null || count <= 0;
            if (!flag)
            {
                Thing thing = ThingMaker.MakeThing(def, null);
                thing.stackCount = count;
                container.TryAdd(thing, true);
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00003614 File Offset: 0x00001814
        private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
        {
            return from x in bodyModel.GetNotMissingParts(0, 0, null, null)
                   where (int)x.depth == 2 || ((int)x.depth == 1 && x.def.IsSolid(x, bodyModel.hediffs))
                   select x;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003654 File Offset: 0x00001854
        private static PawnKindDef FindRandomSpacerPawnForSpawn()
        {
            GenCollection.TryRandomElement<PawnKindDef>(from td in DefDatabase<PawnKindDef>.AllDefs
                                                        where td.RaceProps.Humanlike && (td.defaultFactionDef != null && td.defaultFactionDef.techLevel > (TechLevel)4) && td.combatPower < 200f
                                                        select td, out PawnKindDef result);
            return result;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003654 File Offset: 0x00001854
        private static PawnKindDef FindRandomPreSpacerPawnForSpawn()
        {
            GenCollection.TryRandomElement<PawnKindDef>(from td in DefDatabase<PawnKindDef>.AllDefs
                                                        where td.RaceProps.Humanlike && (td.defaultFactionDef != null && td.defaultFactionDef.techLevel < (TechLevel)5) && td.combatPower < 200f
                                                        select td, out PawnKindDef result);
            return result;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003654 File Offset: 0x00001854
        private static PawnKindDef FindRandomAnimalForSpawn()
        {
            GenCollection.TryRandomElement<PawnKindDef>(from td in DefDatabase<PawnKindDef>.AllDefs
                                                        where td.RaceProps.Animal && td.combatPower < 200f
                                                        select td, out PawnKindDef result);
            return result;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00003654 File Offset: 0x00001854
        private static PawnKindDef FindRandomXenomorphForSpawn()
        {
            GenCollection.TryRandomElement<PawnKindDef>(from td in DefDatabase<PawnKindDef>.AllDefs
                                                        where td.race.defName.Contains("RRY_Xenomorph") && td.combatPower < 200f
                                                        select td, out PawnKindDef result);
            return result;
        }
    }
}
