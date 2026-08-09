using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x0200092C RID: 2348
    public static class DropThoughRoofCellFinder
    {
        // Token: 0x06003682 RID: 13954 RVA: 0x001A05D0 File Offset: 0x0019E9D0
        public static IntVec3 RandomDropSpot(Map map)
        {
            return CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && c.Roofed(map) && !c.Fogged(map), map, 1000);
        }
        
        // Token: 0x06003684 RID: 13956 RVA: 0x001A082C File Offset: 0x0019EC2C
        public static bool TryFindDropSpotNear(IntVec3 center, Map map, out IntVec3 result, bool allowFogged, bool canRoofPunch)
        {
            if (DebugViewSettings.drawDestSearch)
            {
                map.debugDrawer.FlashCell(center, 1f, "center", 50);
            }
            Predicate<IntVec3> validator = (IntVec3 c) => DropThoughRoofCellFinder.IsGoodDropSpot(c, map, allowFogged, canRoofPunch) && map.reachability.CanReach(center, c, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly) && GenRadial.RadialCellsAround(c, 1, true).Any(x => x.Roofed(map));
            int num = 5;
            while (!CellFinder.TryFindRandomCellNear(center, map, num, validator, out result, -1))
            {
                num += 3;
                if (num > 16)
                {
                    result = center;
                    return false;
                }
            }
            return true;
        }

        // Token: 0x06003685 RID: 13957 RVA: 0x001A08D4 File Offset: 0x0019ECD4
        public static bool IsGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch)
        {
            if (!c.InBounds(map) || !c.Standable(map))
            {
                return false;
            }
            if (!DropThoughRoofCellFinder.CanPhysicallyDropInto(c, map, canRoofPunch))
            {
                if (DebugViewSettings.drawDestSearch)
                {
                    map.debugDrawer.FlashCell(c, 0f, "phys", 50);
                }
                return false;
            }
            if (Current.ProgramState == ProgramState.Playing && !allowFogged && c.Fogged(map))
            {
                return false;
            }
            List<Thing> thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing is IActiveTransporter || thing is Skyfaller)
                {
                    return false;
                }
                if (thing.def.category != ThingCategory.Plant && GenSpawn.SpawningWipes(ThingDefOf.ActiveDropPod, thing.def))
                {
                    return false;
                }
            }
            return true;
        }


        // Token: 0x06003687 RID: 13959 RVA: 0x001A0A20 File Offset: 0x0019EE20
        public static IntVec3 FindRaidDropCenterDistant(Map map)
        {
            Faction hostFaction = map.ParentFaction ?? Faction.OfPlayer;
            IEnumerable<Thing> enumerable = map.mapPawns.FreeHumanlikesSpawnedOfFaction(hostFaction).Cast<Thing>();
            if (hostFaction == Faction.OfPlayer)
            {
                enumerable = enumerable.Concat(map.listerBuildings.allBuildingsColonist.Cast<Thing>());
            }
            else
            {
                enumerable = enumerable.Concat(from x in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                                               where x.Faction == hostFaction
                                               select x);
            }
            int num = 0;
            float num2 = 65f;
            IntVec3 intVec;
            for (; ; )
            {
                intVec = CellFinder.RandomCell(map);
                num++;
                if (DropThoughRoofCellFinder.CanPhysicallyDropInto(intVec, map, true) && !intVec.Fogged(map))
                {
                    if (num > 300)
                    {
                        break;
                    }
                    if (!intVec.Roofed(map))
                    {
                        num2 -= 0.2f;
                        bool flag = false;
                        foreach (Thing thing in enumerable)
                        {
                            if ((float)(intVec - thing.Position).LengthHorizontalSquared < num2 * num2)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag && map.reachability.CanReachFactionBase(intVec, hostFaction))
                        {
                            return intVec;
                        }
                    }
                }
            }
            return intVec;
        }

        // Token: 0x06003688 RID: 13960 RVA: 0x001A0BAC File Offset: 0x0019EFAC
        public static bool TryFindRaidDropCenterClose(out IntVec3 spot, Map map)
        {
            Faction parentFaction = map.ParentFaction;
            if (parentFaction == null)
            {
                return RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => DropThoughRoofCellFinder.CanPhysicallyDropInto(x, map, true) && !x.Fogged(map) && x.Standable(map), map, out spot);
            }
            int num = 0;
            for (; ; )
            {
                IntVec3 root = IntVec3.Invalid;
                if (map.mapPawns.FreeHumanlikesSpawnedOfFaction(parentFaction).Count<Pawn>() > 0)
                {
                    root = map.mapPawns.FreeHumanlikesSpawnedOfFaction(parentFaction).RandomElement<Pawn>().Position;
                }
                else
                {
                    if (parentFaction == Faction.OfPlayer)
                    {
                        List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
                        for (int i = 0; i < allBuildingsColonist.Count; i++)
                        {
                            if (DropThoughRoofCellFinder.TryFindDropSpotNear(allBuildingsColonist[i].Position, map, out root, true, true))
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (list[j].Faction == parentFaction && DropThoughRoofCellFinder.TryFindDropSpotNear(list[j].Position, map, out root, true, true))
                            {
                                break;
                            }
                        }
                    }
                    if (!root.IsValid)
                    {
                        RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => DropThoughRoofCellFinder.CanPhysicallyDropInto(x, map, true) && !x.Fogged(map) && x.Standable(map), map, out root);
                    }
                }
                spot = CellFinder.RandomClosewalkCellNear(root, map, 10, null);
                if (DropThoughRoofCellFinder.CanPhysicallyDropInto(spot, map, true) && !spot.Fogged(map))
                {
                    break;
                }
                num++;
                if (num > 300)
                {
                    goto Block_10;
                }
            }
            return true;
            Block_10:
            spot = CellFinderLoose.RandomCellWith((IntVec3 c) => DropThoughRoofCellFinder.CanPhysicallyDropInto(c, map, true), map, 1000);
            return false;
        }

        // Token: 0x06003689 RID: 13961 RVA: 0x001A0DA8 File Offset: 0x0019F1A8
        private static bool CanPhysicallyDropInto(IntVec3 c, Map map, bool canRoofPunch)
        {
            if (!c.Walkable(map))
            {
                return false;
            }
            RoofDef roof = c.GetRoof(map);
            if (roof != null)
            {
                if (!canRoofPunch)
                {
                    return false;
                }
                /*
                if (roof.isThickRoof)
                {
                    return false;
                }
                */
            }
            return true;
        }
    }
}
