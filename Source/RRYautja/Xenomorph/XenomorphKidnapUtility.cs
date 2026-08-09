using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RRYautja
{
    // Token: 0x020000D7 RID: 215
    public static class XenomorphKidnapUtility
    {
        public static bool eggsPresent;
        public static bool eggsReachable;
        public static Thing closestReachableEgg;
        public static Thing closestReachableCocoontoEgg;
        
        public static bool hivelikesPresent;
        public static bool hivelikesReachable;
        public static Thing closestReachableHivelike;

        public static bool hiveslimepresent;
        public static bool hiveslimeReachable;
        public static Thing closestreachablehiveslime;
        
        public static Thing eggThing;
        public static Thing hiveThing;

        // Token: 0x060004D2 RID: 1234 RVA: 0x00031074 File Offset: 0x0002F474
        public static bool TryFindGoodKidnapVictim(Pawn kidnapper, float maxDist, out Pawn victim, List<Thing> disallowed = null, bool roofed = false, bool allowCocooned = false, float minDistance = 0f, bool allowHost = false)
        {
            if (!kidnapper.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !kidnapper.Map.reachability.CanReachMapEdge(kidnapper.Position, TraverseParms.For(kidnapper, Danger.Some, TraverseMode.ByPawn, false)))
            {
                victim = null;
                return false;
            }
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn = t as Pawn;
                bool minFlag = minDistance==0 ? true : t.Position.DistanceTo(kidnapper.mindState.duty.focus.Cell) <= minDistance;
                bool roofedFlag = !t.Position.Roofed(pawn.Map);
                bool cocoonFlag = !pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Cocooned) || allowCocooned;

                bool xenoimpregnationFlag = pawn.health.hediffSet.hediffs.Any(x => x.def.defName.Contains("XenomorphImpregnation") && x.CurStageIndex < x.def.stages.Count - 2);
                bool neoimpregnationFlag = pawn.health.hediffSet.hediffs.Any(x=> x.def.defName.Contains("NeomorphImpregnation"));
                bool impregnationFlag = ((xenoimpregnationFlag && cocoonFlag) || !xenoimpregnationFlag) && !neoimpregnationFlag;
                bool pawnFlag = pawn.isPotentialHost(allowImpreg: allowHost) && pawn.Downed;
            //    Log.Message(string.Format("minFlag: {0}, roofedFlag: {1}, cocoonFlag: {2}, xenoimpregnationFlag: {3}, neoimpregnationFlag: {4}, impregnationFlag: {5}, pawnFlag: {6}, CanReserve: {7}, Allowed: {8}", minFlag, roofedFlag, cocoonFlag, xenoimpregnationFlag, neoimpregnationFlag, impregnationFlag, pawnFlag, kidnapper.CanReserve(pawn, 1, -1, null, false), (disallowed == null || !disallowed.Contains(pawn))));
                return  cocoonFlag && pawnFlag && impregnationFlag && minFlag && kidnapper.CanReserve(pawn, 1, -1, null, false) && (disallowed == null || !disallowed.Contains(pawn));
            };
            victim = (Pawn)GenClosest.ClosestThingReachable(kidnapper.Position, kidnapper.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some, false), maxDist, validator, null, 0, -1, false, RegionType.Set_Passable, false);
            return victim != null;
        }

        public static bool HiveMainPresent(Map map)
        {
            if (!map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive).NullOrEmpty())
            {
                return true;
            }

            return false;
        }

        public static bool HiveChildPresent(Map map)
        {
            if (!map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive_Child).NullOrEmpty())
            {
                return true;
            }
            
            return false;
        }

        public static bool TryFindGoodHiveLoc(Pawn pawn, out IntVec3 c, Pawn victim = null, bool allowFogged = false, bool allowUnroofed = false, bool allowDigging = false)
        {
            Map map = pawn.Map;
            c = IntVec3.Invalid;
            if (map == null)
            {
                return false;
            }
            if (!pawn.isXenomorph(out Comp_Xenomorph _Xenomorph))
            {
                return false;
            }
            else
            {
                c = _Xenomorph.HiveLoc;
            }
            MapComponent_HiveGrid hiveGrid = pawn.Map.GetComponent<MapComponent_HiveGrid>();
            bool selected = map != null ? Find.Selector.SelectedObjects.Contains(pawn) && (Prefs.DevMode) : false;

            Predicate<IntVec3> validatora = delegate (IntVec3 y)
            {
                if (y.GetTerrain(map).HasTag("Water"))
                {
                    return false;
                }
                bool roofed = (!allowUnroofed && y.Roofed(map)) || allowUnroofed;
                bool score = InfestationLikeCellFinder.GetScoreAt(y, map, allowFogged, allowUnroofed, allowDigging) > 0f;;
                bool filled = y.Filled(map) && !allowDigging;
                bool edifice = y.GetEdifice(map).DestroyedOrNull() || allowDigging;
                bool building = y.GetFirstBuilding(map).DestroyedOrNull() || allowDigging;
                bool thing = y.GetThingList(map).All(x => x.GetType() != typeof(Building_XenomorphCocoon) && x.GetType() != typeof(Building_XenoEgg) && x.GetType() != typeof(HiveLike));
                bool r = score && !filled && edifice && building && thing && roofed;
                return r;
            };
            /*
            if (validatora (_Xenomorph.HiveLoc))
            {

                c = _Xenomorph.HiveLoc;
            }
            else
            if (hiveGrid.Hivelist.NullOrEmpty())
            {
                Log.Warning("no hives present");
                if (!hiveGrid.HiveLoclist.NullOrEmpty())
                {
                //    Log.Message("hivelocs present");
                    c = hiveGrid.HiveLoclist.RandomElement();
                //    return true;
                }
                else
                {
                    Log.Warning("no hivelocs present");
                }
            }
            else
            {
            //    Log.Message("hives present");
                c = hiveGrid.Hivelist.RandomElement().Position;
            //    return true;
            }
            */
            if (c == IntVec3.Invalid || c.x < 1 || c.z < 1 || c == IntVec3.Zero || c.InNoBuildEdgeArea(map) || c.InNoZoneEdgeArea(map) || c.GetTerrain(map).HasTag("Water"))
            {
                if (!InfestationLikeCellFinder.TryFindCell(out c, out IntVec3 lc, map, allowFogged, allowUnroofed, allowDigging))
                {
                //    Log.Message(string.Format("Cant find suitable hive location, defaulting to map edge"));
                    if (!InfestationCellFinder.TryFindCell(out c, map))
                    {
                    //    Log.Message(string.Format("Cant find suitable hive location, defaulting to map edge"));
                        if (!RCellFinder.TryFindBestExitSpot(pawn, out c, TraverseMode.ByPawn))
                        {
                        //    Log.Message(string.Format("Cant find spot near map edge"));
                        }
                        else
                        {
                        //    Log.Message(string.Format("RCellFinder: {0}", c));
                        }
                    }
                    else
                    {
                    //    Log.Message(string.Format("InfestationCellFinder: {0}", c));
                    }
                }
                else
                {
                //    Log.Message(string.Format("InfestationLikeCellFinder: {0}", c));
                }
            }
            if (c != IntVec3.Invalid && c.x > 1 && c.z > 1 && !c.InNoBuildEdgeArea(map) && !c.InNoZoneEdgeArea(map) && !c.GetTerrain(map).HasTag("Water"))
            {
                if (pawn.GetLord() != null && pawn.GetLord() is Lord lord)
                {
                    //    Log.Message(string.Format("TryFindGoodHiveLoc pawn.GetLord() != null"));
                }
                else
                {
                    //    Log.Message(string.Format("TryFindGoodHiveLoc pawn.GetLord() == null"));
                }
                if (pawn.mindState.duty.def != XenomorphDefOf.RRY_Xenomorph_DefendAndExpandHive && pawn.mindState.duty.def != XenomorphDefOf.RRY_Xenomorph_DefendHiveAggressively)
                {
                    //    Log.Message(string.Format("TryFindGoodHiveLoc UpdateDuty"));
                    pawn.mindState.duty = new PawnDuty(XenomorphDefOf.RRY_Xenomorph_DefendAndExpandHive, c, 40f);
                }
                if (hiveGrid.HiveLoclist.NullOrEmpty() || !hiveGrid.HiveLoclist.Contains(c))
                {
                    //    Log.Message(string.Format("TryFindGoodHiveLoc Adding to HiveLoclist"));
                    hiveGrid.HiveLoclist.Add(c);
                }
                if (!victim.DestroyedOrNull())
                {
                    Func<Pawn, IntVec3, IntVec3, bool> validator = delegate (Pawn p, IntVec3 z, IntVec3 y)
                    {
                        if (y.GetTerrain(map).HasTag("Water"))
                        {
                            return false;
                        }
                        bool roofed = (!allowUnroofed && y.Roofed(map)) || allowUnroofed;
                        bool thing = y.GetThingList(map).Any(x => x.GetType() != typeof(Building_XenomorphCocoon) && x.GetType() != typeof(Building_XenoEgg) && x.GetType() != typeof(HiveLike) && x.GetType() != typeof(Building));
                        bool r = thing && roofed;
                        //   Log.Message(string.Format("Cell: {0}, score: {1}, XenohiveA: {2}, XenohiveB: {3}, !filled: {4}, edifice: {5}, building: {6}, thingA: {7}, thingB: {8}, roofed: {9}\nResult: {10}", y, GetScoreAt(y, map, allowFogged), XenohiveA , XenohiveB , !filled , edifice , building , thingA , thingB, roofed, result));
                        return r;
                    };
                    c = RCellFinder.RandomWanderDestFor(pawn, c, 5f, validator, Danger.Some);
                }
                return true;
            }
            else return false;
            //return c != IntVec3.Invalid && c != IntVec3.Zero && !c.InNoBuildEdgeArea(map) && !c.InNoZoneEdgeArea(map) && !c.GetTerrain(map).HasTag("Water");
        }


        // Token: 0x060004D2 RID: 1234 RVA: 0x00031074 File Offset: 0x0002F474
        public static bool TryFindGoodImpregnateVictim(Pawn kidnapper, float maxDist, out Pawn victim, List<Thing> disallowed = null)
        {
            if (!kidnapper.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !kidnapper.Map.reachability.CanReachMapEdge(kidnapper.Position, TraverseParms.For(kidnapper, Danger.Some, TraverseMode.ByPawn, false)))
            {
            //    Log.Message(string.Format("TryFindGoodImpregnateVictim \n{0} incapable of job", kidnapper.LabelShortCap));
                victim = null;
                return false;
            }
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn = t as Pawn;
                bool cocoonFlag = !pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Cocooned);
                bool pawnFlag = ((pawn.isPotentialHost())) && !XenomorphUtil.IsXenomorph(pawn) && pawn.gender == Gender.Female && pawn.Downed && (pawn.Faction == null || pawn.Faction.HostileTo(kidnapper.Faction) || kidnapper.Faction == null);
            //    Log.Message(string.Format(" cocoonFlag; {0} \n pawnFlag: {1}", cocoonFlag, pawnFlag));
                return (cocoonFlag && pawnFlag) && (kidnapper.CanReserve(pawn, 1, -1, null, false) && (disallowed == null || !disallowed.Contains(pawn))) && pawn != kidnapper && pawn.gender == Gender.Female;
            };
        //    Log.Message(string.Format("TryFindGoodImpregnateVictim \nvalidator {0}", validator));
            victim = (Pawn)GenClosest.ClosestThingReachable(kidnapper.Position, kidnapper.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some, false), maxDist, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        //    Log.Message(string.Format("TryFindGoodImpregnateVictim \nvictim {0}", victim));
            return victim != null;
        }

        // Token: 0x060004D3 RID: 1235 RVA: 0x0003113C File Offset: 0x0002F53C
        public static Pawn ReachableWoundedGuest(Pawn searcher)
        {
            List<Pawn> list = searcher.Map.mapPawns.SpawnedPawnsInFaction(searcher.Faction);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i];
                if (pawn.guest != null && !pawn.IsPrisoner && pawn.Downed && searcher.CanReserveAndReach(pawn, PathEndMode.OnCell, Danger.Some, 1, -1, null, false))
                {
                    return pawn;
                }
            }
            return null;
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
        
        public static List<IntVec3> XenoCocoonLocations(IntVec3 center, float radius, Map map)
        {
            int num = GenRadial.NumCellsInRadius(radius);
            List<IntVec3> list = new List<IntVec3>();
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = center + GenRadial.RadialPattern[i];
                CellRect rect = new CellRect(intVec.x, intVec.z, 1, 1);
                if (intVec.InBounds(map))
                {
                    if (IsMapRectClear(rect, map))
                    {
                        list.Add(intVec);
                    }
                }
            }
            return list;
        }

    }
}
