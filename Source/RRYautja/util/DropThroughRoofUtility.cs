using RRYautja;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
    // Token: 0x0200070C RID: 1804
    public static class DropThroughRoofUtility
    {
        // Token: 0x06002763 RID: 10083 RVA: 0x0012C48C File Offset: 0x0012A88C
        public static void DropThingsNear(IntVec3 dropCenter, Map map, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true)
        {
            DropThroughRoofUtility.tempList.Clear();
            foreach (Thing item in things)
            {
                List<Thing> list = new List<Thing>
                {
                    item
                };
                DropThroughRoofUtility.tempList.Add(list);
            }
            DropThroughRoofUtility.DropThingGroupsNear(dropCenter, map, DropThroughRoofUtility.tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch);
            DropThroughRoofUtility.tempList.Clear();
        }

        // Token: 0x06002764 RID: 10084 RVA: 0x0012C518 File Offset: 0x0012A918
        public static void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true)
        {
            foreach (List<Thing> list in thingsGroups)
            {
                IntVec3 intVec;
                if (!DropThoughRoofCellFinder.TryFindDropSpotNear(dropCenter, map, out intVec, true, canRoofPunch))
                {
                    Log.Warning(string.Concat(new object[]
                    {
                        "DropThingsNear failed to find a place to drop ",
                        list.FirstOrDefault<Thing>(),
                        " near ",
                        dropCenter,
                        ". Dropping on random square instead."
                    }), false);
                    intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map) && (c.Roofed(map) && c.GetRoof(map) != RoofDefOf.RoofRockThick), map, 1000);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SetForbidden(true, false);
                }
                if (instaDrop)
                {
                    foreach (Thing thing in list)
                    {
                        GenPlace.TryPlaceThing(thing, intVec, map, ThingPlaceMode.Near, null, null);
                    }
                }
                else
                {

                    ActiveTransporterInfo activeDropPodInfo = new ActiveTransporterInfo();
                    foreach (Thing item in list)
                    {
                        activeDropPodInfo.innerContainer.TryAddOrTransfer(item, true);
                    }
                    activeDropPodInfo.openDelay = openDelay;
                    activeDropPodInfo.leaveSlag = leaveSlag;

                    DropThroughRoofUtility.MakeTunnelAt(intVec, map, activeDropPodInfo);
                }
            }
        }

        // Token: 0x06002762 RID: 10082 RVA: 0x0012C458 File Offset: 0x0012A858
        public static void MakeTunnelAt(IntVec3 c, Map map, ActiveTransporterInfo info)
        {
            ThingDef TunnelDef = DefDatabase<ThingDef>.GetNamed("RRY_Tunneler");
        //    Log.Message(string.Format("making tunnelSpawner: {0}, @: {1}, {2}, {3}", TunnelDef, c, map, info.innerContainer.ContentsString));
            TunnelSpawner tunnelSpawner = (TunnelSpawner)ThingMaker.MakeThing(TunnelDef, null);
            foreach (Thing item in info.innerContainer)
            {
                tunnelSpawner.GetDirectlyHeldThings().TryAddOrTransfer(item, false);
            }
            GenSpawn.Spawn(tunnelSpawner, c, map);
            //--    SkyfallerMaker.SpawnSkyfaller(ThingDefOf.DropPodIncoming, tunnelSpawner, c, map);
        }

        // Token: 0x04001640 RID: 5696
        private static List<List<Thing>> tempList = new List<List<Thing>>();
    }
}
