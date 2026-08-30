using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
    public class ThingDef_HiveLike : ThingDef
    {
        public FactionDef Faction;
        public ThingDef TunnelDef;
        public ThingDef TunnelDefchild;
        public ThingDef HiveDefchild;
        public List<PawnKindDef> PawnKinds = new List<PawnKindDef>();
        public float maxSpawnPointsPerHive = 550f;
        public float initalSpawnPointsPerHive = 250f;
    }
    // Token: 0x020006EC RID: 1772
    public class HiveLike : ThingWithComps
    {
        public ThingDef_HiveLike Def
        {
            get
            {
                return this.def as ThingDef_HiveLike;
            }
        }
        
        public Faction OfFaction
        {
            get
            {
             //   Log.Message(string.Format("Faction: {0}", Find.FactionManager.FirstFactionOfDef(Def.Faction)));
                return Find.FactionManager.FirstFactionOfDef(Def.Faction);
            }
        }

        public FactionDef OfFactionDef
        {
            get
            {
                //Log.Message(string.Format("FactionDef: {0}", Find.FactionManager.FirstFactionOfDef(Def.Faction)));
                return Def.Faction;
            }
        }

        public CompSpawnerHiveLikes hiveExpander
        {
            get
            {
                return this.TryGetComp<CompSpawnerHiveLikes>();
            }
        }

        public CompMaintainableLike hiveMaintainer
        {
            get
            {
                return this.TryGetComp<CompMaintainableLike>();
            }
        }

        public bool hiveNode
        {
            get
            {
                return this.def == XenomorphDefOf.RRY_Xenomorph_Hive;
            }
        }
        public bool hiveDormant
        {
            get
            {
                if (hiveExpander != null)
                {
                    return hiveExpander.canSpawnHiveLikes;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool hasQueen
        {
            get
            {
                bool hasqueen = false;
                if (!this.innerContainer.NullOrEmpty())
                {
                    hasqueen = this.innerContainer.Any(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen && x.xenomorph().HiveLoc == this.Position);
                }
                else
                if (!Map.HiveGrid().Queenlist.NullOrEmpty())
                {
                    hasqueen = Map.HiveGrid().Queenlist.Any(x=> x.xenomorph().HiveLoc == this.Position);
                }
                return hasqueen;
            }
        }

        public bool Childhive
        {
            get
            {
                return hiveExpander == null;
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
                else
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

        // Token: 0x040015B0 RID: 5552
        
        private float maxSpawnPointsPerHive = -1f;
        public float MaxSpawnedPawnsPoints
        {
            get
            {
                if (maxSpawnPointsPerHive==-1f)
                {
                    maxSpawnPointsPerHive = Def.maxSpawnPointsPerHive;
                }
                return maxSpawnPointsPerHive;
            }
            set
            {
                maxSpawnPointsPerHive = value;
            }
        }

        // Token: 0x040015B1 RID: 5553
        private float initialPawnsPoints = -1f;
        public float InitialPawnsPoints
        {
            get
            {
                if (initialPawnsPoints==-1f)
                {
                    initialPawnsPoints = Def.initalSpawnPointsPerHive;
                }
                return initialPawnsPoints;
            }
            set
            {
                initialPawnsPoints = value;
            }
        }
        // Token: 0x170005CD RID: 1485
        // (get) Token: 0x06002670 RID: 9840 RVA: 0x00123F94 File Offset: 0x00122394

        public List<HiveLike> childHiveLikes
        {
            get
            {
                List<HiveLike> blist = new List<HiveLike>();
                foreach (var item in XenomorphUtil.SpawnedHivelikes(Map))
                {
                    blist.Add((HiveLike)item);
                }
                return blist.FindAll(x => x.parentHiveLike == this);
            }
        }

        public int childHiveLikesCount
        {
            get
            {
                return childHiveLikes.Count;
            }
        }

        public Lord Lord
		{
			get
			{
                if (parentHiveLike!=null)
                {
                    if (parentHiveLike.Lord!=null)
                    {
                        return parentHiveLike.Lord;
                    }
                }
				Predicate<Pawn> hasDefendHiveLord = delegate(Pawn x)
				{
					Lord lord = x.GetLord();
					return lord != null && lord.LordJob is LordJob_DefendAndExpandHiveLike;
				};
				Pawn foundPawn = this.spawnedPawns.Find(hasDefendHiveLord);
				if (base.Spawned)
				{
					if (foundPawn == null)
					{
						RegionTraverser.BreadthFirstTraverse(this.GetRegion(RegionType.Set_Passable), (Region from, Region to) => true, delegate(Region r)
						{
							List<Thing> list = r.ListerThings.ThingsOfDef(Def.TunnelDef);
							for (int i = 0; i < list.Count; i++)
							{
								if (list[i] != this)
								{
									if (list[i].Faction == this.Faction)
									{
										foundPawn = ((HiveLike)list[i]).spawnedPawns.Find(hasDefendHiveLord);
										if (foundPawn != null)
										{
											return true;
										}
									}
								}
							}
							return false;
						}, 20, RegionType.Set_Passable);
					}
					if (foundPawn != null)
					{
						return foundPawn.GetLord();
					}
				}
				return null;
			}
		}

		// Token: 0x170005CE RID: 1486
		// (get) Token: 0x06002671 RID: 9841 RVA: 0x00124050 File Offset: 0x00122450
		private float SpawnedPawnsPoints
		{
			get
			{
				this.FilterOutUnspawnedPawns();
				float num = 0f;
				for (int i = 0; i < this.spawnedPawns.Count; i++)
				{
					num += this.spawnedPawns[i].kindDef.combatPower;
				}
				return num;
			}
		}

		// Token: 0x06002672 RID: 9842 RVA: 0x0012409F File Offset: 0x0012249F
		public void ResetStaticData()
		{
			spawnablePawnKinds.Clear();
            if (OfPawnKinds.Count > 0)
            {
                spawnablePawnKinds = OfPawnKinds;
            }
            else
            {
                if (OfFactionDef.basicMemberKind!=null)
                {
                    spawnablePawnKinds.Add(OfFactionDef.basicMemberKind);
                }
                else
                {
                //    Log.Error(string.Format("COuldnt find any pawnkinds of the {0} faction to spawn for {1}", OfFaction, OfTunnel.defName));
                }
            }
        //    Log.Error(string.Format("COuldnt find any pawnkinds of the {0} faction to spawn for {1}", OfFaction, OfTunnel.defName));
        }

		// Token: 0x06002673 RID: 9843 RVA: 0x001240D8 File Offset: 0x001224D8
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_HiveLike) && this.def == XenomorphDefOf.RRY_Xenomorph_Hive)
            {
                LessonAutoActivator.TeachOpportunity(XenomorphConceptDefOf.RRY_Concept_HiveLike, OpportunityType.Important);
            }
            base.SpawnSetup(map, respawningAfterLoad);
			if (base.Faction == null)
			{
				this.SetFaction(OfFaction, null);

            }
            if (this.getsQueen && hiveNode && !respawningAfterLoad && !hasQueen)
            {
                Pawn newQueen = PawnGenerator.GeneratePawn(new PawnGenerationRequest(XenomorphDefOf.RRY_Xenomorph_Queen, factionInt));
                this.innerContainer.TryAdd(newQueen);
                this.assignedQueen = newQueen;
                this.getsQueen = false; // Only spawn queen once
            }
			if (!respawningAfterLoad && this.active && canSpawnPawns)
            {
                this.SpawnInitialPawns();
            }
            else
            {
                spawnablePawnKinds = OfPawnKinds;
            }
            MapComponent_HiveGrid hiveGrid = map.HiveGrid();
            if (hiveGrid!=null)
            {
                if (this.def == XenomorphDefOf.RRY_Xenomorph_Hive)
                {
                    if (!hiveGrid.Hivelist.Contains(this))
                    {
                        hiveGrid.Hivelist.Add(this);
                    }
                    if (!hiveGrid.HiveLoclist.Contains(this.Position))
                    {
                        hiveGrid.HiveLoclist.Add(this.Position);
                    }
                }
                if (this.def == XenomorphDefOf.RRY_Xenomorph_Hive_Child)
                {
                    if (!hiveGrid.Hivelist.Contains(this))
                    {
                        hiveGrid.Hivelist.Add(this);
                    }
                    if (!hiveGrid.HiveLoclist.Contains(this.Position))
                    {
                        hiveGrid.HiveLoclist.Add(this.Position);
                    }
                }
            }
		}

		// Token: 0x06002674 RID: 9844 RVA: 0x00124110 File Offset: 0x00122510
		private void SpawnInitialPawns()
		{
			this.SpawnPawnsUntilPoints(InitialPawnsPoints);
			this.CalculateNextPawnSpawnTick();
		}

		// Token: 0x06002675 RID: 9845 RVA: 0x00124124 File Offset: 0x00122524
		public void SpawnPawnsUntilPoints(float points)
        {
            spawnablePawnKinds = OfPawnKinds;
            int num = 0;
			while (this.SpawnedPawnsPoints < points)
			{
				num++;
				if (num > 1000)
				{
					Log.Error("Too many iterations.");
					break;
				}
                if (!this.TrySpawnPawn(out Pawn pawn))
                {
                    break;
                }
            }
			this.CalculateNextPawnSpawnTick();
		}

		// Token: 0x06002676 RID: 9846 RVA: 0x0012417C File Offset: 0x0012257C
		protected override void Tick()
		{
			base.Tick();
			if (base.Spawned)
			{
				// Check if assigned Queen has died
				if (assignedQueen != null && (assignedQueen.Dead || assignedQueen.Destroyed))
				{
					AvPDebug.LogOnce("QueenDied", "Queen " + assignedQueen.LabelShort + " died - hive at " + Position + " stops spawning");
					canSpawnPawns = false;
					assignedQueen = null;
				}
				this.FilterOutUnspawnedPawns();
				if (!this.active && !base.Position.Fogged(base.Map) && canSpawnPawns)
				{
					this.Activate();
				}
				if (this.active && Find.TickManager.TicksGame >= this.nextPawnSpawnTick)
				{
					if (this.SpawnedPawnsPoints < MaxSpawnedPawnsPoints)
					{
                        bool flag = this.TrySpawnPawn(out Pawn pawn);
                        if (flag && pawn.caller != null)
						{
							pawn.caller.DoCall();
						}
					}
					this.CalculateNextPawnSpawnTick();
                }
                if (this.Map.skyManager.CurSkyGlow < 0.5f && this.active)
                {
                    if (this.innerContainer != null)
                    {
                        // Only eject pawns at night if there are hosts nearby to hunt
                        // Don't eject if recently entered (prevents in-out loop when hive is roofed)
                        if (Find.TickManager.TicksGame % 2500 == 0 && // Only check every ~42 seconds
                            ((this.innerContainer.Count == 1 && !this.innerContainer.Any(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen)) || this.innerContainer.Count > 1))
                        {
                            // Only eject if there are potential hosts nearby
                            Thing host = GenClosest.ClosestThingReachable(this.Position, this.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), Verse.AI.PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.ByPawn, Danger.Deadly), 30f, (x => x.isPotentialHost()));
                            if (host != null && !host.DestroyedOrNull())
                            {
                                this.innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near, null, null);
                            }
                        }

                    }
                }
                if (!this.innerContainer.NullOrEmpty())
                {
                    if (hiveMaintainer != null)
                    {
                        if (this.innerContainer.Count > 0 && hiveMaintainer.CurStage == MaintainableStage.NeedsMaintenance)
                        {
                            Thing thing = this.innerContainer.RandomElement();
                            this.innerContainer.TryDrop(thing, ThingPlaceMode.Near, out thing);
                        }
                    }
                    if (Find.TickManager.TicksGame % 10000 == 0)
                    {
                        Thing thing = GenClosest.ClosestThingReachable(this.Position, this.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), Verse.AI.PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.ByPawn, Danger.Deadly), 30f, (x => x.isPotentialHost()));
                        if (this.innerContainer.Count > 0 && !thing.DestroyedOrNull())
                        {
                            Pawn p = (Pawn)thing;
                            for (int i = 0; i < this.innerContainer.Count; i++)
                            {
                                Thing thing2 = this.innerContainer[i];
                                float chance = p.RaceProps.baseBodySize * p.ageTracker.CurLifeStage.bodySizeFactor / XenomorphUtil.DistanceBetween(this.Position, p.Position) * this.innerContainer.Count;
                                if (Rand.ChanceSeeded(chance, AvPConstants.AvPSeed))
                                {
                                    this.innerContainer.TryDrop(thing2, ThingPlaceMode.Near, out thing2);
                                }
                            }
                        }
                    }
                }

            }
		}

        // Token: 0x06002678 RID: 9848 RVA: 0x0012427C File Offset: 0x0012267C
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
			{
				Lord lord = this.Lord;
				if (lord != null)
				{
					lord.ReceiveMemo(HiveLike.MemoAttackedByEnemy);
				}
			}
			if (dinfo.Def == DamageDefOf.Flame && (float)this.HitPoints < (float)base.MaxHitPoints * 0.3f)
			{
				Lord lord2 = this.Lord;
				if (lord2 != null)
				{
					lord2.ReceiveMemo(HiveLike.MemoBurnedBadly);
				}
            }
            if (this.innerContainer.Count > 0)
            {
                this.EjectContents();
            }
            if (hiveNode)
            {
                if (this.innerContainer.Count > 0)
                {
                    this.EjectQueens();
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
		}

		// Token: 0x06002679 RID: 9849 RVA: 0x0012431C File Offset: 0x0012271C
		public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
		{
			if (base.Spawned && (dinfo == null || dinfo.Value.Category != DamageInfo.SourceCategory.Collapse))
			{
				List<Lord> lords = base.Map.lordManager.lords;
				for (int i = 0; i < lords.Count; i++)
				{
					lords[i].ReceiveMemo(HiveLike.MemoDestroyedNonRoofCollapse);
				}
			}
			base.Kill(dinfo, exactCulprit);
		}

        // Token: 0x0600267B RID: 9851 RVA: 0x00124448 File Offset: 0x00122848
        private void Activate()
		{
			this.active = true;
			this.SpawnInitialPawns();
			this.CalculateNextPawnSpawnTick();
			CompSpawnerHiveLikes comp = base.GetComp<CompSpawnerHiveLikes>();
			if (comp != null)
			{
				comp.CalculateNextHiveLikeSpawnTick();
			}
		}

		// Token: 0x0600267C RID: 9852 RVA: 0x0012447C File Offset: 0x0012287C
		private void CalculateNextPawnSpawnTick()
		{
			float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)this.spawnedPawns.Count);
			this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(HiveLike.PawnSpawnIntervalDays.RandomInRange * 60000f / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		// Token: 0x0600267D RID: 9853 RVA: 0x001244E8 File Offset: 0x001228E8
		private void FilterOutUnspawnedPawns()
		{
			for (int i = this.spawnedPawns.Count - 1; i >= 0; i--)
			{
				if (!this.spawnedPawns[i].Spawned)
				{
					this.spawnedPawns.RemoveAt(i);
				}
			}
		}

        // Token: 0x0600267E RID: 9854 RVA: 0x00124538 File Offset: 0x00122938
        private bool TrySpawnPawn(out Pawn pawn)
        {
            if (!this.canSpawnPawns)
            {
                pawn = null;
                return false;
            }
            float curPoints = this.SpawnedPawnsPoints;
            IEnumerable<PawnKindDef> source = from x in spawnablePawnKinds
                                              where curPoints + x.combatPower <= MaxSpawnedPawnsPoints
                                              select x;
            if (!source.TryRandomElement(out PawnKindDef kindDef))
            {
                pawn = null;
                return false;
            }
            if (!this.spawnedPawns.Any(x=> x.def!=XenomorphRacesDefOf.RRY_Xenomorph_Drone))
            {
                kindDef = XenomorphDefOf.RRY_Xenomorph_Drone;
            }
            pawn = PawnGenerator.GeneratePawn(kindDef, base.Faction);
            if (pawn.Faction == null && base.Faction != null)
            {
                pawn.SetFaction(base.Faction);
            }
            this.spawnedPawns.Add(pawn);
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 2, null), base.Map, WipeMode.Vanish);
            Lord lord = this.Lord;
            if (lord == null)
            {
                lord = this.CreateNewLord();
            }
            pawn.xenomorph().HiveLoc = this.Position;
            lord.AddPawn(pawn);
            DefDatabase<SoundDef>.GetNamedSilentFail("Hive_Spawn")?.PlayOneShot(this);
            return true;
        }

        private bool TrySpawnPawn(ThingOwner thingOwner, out Pawn pawn)
        {
            if (!this.canSpawnPawns)
            {
                pawn = null;
                return false;
            }
            float curPoints = this.SpawnedPawnsPoints;
            List<Pawn> sourced = null;
            foreach (Pawn p in innerContainer)
            {
                sourced.Add(p);
            }
            IEnumerable<Pawn> source = from x in sourced
                                       where curPoints + x.kindDef.combatPower <= MaxSpawnedPawnsPoints
                                       select x;
            if (!source.TryRandomElement(out Pawn kind))
            {
                pawn = null;
                return false;
            }
            pawn = kind;
            this.spawnedPawns.Add(pawn);
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 2, null), base.Map, WipeMode.Vanish);
            Lord lord = this.Lord;
            if (lord == null)
            {
                lord = this.CreateNewLord();
            }
            lord.AddPawn(pawn);
            DefDatabase<SoundDef>.GetNamedSilentFail("Hive_Spawn")?.PlayOneShot(this);
            return true;
        }

        // Token: 0x0600267F RID: 9855 RVA: 0x001245FC File Offset: 0x001229FC
        public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (Prefs.DevMode)
            {
                float curPoints = this.SpawnedPawnsPoints;
                string Desc = "All Possible Spawns: "+ OfPawnKinds.Count;
                Desc += "\n";
                Desc += "points remaining: " + (MaxSpawnedPawnsPoints - curPoints);
                Desc += "\n\n";
                foreach (PawnKindDef PKD in OfPawnKinds)
                {
                    Desc += PKD.LabelCap+", Points: "+PKD.combatPower;
                    Desc += "\n";
                }
                IEnumerable<PawnKindDef> source = from x in OfPawnKinds
                                                  where curPoints + x.combatPower <= MaxSpawnedPawnsPoints
                                                  select x;

                Desc += "\n";
                Desc += "Affordable Types: " + source.Count();
                Desc += "\n\n";
                foreach (PawnKindDef PKD in source)
                {
                    Desc += PKD.LabelCap + ", Points: " + PKD.combatPower;
                    Desc += "\n";
                }
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn pawn",
                    icon = TexCommand.ReleaseAnimals,
                    defaultDesc = Desc,
                    action = delegate ()
                    {
                        this.TrySpawnPawn(out Pawn pawn);
                    }
                };
                if (!this.innerContainer.NullOrEmpty())
                {
                    if (this.innerContainer.Any(x => x.def != XenomorphRacesDefOf.RRY_Xenomorph_Queen && x.xenomorph().HiveLoc == this.Position))
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "DEBUG: release all pawns",
                            icon = TexCommand.ReleaseAnimals,
                            defaultDesc = "Release all xenos inside",
                            action = delegate ()
                            {
                                this.EjectContents();
                            }
                        };
                    }
                    if (this.innerContainer.Any(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen && x.xenomorph().HiveLoc == this.Position))
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "DEBUG: release all Queens",
                            icon = TexCommand.ReleaseAnimals,
                            defaultDesc = "Release all Queens inside",
                            action = delegate ()
                            {
                                this.EjectQueens();
                            }
                        };
                    }
                }

                if (!hasQueen && this.parentHiveLike == null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEBUG: Spawn Queen",
                        icon = TexCommand.Install,
                        defaultDesc = "Adds a Queen to this hive",
                        action = delegate ()
                        {
                            Pawn newQueen = PawnGenerator.GeneratePawn(new PawnGenerationRequest(XenomorphDefOf.RRY_Xenomorph_Queen, factionInt));
                            this.innerContainer.TryAdd(newQueen);
                            this.assignedQueen = newQueen;
                        }
                    };
                }

            }
			yield break;
		}

		// Token: 0x06002680 RID: 9856 RVA: 0x00124620 File Offset: 0x00122A20
		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (this.spawnedPawns.Count > 0)
			{
				if (this.spawnedPawns.Any((Pawn p) => !p.Downed))
				{
					reason = this.def.label;
					return true;
				}
			}
			reason = null;
			return false;
		}

		// Token: 0x06002681 RID: 9857 RVA: 0x0012467E File Offset: 0x00122A7E
		private Lord CreateNewLord()
		{
			return LordMaker.MakeNewLord(base.Faction, new LordJob_DefendAndExpandHiveLike(!this.caveColony), base.Map, null);
		}
        /*
        // Token: 0x060024FB RID: 9467 RVA: 0x00116DF3 File Offset: 0x001151F3
        public virtual bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true);
        }

        // Token: 0x060024FC RID: 9468 RVA: 0x00116E04 File Offset: 0x00115204
        public virtual bool BaseTryAcceptThing(Thing thing, bool allowSpecialEffects = true)
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
                return true;
            }
            return false;
        }
        */
        // Token: 0x060024F3 RID: 9459 RVA: 0x00116CE3 File Offset: 0x001150E3
        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        // Token: 0x060024F4 RID: 9460 RVA: 0x00116CEB File Offset: 0x001150EB
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }


        public virtual void EjectContents()
        {
            if (this.innerContainer.Any(x => x.def != XenomorphRacesDefOf.RRY_Xenomorph_Queen && x.xenomorph().HiveLoc == this.Position))
            {
                List<Thing> contents = this.innerContainer.Where(x => x.def != XenomorphRacesDefOf.RRY_Xenomorph_Queen && x.xenomorph().HiveLoc == this.Position).ToList();
                foreach (Thing t in contents)
                {
                    this.innerContainer.Remove(t);
                    GenSpawn.Spawn(t, this.Position, base.Map);
                }
            }
            //    this.innerContainer.TryDropAll(this.Position, base.Map, ThingPlaceMode.Near, null, null);
        }

        public virtual void EjectQueens()
        {
            if (this.def == XenomorphDefOf.RRY_Xenomorph_Hive || (this.def == XenomorphDefOf.RRY_Xenomorph_Hive_Child && this.parentHiveLike.DestroyedOrNull()))
            {
                if (this.innerContainer.Any(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen && x.xenomorph().HiveLoc == this.Position))
                {
                    List<Thing> queens = this.innerContainer.Where(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen && x.xenomorph().HiveLoc == this.Position).ToList();
                    foreach (Thing queen in queens)
                    {
                        this.innerContainer.Remove(queen);
                        GenSpawn.Spawn(queen, this.Position, base.Map);
                    }
                }
            }
        }


        // Token: 0x06002677 RID: 9847 RVA: 0x00124224 File Offset: 0x00122624
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = base.Map;
            base.DeSpawn(mode);
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                lords[i].ReceiveMemo(HiveLike.MemoDeSpawned);
            }
            XenomorphHiveUtility.Notify_HiveLikeDespawned(this, map);
            MapComponent_HiveGrid hiveGrid = map.HiveGrid();
            if (hiveGrid != null)
            {
                if (this.def == XenomorphDefOf.RRY_Xenomorph_Hive)
                {
                    if (hiveGrid.Hivelist.Contains(this))
                    {
                        hiveGrid.Hivelist.Remove(this);
                    }
                    if (hiveGrid.HiveLoclist.Contains(this.Position))
                    {
                        hiveGrid.HiveLoclist.Remove(this.Position);
                    }
                }
                if (this.def == XenomorphDefOf.RRY_Xenomorph_Hive_Child)
                {
                    if (hiveGrid.HiveChildlist.Contains(this))
                    {
                        hiveGrid.HiveChildlist.Remove(this);
                    }
                    if (hiveGrid.HiveChildLoclist.Contains(this.Position))
                    {
                        hiveGrid.HiveChildLoclist.Remove(this.Position);
                    }
                }
            }
        }

        // Token: 0x060024FD RID: 9469 RVA: 0x00116E88 File Offset: 0x00115288
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.innerContainer.Count > 0)
            {
                this.EjectContents();
                this.EjectQueens();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (dinfo.Def==XenomorphDefOf.RRY_AcidBurn || dinfo.Def == XenomorphDefOf.RRY_AcidDamage)
            {
                absorbed = true;
            }
            base.PreApplyDamage(ref dinfo, out absorbed);
        }

        protected ThingOwner innerContainer => Map.HiveGrid().GetDirectlyHeldThings();
        public List<PawnKindDef> PawnKinds = new List<PawnKindDef>();
        public HiveLike parentHiveLike;
        public bool active = true;
        public int nextPawnSpawnTick = -1;
        public int lastHostSeenTick = -1;
        public List<Pawn> spawnedPawns = new List<Pawn>();
        public List<Pawn> spawnedHiveguardPawns = new List<Pawn>();
        public List<Pawn> spawnedWorkerPawns = new List<Pawn>();
        public List<Pawn> spawnedScoutPawns = new List<Pawn>();
        public bool caveColony;
		public bool canSpawnPawns = true;
        public bool getsQueen = true;
        public Pawn assignedQueen;
        public const int PawnSpawnRadius = 2;
		private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(0.85f, 1.15f);
        public List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();
        public static readonly string MemoAttackedByEnemy = "HiveAttacked";
		public static readonly string MemoDeSpawned = "HiveDeSpawned";
		public static readonly string MemoBurnedBadly = "HiveBurnedBadly";
		public static readonly string MemoDestroyedNonRoofCollapse = "HiveDestroyedNonRoofCollapse";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick", 0, false);
            Scribe_Values.Look<int>(ref this.lastHostSeenTick, "lastHostSeenTick", 0, false);
            Scribe_Collections.Look<Pawn>(ref this.spawnedPawns, "spawnedPawns", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.spawnedHiveguardPawns, "spawnedHiveguardPawns", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.spawnedWorkerPawns, "spawnedWorkerPawns", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.spawnedScoutPawns, "spawnedScoutPawns", LookMode.Reference, new object[0]);
            Scribe_Values.Look<bool>(ref this.caveColony, "caveColony", false, false);
            Scribe_Values.Look<bool>(ref this.canSpawnPawns, "canSpawnPawns", true, false);
            Scribe_Values.Look<bool>(ref this.getsQueen, "getsQueen", true, false);
            Scribe_References.Look<Pawn>(ref this.assignedQueen, "assignedQueen");
            Scribe_References.Look<HiveLike>(ref this.parentHiveLike, "parentHiveLike");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.spawnedPawns.RemoveAll((Pawn x) => x == null);
            }
        }
    }
}
