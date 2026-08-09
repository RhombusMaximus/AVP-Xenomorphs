using RRYautja;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
    // Token: 0x020006C7 RID: 1735
    public class Building_CrashedShipPart_CryptosleepCasket_Xenomorph : Building // _CrashedShipPart
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }

        private List<Rot4> Rotlist = new List<Rot4>
        {
            Rot4.North,
            Rot4.South,
            Rot4.East,
            Rot4.West
        };

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = this.Map;
            IntVec3 position = this.Position;
            ThingDef named = XenomorphDefOf.RRY_XenomorphCryptosleepCasket;
            int num = (named.Size.x > named.Size.z) ? named.Size.x : named.Size.z;
        //    int num2 = 0;
            base.Destroy(mode);
            int count = Rand.RangeInclusive(0, 3);
            if (count>0)
            {
                for (int i = 0; i < count; i++)
                {
                    Rot4 rot = Rotlist.RandomElement();
                    CellRect mapRect;
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(position, map, 5, null);
                    mapRect = new CellRect(intVec.x, intVec.z, num, num);
                    GenSpawn.Spawn(TryMakeCasket(mapRect, map, named), intVec, map, rot, WipeMode.Vanish, false);
                //    GenPlace.TryPlaceThing(TryMakeCasket(mapRect, map, named), intVec, map, ThingPlaceMode.Near);
                }
            }

        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
        }

        // Token: 0x0600000B RID: 11 RVA: 0x00002C9C File Offset: 0x00000E9C
        private static bool IsMapRectClear(CellRect mapRect, Map map)
        {
            foreach (IntVec3 intVec in mapRect)
            {
                bool flag = !map.pathing.Normal.pathGrid.WalkableFast(intVec);
                if (flag)
                {
                    return false;
                }
                List<Thing> thingList = GridsUtility.GetThingList(intVec, map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    bool flag2 = thingList[i].def.category == (ThingCategory)3 || thingList[i].def.category == (ThingCategory)1 || thingList[i].def.category == (ThingCategory)10;
                    if (flag2)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002D80 File Offset: 0x00000F80
        private static void ClearMapRect(CellRect mapRect, Map map)
        {
            foreach (IntVec3 intVec in mapRect)
            {
                List<Thing> thingList = GridsUtility.GetThingList(intVec, map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    thingList[i].Destroy(0);
                }
            }
        }
        // Token: 0x0600000D RID: 13 RVA: 0x00002DF8 File Offset: 0x00000FF8
        private static Building_XenomorphCryptosleepCasket TryMakeCasket(CellRect mapRect, Map map, ThingDef thingDef)
        {
            mapRect.ClipInsideMap(map);
            CellRect cellRect;
            cellRect = new CellRect(mapRect.BottomLeft.x + 1, mapRect.BottomLeft.z + 1, 2, 1);
            cellRect.ClipInsideMap(map);
            foreach (IntVec3 intVec in cellRect)
            {
                List<Thing> thingList = GridsUtility.GetThingList(intVec, map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    bool flag = !thingList[i].def.destroyable;
                    if (flag)
                    {
                        return null;
                    }
                }
            }
            Building_XenomorphCryptosleepCasket building_CryptosleepCasket = (Building_XenomorphCryptosleepCasket)ThingMaker.MakeThing(thingDef, null);
            building_CryptosleepCasket.SetPositionDirect(cellRect.BottomLeft);
            bool flag2 = Rand.Value < 0.5f;
            if (flag2)
            {
                building_CryptosleepCasket.Rotation = Rot4.East;
            }
            else
            {
                building_CryptosleepCasket.Rotation = Rot4.North;
            }
            return building_CryptosleepCasket;
        }

    }
}
