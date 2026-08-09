using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace RRYautja
{
	// Token: 0x02000332 RID: 818
	public class IncidentWorker_NeomorphFungusSprout : IncidentWorker
	{
		// Token: 0x06000E22 RID: 3618 RVA: 0x00069D20 File Offset: 0x00068120
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms) || !settings.SettingsHelper.latest.AllowNeomorphs)
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 intVec;
			return map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow && this.TryFindRootCell(map, out intVec);
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x00069D68 File Offset: 0x00068168
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
            ThingDef thingDef = null;
            if (this.def == XenomorphDefOf.RRY_Neomorph_FungusSprout)
            {
                thingDef = XenomorphDefOf.RRY_Plant_Neomorph_Fungus;
            }
            if (this.def == XenomorphDefOf.RRY_Neomorph_FungusSprout_Hidden)
            {
                thingDef = XenomorphDefOf.RRY_Plant_Neomorph_Fungus_Hidden;
            }
			Map map = (Map)parms.target;
			IntVec3 root;
			if (!this.TryFindRootCell(map, out root))
			{
				return false;
			}
			Thing thing = null;
			int randomInRange = IncidentWorker_NeomorphFungusSprout.CountRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				IntVec3 intVec;
				if (!CellFinder.TryRandomClosewalkCellNear(root, map, 6, out intVec, (IntVec3 x) => this.CanSpawnAt(x, map)))
				{
					break;
				}
				Plant plant = intVec.GetPlant(map);
				if (plant != null)
				{
					plant.Destroy(DestroyMode.Vanish);
				}
				Thing thing2 = GenSpawn.Spawn(thingDef, intVec, map, WipeMode.Vanish);
				if (thing == null)
				{
					thing = thing2;
				}
			}
			if (thing == null)
			{
				return false;
            }
            if (this.def == XenomorphDefOf.RRY_Neomorph_FungusSprout)
            {
                base.SendStandardLetter(parms, thing, Array.Empty<NamedArgument>());
            }
			return true;
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x00069E4C File Offset: 0x0006824C
		private bool TryFindRootCell(Map map, out IntVec3 cell)
		{
			return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => this.CanSpawnAt(x, map) && RegionAndRoomQuery.RoomAt(x, map, RegionType.Set_Passable).CellCount >= 64, map, out cell);
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x00069E88 File Offset: 0x00068288
		private bool CanSpawnAt(IntVec3 c, Map map)
		{
			if (!c.Standable(map) || c.Fogged(map) || map.fertilityGrid.FertilityAt(c) < XenomorphDefOf.RRY_Plant_Neomorph_Fungus.plant.fertilityMin || !RegionAndRoomQuery.RoomAt(c, map, RegionType.Set_Passable).PsychologicallyOutdoors || c.GetEdifice(map) != null || !PlantUtility.GrowthSeasonNow(c, map, false))
			{
				return false;
			}
			Plant plant = c.GetPlant(map);
			if (plant != null && plant.def.plant.growDays > 10f)
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def == XenomorphDefOf.RRY_Plant_Neomorph_Fungus || thingList[i].def == XenomorphDefOf.RRY_Plant_Neomorph_Fungus_Hidden)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x04000922 RID: 2338
		private static readonly IntRange CountRange = new IntRange(10, 20);

		// Token: 0x04000923 RID: 2339
		private const int MinRoomCells = 64;

		// Token: 0x04000924 RID: 2340
		private const int SpawnRadius = 6;
	}
}
