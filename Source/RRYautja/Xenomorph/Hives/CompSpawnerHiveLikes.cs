using RRYautja;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
    // Token: 0x02000768 RID: 1896
    public class CompProperties_SpawnerHiveLikes : CompProperties
    {
        // Token: 0x060029E9 RID: 10729 RVA: 0x0013D89C File Offset: 0x0013BC9C
        public CompProperties_SpawnerHiveLikes()
        {
            this.compClass = typeof(CompSpawnerHiveLikes);
        }
        public ThingDef tunnelthingDef;

        // Token: 0x04001742 RID: 5954
        public float HiveSpawnPreferredMinDist = 20f;

        // Token: 0x04001743 RID: 5955
        public float HiveSpawnRadius = 40f;

        // Token: 0x04001744 RID: 5956
        public FloatRange HiveSpawnIntervalDays = new FloatRange(2f, 3f);

        // Token: 0x04001745 RID: 5957
        public SimpleCurve ReproduceRateFactorFromNearbyHiveCountCurve = new SimpleCurve
        {
            {
                new CurvePoint(0f, 1f),
                true
            },
            {
                new CurvePoint(7f, 0.35f),
                true
            }
        };
    }
    // Token: 0x02000767 RID: 1895
    public class CompSpawnerHiveLikes : ThingComp
	{
		// Token: 0x17000679 RID: 1657
		// (get) Token: 0x060029DD RID: 10717 RVA: 0x0013D0CB File Offset: 0x0013B4CB
		private CompProperties_SpawnerHiveLikes Props
		{
			get
			{
				return (CompProperties_SpawnerHiveLikes)this.props;
			}
		}

        private HiveLike hiveLike
        {
            get
            {
                return (HiveLike)parent;
            }
        }
        
        public List<HiveLike> ChildHives
        {
            get
            {
                List<HiveLike> hives = new List<HiveLike>();
                foreach (var item in parent.Map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive_Child))
                {
                    hives.Add((HiveLike)item);
                }
                return hives;
            }
        }
        
        public float MaxTunnelSpawnRadius
        {
            get
            { // MinTunnelSpawnPreferredDist
                float num1 = ((ChildHives.Count + 1)*10) + Props.HiveSpawnRadius;
                if (num1 > parent.Map.Size.x)
                {
                    num1 = parent.Map.Size.x;
                }
                return num1;
            }
        }

        public float MinTunnelSpawnPreferredDist
        {
            get
            {
                float num1 = ((ChildHives.Count + 1) * 5) + Props.HiveSpawnPreferredMinDist;
                if (num1 > 50)
                {
                    num1 = 50;
                }

                return num1;
            }
        }
        // Token: 0x1700067A RID: 1658
        // (get) Token: 0x060029DE RID: 10718 RVA: 0x0013D0D8 File Offset: 0x0013B4D8
        private bool CanSpawnChildHiveLike
		{
			get
			{
				return this.canSpawnHiveLikes && XenomorphHiveUtility.TotalSpawnedHiveLikesCount(this.parent.Map) < 30;
			}
		}

		// Token: 0x060029DF RID: 10719 RVA: 0x0013D0FC File Offset: 0x0013B4FC
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.CalculateNextHiveLikeSpawnTick();
			}
		}

		// Token: 0x060029E0 RID: 10720 RVA: 0x0013D10C File Offset: 0x0013B50C
		public override void CompTick()
		{
			base.CompTick();
            int extra = 0;
            if (hiveLike.def == XenomorphDefOf.RRY_Xenomorph_Hive && canSpawnHiveLikes)
            {
                extra += hiveLike.GetDirectlyHeldThings().Count;
                foreach (var item in hiveLike.childHiveLikes)
                {
                    extra += item.GetDirectlyHeldThings().Count;
                }
                this.nextHiveSpawnTick -= extra;
            }

            if (this.parent is HiveLike hivelike && (hivelike == null || hivelike.active) && this.nextHiveSpawnTick <= 0)
			{
                if (this.TrySpawnChildHiveLike(MinTunnelSpawnPreferredDist, MaxTunnelSpawnRadius, out HiveLike t))
                {
                    Messages.Message("RRY_MessageHiveReproduced".Translate(), t, MessageTypeDefOf.NegativeEvent, true);
                }
                else
                {
                    this.CalculateNextHiveLikeSpawnTick();
                }
            }
            else
            if (this.parent is TunnelHiveLikeSpawner tunnellike && (tunnellike == null || tunnellike.active) && this.nextHiveSpawnTick <= 0)
            {
                if (this.TrySpawnChildHiveLike(MinTunnelSpawnPreferredDist, MaxTunnelSpawnRadius, out TunnelHiveLikeSpawner t))
                {
                    Messages.Message("RRY_MessageHiveReproduced".Translate(), t, MessageTypeDefOf.NegativeEvent, true);
                }
                else
                {
                    this.CalculateNextHiveLikeSpawnTick();
                }
            }
        }

		// Token: 0x060029E1 RID: 10721 RVA: 0x0013D188 File Offset: 0x0013B588
		public override string CompInspectStringExtra()
		{
			if (!this.canSpawnHiveLikes)
			{
				return "RRY_DormantHiveNotReproducing".Translate();
			}
			if (this.CanSpawnChildHiveLike)
            {
                if (this.parent.HitPoints<this.parent.MaxHitPoints && Find.TickManager.TicksGame % 250 == 0)
                {
                    this.parent.HitPoints++;
                }
                int extra = 0;
                if (hiveLike.def == XenomorphDefOf.RRY_Xenomorph_Hive && canSpawnHiveLikes)
                {
                    extra += hiveLike.GetDirectlyHeldThings().Count;
                    foreach (var item in hiveLike.childHiveLikes)
                    {
                        extra += item.GetDirectlyHeldThings().Count;
                    }
                }
                if (extra > 0)
                {
                    return "RRY_HiveReproducesIn".Translate() + ": " + ((this.nextHiveSpawnTick) / extra).ToStringTicksToPeriod();
                }
                return "RRY_HiveReproducesIn".Translate() + ": " + ((this.nextHiveSpawnTick)).ToStringTicksToPeriod();
			}
			return null;
		}

		// Token: 0x060029E2 RID: 10722 RVA: 0x0013D1E4 File Offset: 0x0013B5E4
		public void CalculateNextHiveLikeSpawnTick()
		{
			Room room = this.parent.GetRoom(RegionType.Set_Passable);
			int num = 0;
			int num2 = GenRadial.NumCellsInRadius(9f);
			for (int i = 0; i < num2; i++)
			{
				IntVec3 intVec = this.parent.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(this.parent.Map))
				{
					if (RegionAndRoomQuery.RoomAt(intVec, this.parent.Map, RegionType.Set_Passable) == room)
					{
						if (intVec.GetThingList(this.parent.Map).Any((Thing t) => t is HiveLike))
						{
							num++;
						}
					}
				}
			}
			float num3 = this.Props.ReproduceRateFactorFromNearbyHiveCountCurve.Evaluate((float)num);
			this.nextHiveSpawnTick = (int)(this.Props.HiveSpawnIntervalDays.RandomInRange * 60000f / (num3 * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

        // Token: 0x060029E3 RID: 10723 RVA: 0x0013D300 File Offset: 0x0013B700
        public bool TrySpawnChildHiveLike(float minDist, float maxDist, out HiveLike newHiveLike, bool ignoreRoofedRequirement = true, bool allowUnreachable = false, bool aggressive = false)
        {
            if (!this.CanSpawnChildHiveLike)
            {
                newHiveLike = null;
                return false;
            }
            IntVec3 loc = CompSpawnerHiveLikes.FindChildHiveLocation(this.parent.Position, this.parent.Map, ((HiveLike)this.parent).Def.HiveDefchild, this.Props, minDist, maxDist, ignoreRoofedRequirement, allowUnreachable);
            if (!loc.IsValid)
            {
                newHiveLike = null;
                return false;
            }
            newHiveLike = (HiveLike)ThingMaker.MakeThing(this.parent.def, null);
            if (newHiveLike.Faction != this.parent.Faction)
            {
                newHiveLike.SetFaction(this.parent.Faction, null);
            }
            if (this.parent is HiveLike hivelike)
            {
                newHiveLike.active = hivelike.active;
                newHiveLike.parentHiveLike = hivelike.parentHiveLike ?? hivelike;
                //childHiveLikes.Add(newHiveLike);
            }
            GenSpawn.Spawn(newHiveLike.Def.TunnelDefchild, loc, this.parent.Map, WipeMode.FullRefund);
            this.CalculateNextHiveLikeSpawnTick();
            return true;
        }
        // Token: 0x060029E3 RID: 10723 RVA: 0x0013D300 File Offset: 0x0013B700
        public bool TrySpawnChildHiveLike(float minDist, float maxDist, out TunnelHiveLikeSpawner newTunnelLike, bool ignoreRoofedRequirement = true, bool allowUnreachable = false, bool aggressive = false)
        {
            if (!this.CanSpawnChildHiveLike)
            {
                newTunnelLike = null;
                return false;
            }
            IntVec3 loc = FindChildHiveLocation(this.parent.Position, this.parent.Map, ((HiveLike)this.parent).Def.HiveDefchild, this.Props, minDist, maxDist, ignoreRoofedRequirement, allowUnreachable);
            if (!loc.IsValid)
            {
                newTunnelLike = null;
                return false;
            }
            newTunnelLike = (TunnelHiveLikeSpawner)ThingMaker.MakeThing(((HiveLike)this.parent).Def.TunnelDefchild, null);
            /*
            if (newTunnelLike.Faction != this.parent.Faction)
            {
                newTunnelLike.SetFaction(this.parent.Faction, null);
            }
            */
            if (this.parent is TunnelHiveLikeSpawner hivelike)
            {
                newTunnelLike.active = hivelike.active;
                newTunnelLike.parentHiveLike = this.hiveLike;
            //    childTunnelLikes.Add(newTunnelLike);
            }
            GenSpawn.Spawn(newTunnelLike.Def, loc, this.parent.Map, WipeMode.FullRefund);
            this.CalculateNextHiveLikeSpawnTick();
            return true;
        }

        // Token: 0x060029E4 RID: 10724 RVA: 0x0013D3DC File Offset: 0x0013B7DC
        public static IntVec3 FindChildHiveLocation(IntVec3 pos, Map map, ThingDef parentDef, CompProperties_SpawnerHiveLikes props, float minDist, float maxDist, bool ignoreRoofedRequirement = false, bool allowUnreachable = false, bool aggressive = false)
		{
			IntVec3 intVec = IntVec3.Invalid;
			for (int i = 0; i < 3; i++)
            {
                bool flag;
				if (i < 2)
				{
                    /*
					if (i == 1)
					{
						minDist = 0f;
					}
                    */
					flag = CellFinder.TryFindRandomReachableCellNear(pos, map, maxDist, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => CompSpawnerHiveLikes.CanSpawnHiveAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement), null, out intVec, 999999);
				}
				else
				{
					flag = (allowUnreachable && CellFinder.TryFindRandomCellNear(pos, map, (int)maxDist, (IntVec3 c) => CompSpawnerHiveLikes.CanSpawnHiveAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement), out intVec, -1));
				}
				if (flag)
				{
					intVec = CellFinder.FindNoWipeSpawnLocNear(intVec, map, parentDef, Rot4.North, 2, (IntVec3 c) => CompSpawnerHiveLikes.CanSpawnHiveAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement));
					break;
				}
			}
            if (Rand.Chance(0.05f) || intVec == IntVec3.Invalid)
            {
                if (InfestationCellFinder.TryFindCell(out IntVec3 c, map))
                {
                    return c;
                }
            }
			return intVec;
		}

		// Token: 0x060029E5 RID: 10725 RVA: 0x0013D4FC File Offset: 0x0013B8FC
		private static bool CanSpawnHiveAt(IntVec3 c, Map map, IntVec3 parentPos, ThingDef parentDef, float minDist, bool ignoreRoofedRequirement)
		{
			if ((!ignoreRoofedRequirement && !c.Roofed(map)) || (!c.Walkable(map) || (minDist != 0f && (float)c.DistanceToSquared(parentPos) < minDist * minDist)))
			{
				return false;
			}
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
				if (c2.InBounds(map))
				{
					List<Thing> thingList = c2.GetThingList(map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j] is HiveLike || thingList[j] is TunnelHiveLikeSpawner)
						{
							return false;
						}
					}
				}
			}
			List<Thing> thingList2 = c.GetThingList(map);
			for (int k = 0; k < thingList2.Count; k++)
			{
				Thing thing = thingList2[k];
				bool flag = thing.def.category == ThingCategory.Building && thing.def.passability == Traversability.Impassable;
				if (flag && GenSpawn.SpawningWipes(parentDef, thing.def))
				{
					return true;
				}
			}
			return true;
		}

		// Token: 0x060029E6 RID: 10726 RVA: 0x0013D65C File Offset: 0x0013BA5C
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
            {
                if (!this.canSpawnHiveLikes)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Wake Up",
                        action = delegate ()
                        {
                            this.canSpawnHiveLikes = true;
                        }
                    };
                }
                int num = 1;
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Reproduce",
                    icon = TexCommand.GatherSpotActive,
                    defaultDesc = string.Format("this hive has {0} child nodes MinTunnelSpawnPreferredDist: {1}, MaxTunnelSpawnRadius: {2}", ChildHives.Count, MinTunnelSpawnPreferredDist, MaxTunnelSpawnRadius),
                    action = delegate ()
                    {
                        this.TrySpawnChildHiveLike(MinTunnelSpawnPreferredDist, MaxTunnelSpawnRadius, out HiveLike Hivelike, canSpawnUnroofed, canSpawnUnreachable, aggressiveSpawn);
                    },
                    groupKey = num
				};
                if (DebugSettings.godMode)
                {
                    num++;
                    yield return new Command_Toggle
                    {
                        icon = canSpawnUnroofed ? TexCommand.ForbidOff : TexCommand.ForbidOn,
                        defaultLabel = "Unroofed Spawning",
                        isActive = (() => canSpawnUnroofed),
                        toggleAction = delegate ()
                        {
                            canSpawnUnroofed = !canSpawnUnroofed;
                        },
                        groupKey = num
                    };
                    num++;
                    yield return new Command_Toggle
                    {
                        icon = canSpawnUnreachable ? TexCommand.ForbidOff : TexCommand.ForbidOn,
                        defaultLabel = "Unreachable Spawning",
                        isActive = (() => canSpawnUnreachable),
                        toggleAction = delegate ()
                        {
                            canSpawnUnreachable = !canSpawnUnreachable;
                        },
                        groupKey = num
                    };
                    num++;
                    yield return new Command_Toggle
                    {
                        icon = aggressiveSpawn ? TexCommand.ForbidOff : TexCommand.ForbidOn,
                        defaultLabel = "Agressive Spawning",
                        isActive = (() => aggressiveSpawn),
                        toggleAction = delegate ()
                        {
                            aggressiveSpawn = !aggressiveSpawn;
                        },
                        groupKey = num
                    };
                }
            }
			yield break;
		}

        // Token: 0x060029E7 RID: 10727 RVA: 0x0013D67F File Offset: 0x0013BA7F
        public override void PostExposeData()
		{
			Scribe_Values.Look<int>(ref this.nextHiveSpawnTick, "nextHiveLikeSpawnTick", 0, false);
            Scribe_Values.Look<bool>(ref this.canSpawnHiveLikes, "canSpawnHiveLikes", true, false);
            Scribe_Values.Look<bool>(ref this.canSpawnUnroofed, "canSpawnUnroofed", false, false);
            Scribe_Values.Look<bool>(ref this.canSpawnUnreachable, "canSpawnUnreachable", false, false);
            Scribe_Values.Look<bool>(ref this.aggressiveSpawn, "aggressiveSpawn", false, false);
            Scribe_Collections.Look<HiveLike>(ref this.childHiveLikes, "childHiveLikes", LookMode.Reference, new object[0]);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.childHiveLikes.RemoveAll((HiveLike x) => x == null);
            }
            Scribe_Collections.Look<TunnelHiveLikeSpawner>(ref this.childTunnelLikes, "childTunnelLikes", LookMode.Reference, new object[0]);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.childHiveLikes.RemoveAll((HiveLike x) => x == null);
            }
        }

        public List<HiveLike> childHiveLikes = new List<HiveLike>();
        public List<TunnelHiveLikeSpawner> childTunnelLikes = new List<TunnelHiveLikeSpawner>();
        // Token: 0x0400173E RID: 5950
        private int nextHiveSpawnTick = -1;

        // Token: 0x0400173F RID: 5951
        public bool canSpawnHiveLikes = true;
        public bool canSpawnUnroofed = false;
        public bool canSpawnUnreachable = false;
        public bool aggressiveSpawn = false;

        // Token: 0x04001740 RID: 5952
        public const int MaxHivesPerMap = 30;
	}
}
