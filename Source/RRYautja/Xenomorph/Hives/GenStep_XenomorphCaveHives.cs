using RRYautja.settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
    // Token: 0x020003F8 RID: 1016
    public class GenStep_XenomorphCaveHives : GenStep
    {
        // Token: 0x1700025E RID: 606
        // (get) Token: 0x06001188 RID: 4488 RVA: 0x00084CE7 File Offset: 0x000830E7
        public override int SeedPart
        {
            get
            {
                return 349641510;
            }
        }

        // Token: 0x06001189 RID: 4489 RVA: 0x00084CF0 File Offset: 0x000830F0
        public override void Generate(Map map, GenStepParams parms)
        {
            if (!Find.Storyteller.difficulty.allowCaveHives || !SettingsHelper.latest.AllowXenomorphFaction )
            {
                return;
            }
            MapGenFloatGrid caves = MapGenerator.Caves;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            float num = 0.7f;
            int num2 = 0;
            this.rockCells.Clear();
            foreach (IntVec3 intVec in map.AllCells)
            {
                if (elevation[intVec] > num)
                {
                    this.rockCells.Add(intVec);
                }
                if (caves[intVec] > 0f)
                {
                    num2++;
                }
            }
            List<IntVec3> list = (from c in map.AllCells
                                  where map.thingGrid.ThingsAt(c).Any((Thing thing) => thing.Faction != null)
                                  select c).ToList<IntVec3>();
            GenMorphology.Dilate(list, 50, map, null);
            HashSet<IntVec3> hashSet = new HashSet<IntVec3>(list);
            int num3 = GenMath.RoundRandom((float)num2 / 1000f);
            GenMorphology.Erode(this.rockCells, 10, map, null);
            this.possibleSpawnCells.Clear();
            for (int i = 0; i < this.rockCells.Count; i++)
            {
                if (caves[this.rockCells[i]] > 0f && !hashSet.Contains(this.rockCells[i]))
                {
                    this.possibleSpawnCells.Add(this.rockCells[i]);
                }
            }
            this.spawnedHives.Clear();
            this.TrySpawnHive(map);
            this.spawnedHives.Clear();
        }

        // Token: 0x0600118A RID: 4490 RVA: 0x00084ED4 File Offset: 0x000832D4
        private void TrySpawnHive(Map map)
        {
            IntVec3 intVec;
            if (!this.TryFindHiveSpawnCell(map, out intVec))
            {
                return;
            }
            this.possibleSpawnCells.Remove(intVec);
            HiveLike hive = (HiveLike)GenSpawn.Spawn(ThingMaker.MakeThing(XenomorphDefOf.RRY_Xenomorph_Hive, null), intVec, map, WipeMode.Vanish);
            Faction xenoFaction = Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph);
            if (xenoFaction == null)
            {
                // Try to find by defName as fallback
                foreach (var f in Find.FactionManager.AllFactions)
                {
                    if (f.def.defName == "RRY_Xenomorph")
                    {
                        xenoFaction = f;
                        break;
                    }
                }
            }
            if (xenoFaction != null)
            {
                hive.SetFaction(xenoFaction, null);
            }
            hive.caveColony = true;
            /*
            (from x in hive.GetComps<CompSpawner>()
             where x.PropsSpawner.thingToSpawn == ThingDefOf.GlowPod
             select x).First<CompSpawner>().TryDoSpawn();
            */
            hive.SpawnPawnsUntilPoints(Rand.Range(200f, 500f));
            hive.canSpawnPawns = false;
            hive.GetComp<CompSpawnerHiveLikes>().canSpawnHiveLikes = false;
            this.spawnedHives.Add(hive);
        }

        // Token: 0x0600118B RID: 4491 RVA: 0x00084F90 File Offset: 0x00083390
        private bool TryFindHiveSpawnCell(Map map, out IntVec3 spawnCell)
        {
            float num = -1f;
            IntVec3 intVec = IntVec3.Invalid;
            for (int i = 0; i < 3; i++)
            {
                IntVec3 intVec2;
                if (!(from x in this.possibleSpawnCells
                      where x.Standable(map) && x.GetFirstItem(map) == null && x.GetFirstBuilding(map) == null && x.GetFirstPawn(map) == null
                      select x).TryRandomElement(out intVec2))
                {
                    break;
                }
                float num2 = -1f;
                for (int j = 0; j < this.spawnedHives.Count; j++)
                {
                    float num3 = (float)intVec2.DistanceToSquared(this.spawnedHives[j].Position);
                    if (num2 < 0f || num3 < num2)
                    {
                        num2 = num3;
                    }
                }
                if (!intVec.IsValid || num2 > num)
                {
                    intVec = intVec2;
                    num = num2;
                }
            }
            spawnCell = intVec;
            return spawnCell.IsValid;
        }

        // Token: 0x04000AB8 RID: 2744
        private List<IntVec3> rockCells = new List<IntVec3>();

        // Token: 0x04000AB9 RID: 2745
        private List<IntVec3> possibleSpawnCells = new List<IntVec3>();

        // Token: 0x04000ABA RID: 2746
        private List<HiveLike> spawnedHives = new List<HiveLike>();

        // Token: 0x04000ABB RID: 2747
        private const int MinDistToOpenSpace = 10;

        // Token: 0x04000ABC RID: 2748
        private const int MinDistFromFactionBase = 50;

        // Token: 0x04000ABD RID: 2749
        private const float CaveCellsPerHive = 1000f;
    }
}
