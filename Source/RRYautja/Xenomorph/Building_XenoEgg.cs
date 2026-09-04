using RimWorld;
using RimWorld.Planet;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RRYautja
{
    public class Building_XenoEgg : Building
    {
        public Map MyMap => this.Map ?? this.MapHeld;
        public IntVec3 MyPos => this.Position != null ? this.Position : this.PositionHeld;
        bool selected => Find.Selector.SelectedObjects.Contains(this) && Prefs.DevMode && DebugSettings.godMode;
        public bool QueenPresent => MyMap.mapPawns.AllPawnsSpawned.Any(x => x.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen) || (XenomorphUtil.HivelikesPresent(MyMap) && ((XenomorphUtil.SpawnedHivelikes(MyMap)).Any<HiveLike>((HiveLike y) => y.hasQueen)));
        public float hatchRange => xenoHatcher?.Props.triggerRadius ?? 5f;
        public float minGestationTemp => xenoHatcher?.Props.minGestationTemp ?? -30f;
        public bool NormalEgg => this.eggType == EggType.Normal;
        public bool RoyaleEgg => this.eggType == EggType.Royal;
        public bool PraetorianEgg => this.eggType == EggType.Praetorian;
        public bool EggMutating => this.eggState == EggState.Mutating;
        public bool EggMutated => this.eggState == EggState.Mutated;
        public bool RoyalEggPresent => EggPresent(EggType.Royal);
        public bool RoyalEggsPresent(out List<Thing> eggList)
        {
            bool result = EggsPresent(EggType.Royal, out eggList);
            return result;
        }
        public bool EggPresent(EggType state) => MyMap.listerThings.ThingsOfDef(this.def).Any(x => x as Building_XenoEgg != null && x as Building_XenoEgg is Building_XenoEgg hatcher && (hatcher.eggType == state || hatcher.eggType == EggType.All) && x != this);
        public bool EggsPresent(EggType state, out List<Thing> eggList)
        {
            eggList = MyMap.listerThings.ThingsOfDef(this.def).ToList().FindAll(x => x as Building_XenoEgg != null && x as Building_XenoEgg is Building_XenoEgg hatcher && (hatcher.eggType == state || hatcher.eggType == EggType.All) && x != this);
            bool result = !eggList.NullOrEmpty();
            return result;
        }
        public bool EggsPresent(EggType state, out List<Thing> eggList, float MaxRange)
        {
            eggList = MyMap.listerThings.ThingsOfDef(this.def).ToList().FindAll(x => x as Building_XenoEgg != null && x as Building_XenoEgg is Building_XenoEgg hatcher && (hatcher.eggType == state || hatcher.eggType == EggType.All) && x != this && hatcher.MyPos.InHorDistOf(this.MyPos, hatchRange));
            bool result = !eggList.NullOrEmpty();
            return result;
        }
        public bool EggsPresent(GestationState state, float MaxRange, out List<Building_XenoEgg> eggList, EggType type = EggType.All)
        {
            eggList = new List<Building_XenoEgg>();
            foreach (var item in MyMap.listerThings.ThingsOfDef(this.def).ToList().FindAll(x => x as Building_XenoEgg != null && x as Building_XenoEgg is Building_XenoEgg hatcher && (hatcher.eggType == type || type == EggType.All) && x != this && hatcher.MyPos.InHorDistOf(this.MyPos, hatchRange)))
            {
                eggList.Add((Building_XenoEgg)item);
            }
            
            bool result = !eggList.NullOrEmpty();
            return result;
        }
        public Thing targetThing = null;
        public bool HostsPresent() => HostsPresent(out List<Pawn> hostList);
        public bool HostsPresent(out List<Pawn> hostList)
        {
            hostList = MyMap.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.isPotentialHost() && x.Position.InHorDistOf(this.MyPos, hatchRange));
            return !hostList.NullOrEmpty();
        }
        public bool Gestating(out string reason)
        {
            reason = string.Empty;
            if (this.AmbientTemperature < minGestationTemp)
            {
                reason += string.Format("\nToo Cold: {0} min temp: {1}", this.AmbientTemperature, minGestationTemp);
            }
            if (eggState == EggState.Mutating)
            {
                reason += string.Format("\nMutating");
            }
            if (gestateProgress<1f)
            {
                reason += string.Format("\nProgressing");
            }
            switch (this.eggStatus)
            {
                case GestationState.Halted:
                    reason = "Halted: "+reason;
                    return false;
                case GestationState.Finished:
                    reason = "Finished";
                    return false;
                default:
                    return true;
            }
        }
        public bool readyToHatch
        {
            get
            {
                return this.eggStatus == GestationState.Finished;
            }
        }
        public bool shouldHatch
        {
            get
            {
                if (HostsPresent(out List<Pawn> hostList))
                {
                    if (targetThing.DestroyedOrNull())
                    {
                        return false;
                    }
                    List<Building_XenoEgg> egglist = new List<Building_XenoEgg>();
                    bool eggs = EggsPresent(GestationState.Finished, hatchRange, out egglist, EggType.All);
                    if (eggs && egglist.Any(x=> x != this && (x.targetThing != null && x.targetThing == targetThing)))
                    {
                        return false;
                    }
                    int huggercount = targetThing != null ? XenomorphUtil.SpawnedFacehuggerPawns(MyMap, (int)hatchRange, (Pawn)targetThing).Where(x=> x.mindState.enemyTarget == targetThing).Count() : 0;
                    int hostcount = targetThing != null ? hostList.Count : 0;
                    return targetThing != null && huggercount + spawnCount <= hostcount; //&& hostList.Any(x=> !MyMap.HiveGrid().eggHosts.Contains(x));
                }
                return false;
            }
        }
        public bool RoyalPresent
        {
            get
            {
                Func<Pawn, bool> validator = delegate (Pawn t)
                {
                    bool RoyalHugger = t.kindDef == RoyalKindDef;
                    bool RoyalHuggerInfection = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection).TryGetComp<HediffComp_XenoFacehugger>().RoyaleHugger);
                    bool RoyalImpregnation = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_XenomorphImpregnation) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_XenomorphImpregnation).TryGetComp<HediffComp_XenoSpawner>().RoyaleHugger);
                    bool RoyalHiddenImpregnation = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_HiddenXenomorphImpregnation) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_HiddenXenomorphImpregnation).TryGetComp<HediffComp_XenoSpawner>().RoyaleHugger);
                    return RoyalHugger || RoyalHuggerInfection || RoyalImpregnation || RoyalHiddenImpregnation || QueenPresent;
                };
                return MyMap.mapPawns.AllPawnsSpawned.Any((Func<Pawn, bool>)validator);
            }
        }
        public Vector2 DrawSize
        {
            get
            {
                float basic = 0f;
                switch (eggType)
                {
                    case Building_XenoEgg.EggType.Praetorian:
                        basic += 0.10f;
                        break;
                    case Building_XenoEgg.EggType.Royal:
                        basic += 0.25f;
                        break;
                    case Building_XenoEgg.EggType.Hyperfertile:
                        basic += 0.5f;
                        break;
                    default:
                        break;
                }
                float num = Math.Max((basic * mutateProgress),0f);
                return  new Vector2( 1f + (num) , 1f + (num) );
            }
        }

        public override Graphic Graphic
        {
            get
            {
                try
                {
                    string path = this.DefaultGraphic.path;
                    Graphic graphic = base.Graphic.GetCopy(DrawSize);
                    if (graphic == null) return base.Graphic;
                    Color color = Color.white;
                    Color colortwo = Color.white;
                switch (eggType)
                {
                    case Building_XenoEgg.EggType.Hyperfertile:
                        color = new Color(200f,200f,200f);
                        path += "Hyperfertile";
                        break;
                    case Building_XenoEgg.EggType.Praetorian:
                        color = new Color(150f, 150f, 150f);
                        break;
                    case Building_XenoEgg.EggType.Royal:
                        color = new Color(150f, 100f, 100f);
                        path += "Royal";
                        break;
                    default:
                        break;
                }
                if (targetThing!=null && readyToHatch && shouldHatch)
                {
                    if (targetThing.Position.InHorDistOf(MyPos, hatchRange))
                    {
                        path += "_Open";
                    }

                }
                graphic.color = color;
                graphic.path = path;
                graphic.drawSize = DrawSize;
                return graphic.GetCopy(DrawSize);
                }
                catch
                {
                    return base.Graphic;
                }
            //    return graphic.GetColoredVersion(ShaderTypeDefOf.Cutout.Shader, color, colortwo);
            }
        }

        public CompXenoHatcher xenoHatcher
        {
            get
            {
                return this.TryGetComp<CompXenoHatcher>();
            }
        }

        protected override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (this.Faction==null)
                {
                    this.SetFaction(Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph));
                }
                if (this.hatcheeFaction == null)
                {
                    this.hatcheeFaction = Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph);
                }
            }

            float ambientTemperature = this.AmbientTemperature;
            float num = 1f / (xenoHatcher.Props.hatcherDaystoHatch * 60000f);

            if (ambientTemperature > minGestationTemp && Map != null)
            {
                if (this.eggState == EggState.Mutating)
                {
                    this.mutateProgress += num;
                    if (this.mutateProgress >= 1f)
                    {
                        Rand.PushState();
                        spawncount = eggType == EggType.Hyperfertile ? Rand.RangeSeeded(1, 5, this.thingIDNumber) : 1;
                        Rand.PopState();
                    }
                }
                else
                {
                    if (this.Gestating(out string fail))
                    {
                        this.gestateProgress += num;
                    }
                }
                if (Find.TickManager.TicksGame % 250 == 0)
                {
                    if (!targetThing.DestroyedOrNull())
                    {
                        Pawn pawn = targetThing as Pawn;
                        if (pawn!=null)
                        {
                            if (!pawn.isPotentialHost())
                            {
                                targetThing = null;
                            }
                        }
                        else
                        {
                            targetThing = null;
                        }
                    }
                    if (this.readyToHatch && this.willHatch)
                    {
                        this.Hatch();
                    }
                    else
                    {
                        this.TickRare();
                    }

                }
            }
        }

        public override void TickRare()
        {
            base.TickRare();
            if (settings.SettingsHelper.latest.AllowXenoEggMetamorph && this.NormalEgg)
            {
                List<Thing> HEggs = new List<Thing>();
                List<Thing> PEggs = new List<Thing>();
                flagRoyal = !(QueenPresent || RoyalPresent || RoyalEggPresent);
                bool heggs = EggsPresent(EggType.Hyperfertile, out HEggs);
                bool peggs = EggsPresent(EggType.Praetorian, out PEggs);
                flagHyper = XenomorphUtil.TotalSpawnedXenomorphPawnCount(MyMap) < 10 && (!heggs || HEggs.Count < 3);
                flagPrae = false && (!peggs || PEggs.Count < 3);
                flagMutate = flagRoyal || flagHyper || flagPrae;
                if (flagMutate && this.gestateProgress > 0.25f && this.Gestating(out string fr))
                {
                    TryMutate();
                }
            }
            if (!readyToHatch)
            {
                return;
            }
            Pawn pawn = null;
            if (targetThing.DestroyedOrNull())
            {
                if (!TryFindHost(out pawn))
                {
                    return;
                }
                else
                {
                    targetThing = pawn;
                }
            }
            if (targetThing != null)
            {
                if (pawn == null)
                {
                    pawn = targetThing as Pawn;
                }
                if (pawn.isXenoHost())
                {
                    return;
                }
                if (pawn.Position.InHorDistOf(MyPos, hatchRange))
                {
                    if (canHatch && shouldHatch)
                    {
                        float thingdist = MyPos.DistanceTo(pawn.Position);
                        float thingsize = pawn.BodySize;
                        float thingstealth = targetThing.GetStatValue(StatDefOf.HuntingStealth);
                        float thingmovespeed = targetThing.GetStatValue(StatDefOf.MoveSpeed);
                        if (selected)
                        {
                            //    Log.Message(string.Format("distance between {1} @{3} and {2} @ {4}: {0}, gestateProgress: {5}, mutateProgress: {6}", MyPos.DistanceTo(pawn.Position), this.parent.LabelShort, pawn.Label, MyPos, pawn.Position , gestateProgress, mutateProgress));
                            //    Log.Message(string.Format("{0} thingsize: {1}, thingstealth: {2}, thingmovespeed: {3}", pawn.Label, thingsize, thingstealth, thingmovespeed));
                        }

                        // Hatch formula: distance makes it harder, bodySize makes it easier
                        // bodySize scales from 0.3 (minimum, harder to trigger) to 1.0+ (full trigger chance)
                        float sizeFactor = Mathf.Clamp(thingsize, 0.3f, 1.0f);
                        // Normalize 0.3->1.0 range to 0.0->1.0 scale
                        float sizeBonus = (sizeFactor - 0.3f) / 0.7f; // 0.3->0, 0.65->0.5, 1.0->1.0
                        float hatchon = ((10 * thingdist) - (thingsize * 5) - (sizeBonus * 10));
                        lasthatchon = hatchon;
                        Rand.PushState();
                        float roll = thingstealth > 0 ? (Rand.RangeInclusive(0, 100) * (1 - (1 * thingstealth))) : (Rand.RangeInclusive(0, 100));
                        lastspawnroll = roll;
                        Rand.PopState();
                        if (roll > hatchon)
                        {
                            this.willHatch = true;
                        }
                        if (selected)
                        {
                            //    Log.Message(string.Format("{0} hatchon: {1}, roll: {2}, willHatch: {3}", pawn.Label, hatchon, roll, willHatch));
                        }
                    }
                }
                else
                {
                    if (pawn == null)
                    {
                        pawn = targetThing as Pawn;
                    }
                    targetThing = null;
                }
            }
        }

        public override string GetInspectString()
        {
            string des = base.GetInspectString();
            if (!this.Gestating(out string reason))
            {
                des = "Xeno_Egg_Gestation_Progress".Translate() + ": " + GetDescription(eggStatus);
                if (eggStatus != GestationState.Finished)
                {
                    des += " " + this.gestateProgress.ToStringPercent();
                }
                if (EggMutating)
                {
                    des += "\n" + GetDescription(eggType) + " " + "Xeno_Egg_Mutation_Progress".Translate() + ": " + this.mutateProgress.ToStringPercent();
                }
                if (EggMutated)
                {
                    des += "\n" + GetDescription(eggState);
                }
            }
            else
            {
                des = "Xeno_Egg_Gestation_Progress".Translate() + ": " + GetDescription(eggStatus) + " " + this.gestateProgress.ToStringPercent() + "\n" + GetDescription(eggType);
                if (EggMutated)
                {
                    des += "\n" + GetDescription(eggState);
                }
            }
            if (selected)
            {
                if (targetThing != null)
                {
                    if (EggMutated)
                    {
                        if (eggType == EggType.Hyperfertile)
                        {
                            des += " contains Facehugger X" + spawnCount;
                        }
                        if (RoyaleEgg)
                        {
                            des += " contains Royal Facehugger";
                        }
                    }
                    des += "\nTargeting: " + targetThing.LabelShortCap + ", Distance: " + MyPos.DistanceTo(targetThing.Position) + ", Max:" + hatchRange;
                    des += "\nCan Hatch: " + this.canHatch + ", Should Hatch: " + this.shouldHatch + ", Will Hatch: " + this.willHatch;
                    des += "\nlastspawnroll: " + this.lastspawnroll + " lasthatchon: " + this.lasthatchon;

                }
            }
            return des;
        }

        public void TryMutate()
        {
            //    Log.Message("TryMutate");
            float num = 1f / (xenoHatcher.Props.hatcherDaystoHatch * 60000f);
            float chance = 0.1f;
            EggType state = eggType;
            if (flagRoyal)
            {
                state = EggType.Royal;
                chance += 0.5f;
            }
            else if (flagPrae)
            {
                state = EggType.Praetorian;
                chance += 0.1f;
            }
            else if (flagHyper)
            {
                state = EggType.Hyperfertile;
            }
            Rand.PushState();
            flagMutate = flagMutate && Rand.Chance(chance);
            Rand.PopState();
            if (flagMutate)
            {
                //    Log.Message("Mutated");
                eggType = state;
                this.mutateProgress += num;
            }
            else
            {
                //    Log.Message("Mutation failed");
            }
        }

        public bool TryFindHost(out Pawn Host)
        {
            Host = targetThing as Pawn;
            if (HostsPresent(out List<Pawn> hosts))
            {

                List<Building_XenoEgg> egglist = new List<Building_XenoEgg>();
                bool eggs = EggsPresent(GestationState.Finished, hatchRange, out egglist, EggType.All);

                hosts = hosts.Where(x => !egglist.Any(y => y != this && (y.targetThing != null && y.targetThing == x))).ToList();
                if (hosts.Any())
                {
                    Host = GenClosest.ClosestThingReachable(MyPos, MyMap, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), hatchRange, x => x.isPotentialHost() && hosts.Contains(x), null, 0, -1, false, RegionType.Set_Passable, false) as Pawn;
                }
            }
            return !Host.DestroyedOrNull();
        }

        public void Hatch()
        {
            try
            {
                PawnKindDef hatchKindDef = eggType == EggType.Royal ? Building_XenoEgg.RoyalKindDef : Building_XenoEgg.NormalKindDef;
                PawnGenerationRequest request = new PawnGenerationRequest(hatchKindDef, this.hatcheeFaction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, false, false, false, false);
                for (int i = 0; i < spawncount; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    if (TrySpawnHatchedOrBornPawn(pawn))
                    {
                        if (pawn != null)
                        {
                            if (this.hatcheeParent != null)
                            {
                                if (pawn.playerSettings != null && this.hatcheeParent.playerSettings != null && this.hatcheeParent.Faction == this.hatcheeFaction)
                                {
                                    pawn.playerSettings.AreaRestrictionInPawnCurrentMap = this.hatcheeParent.playerSettings.AreaRestrictionInPawnCurrentMap;
                                }
                                if (pawn.RaceProps.IsFlesh)
                                {
                                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, this.hatcheeParent);
                                }
                            }
                            if (this.otherParent != null && (this.hatcheeParent == null || this.hatcheeParent.gender != this.otherParent.gender) && pawn.RaceProps.IsFlesh)
                            {
                                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, this.otherParent);
                            }
                            pawn.jobs.StartJob(new Job(JobDefOf.AttackMelee, targetThing));
                        }
                        if (this.Spawned)
                        {
                            FilthMaker.TryMakeFilth(MyPos, MyMap, ThingDefOf.Filth_AmnioticFluid, 1);
                        }
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    }
                }
            }
            finally
            {
                
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public string GetDescription(EggType category)
        {
            switch (category)
            {
                case EggType.Hyperfertile:
                    return "Xeno_Egg_Hyperfertile".Translate(spawnCount);
                case EggType.Praetorian:
                    return "Xeno_Egg_Praetorian".Translate();
                case EggType.Royal:
                    return "Xeno_Egg_Royal".Translate();
                default:
                    return "Xeno_Egg".Translate();
            }
        }
        public string GetDescription(EggState state)
        {
            switch (state)
            {
                case EggState.Mutating:
                    return "Xeno_Egg_Mutating".Translate();
                case EggState.Mutated:
                    return "Xeno_Egg_Mutated".Translate();
                default:
                    return string.Empty;
            }
        }
        public string GetDescription(GestationState state)
        {
            switch (state)
            {
                case GestationState.Halted:
                    return "Xeno_Egg_Halted".Translate();
                case GestationState.Finished:
                    return "Xeno_Egg_Finished".Translate();
                default:
                    return "Xeno_Egg_Progressing".Translate();
            }
        }

        public EggType eggType = EggType.Normal;
        public EggState eggState
        {
            get
            {
                EggState eggState = EggState.Normal;
                if (this.mutateProgress > 0f)
                {
                    eggState = EggState.Mutating;
                }
                if (this.mutateProgress >= 1f)
                {
                    eggState = EggState.Mutated;
                }
                return eggState;
            }
        }
        public GestationState eggStatus
        {
            get
            {
                GestationState eggStatus = GestationState.Progressing;
                if (this.gestateProgress>=1f)
                {
                    eggStatus = GestationState.Finished;
                }
                else
                {
                    if (this.EggMutating || this.AmbientTemperature < minGestationTemp || Map == null)
                    {
                        eggStatus = GestationState.Halted;
                    }
                }
                return eggStatus;
            }
        }

        public bool TrySpawnHatchedOrBornPawn(Pawn pawn)
        {
            if (this.SpawnedOrAnyParentSpawned)
            {
                return GenSpawn.Spawn(pawn, MyPos, MyMap, WipeMode.Vanish) != null;
            }
            return false;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.def.Minifiable) // && base.Faction == Faction.OfPlayer)
            {
                yield return InstallationDesignatorDatabase.DesignatorFor(this.def);
            }
            yield break;
        }
        public override void PostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
        {
            base.PostGeneratedForTrader(trader, forTile, forFaction);
            this.hatcheeFaction = Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph);
        }

        public Pawn hatcheeParent;
        public Pawn otherParent;
        public Faction hatcheeFaction;
        public float gestateProgress;
        public float mutateProgress;
        public float spawncount;
        private float lastspawnroll = 0f;
        private float lasthatchon = 0f;
        public float spawnCount
        {
            get
            {
                if (spawncount==0)
                {
                    Rand.PushState();
                    spawncount = eggType == EggType.Hyperfertile ? Rand.RangeSeeded(1, 5, this.thingIDNumber) : 1;
                    Rand.PopState();
                }
                return spawncount;
            }
        }
        public bool canHatch
        {
            get
            {
                bool canHatch = false;
                if (targetThing!=null)
                {
                    if (targetThing as Pawn is Pawn pawn && pawn !=null)
                    {
                        canHatch = pawn.isPotentialHost();
                    }
                }
                return canHatch;
            }
        }

        public bool willHatch = false;
        public bool flagRoyal = false;
        public bool flagHyper = false;
        public bool flagPrae = false;
        public bool flagMutate = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.gestateProgress, "gestateProgress", 0f, false);
            Scribe_Values.Look<float>(ref this.mutateProgress, "mutateProgress", 0f, false);
            Scribe_Values.Look<float>(ref this.spawncount, "spawncount", 0, false);
            Scribe_Values.Look<bool>(ref this.flagRoyal, "flagRoyal", false, false);
            Scribe_Values.Look<bool>(ref this.flagHyper, "flagHyper", false, false);
            Scribe_Values.Look<bool>(ref this.flagPrae, "flagPrae", false, false);
            Scribe_Values.Look<bool>(ref this.flagMutate, "flagMutate", false, false);
            Scribe_Values.Look<EggType>(ref this.eggType, "eggType", EggType.Normal, false);
            Scribe_References.Look<Pawn>(ref this.hatcheeParent, "hatcheeParent", false);
            Scribe_References.Look<Pawn>(ref this.otherParent, "otherParent", false);
            Scribe_References.Look<Faction>(ref this.hatcheeFaction, "hatcheeFaction", false);
        }

        public enum EggType : byte
        {
            All,
            Normal,
            Hyperfertile,
            Praetorian,
            Royal
        }
        public enum EggState : byte
        {
            Normal,
            Mutating,
            Mutated
        }
        public enum GestationState : byte
        {
            Progressing,
            Halted,
            Finished
        }


        static PawnKindDef RoyalKindDef { get { return XenomorphDefOf.RRY_Xenomorph_RoyaleHugger; } }
        static PawnKindDef NormalKindDef { get { return XenomorphDefOf.RRY_Xenomorph_FaceHugger; } }
    }
}
