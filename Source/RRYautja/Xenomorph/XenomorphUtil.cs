using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RRYautja
{
    class XenomorphUtil
    {
        public static bool HiveSlimePresent(Map map)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive_Slime).Count > 0;
        }

        public static Thing ClosestReachableHiveSlime(Pawn pawn)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Hive_Slime), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, null, null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }

        public static bool HiveShipPresent(Map map)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_XenomorphCrashedShipPart).Count > 0;
        }

        public static Thing ClosestReachableHiveShip(Pawn pawn)
        {
            return pawn.Map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_XenomorphCrashedShipPart).RandomElement();
        }

        //    public static IntVec3 HiveLocation(Map map)

        // Token: 0x060005D7 RID: 1495 RVA: 0x00039248 File Offset: 0x00037648
        public static bool CanHaulAside(Pawn p, Thing t, IntVec3 center, int radius, out IntVec3 storeCell)
        {
            storeCell = IntVec3.Invalid;
            return t.def.EverHaulable && !t.IsBurning() && p.CanReserveAndReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger(), 1, -1, null, false) && XenomorphUtil.TryFindSpotToPlaceHaulableCloseTo(t, p, t.PositionHeld, center, radius, out storeCell);
        }

        // Token: 0x060005D8 RID: 1496 RVA: 0x000392B4 File Offset: 0x000376B4
        public static Job HaulAsideJobFor(Pawn p, Thing t, IntVec3 center, int radius)
        {
            IntVec3 c;
            if (!XenomorphUtil.CanHaulAside(p, t, center, radius, out c))
            {
                return null;
            }
            return new Job(JobDefOf.HaulToCell, t, c)
            {
                count = 99999,
                haulOpportunisticDuplicates = false,
                haulMode = HaulMode.ToCellNonStorage,
                ignoreDesignations = true
            };
        }

        // Token: 0x060005D9 RID: 1497 RVA: 0x0003930C File Offset: 0x0003770C
        private static bool TryFindSpotToPlaceHaulableCloseTo(Thing haulable, Pawn worker, IntVec3 center, IntVec3 center2, int radius, out IntVec3 spot)
        {
            Region region = center.GetRegion(worker.Map, RegionType.Set_Passable);
            if (region == null)
            {
                spot = center;
                return false;
            }
            TraverseParms traverseParms = TraverseParms.For(worker, Danger.Deadly, TraverseMode.ByPawn, false);
            IntVec3 foundCell = IntVec3.Invalid;
            RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, false), delegate (Region r)
            {
                XenomorphUtil.candidates.Clear();
                XenomorphUtil.candidates.AddRange(r.Cells);
                XenomorphUtil.candidates.Sort((IntVec3 a, IntVec3 b) => a.DistanceToSquared(center).CompareTo(b.DistanceToSquared(center)));
                for (int i = 0; i < XenomorphUtil.candidates.Count; i++)
                {
                    IntVec3 intVec = XenomorphUtil.candidates[i];
                    if (XenomorphUtil.HaulablePlaceValidator(haulable, worker, intVec, center2, radius))
                    {
                        foundCell = intVec;
                        return true;
                    }
                }
                return false;
            }, 100, RegionType.Set_Passable);
            if (foundCell.IsValid)
            {
                spot = foundCell;
                return true;
            }
            spot = center;
            return false;
        }

        // Token: 0x060005DA RID: 1498 RVA: 0x000393CC File Offset: 0x000377CC
        private static bool HaulablePlaceValidator(Thing haulable, Pawn worker, IntVec3 c, IntVec3 center, int radius)
        {
            if (!worker.CanReserveAndReach(c, PathEndMode.OnCell, worker.NormalMaxDanger(), 1, -1, null, false))
            {
                return false;
            }
            if (GenPlace.HaulPlaceBlockerIn(haulable, c, worker.Map, true) != null)
            {
                return false;
            }
            if (!c.Standable(worker.Map))
            {
                return false;
            }
            if (c == haulable.Position && haulable.Spawned)
            {
                return false;
            }
            if (c.ContainsStaticFire(worker.Map))
            {
                return false;
            }
            if (XenomorphUtil.DistanceBetween(c, center) <= radius)
            {
                return false;
            }
            /*
            if (XenomorphUtil.DistanceBetween(c, center) > radius*2)
            {
                return false;
            }
            */
            /*
            if (c.AdjacentTo8Way(center))
            {
                return false;
            }
            */
            if (haulable != null && haulable.def.BlockPlanting)
            {
                Zone zone = worker.Map.zoneManager.ZoneAt(c);
                if (zone is Zone_Growing)
                {
                    return false;
                }
            }
            if (haulable.def.passability != Traversability.Standable)
            {
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 c2 = c + GenAdj.AdjacentCells[i];
                    if (worker.Map.designationManager.DesignationAt(c2, DesignationDefOf.Mine) != null)
                    {
                        return false;
                    }
                }
            }
            Building edifice = c.GetEdifice(worker.Map);
            if (edifice != null)
            {
                Building_Trap building_Trap = edifice as Building_Trap;
                if (building_Trap != null)
                {
                    return false;
                }
            }
            return true;
        }
        // Token: 0x0400030C RID: 780
        private static List<IntVec3> candidates = new List<IntVec3>();
        // Token: 0x060000A8 RID: 168 RVA: 0x00007234 File Offset: 0x00005434
        /*

        public static bool isInfectableThing(ThingDef thingDef)
        {
            return isInfectableThing(thingDef, out string FailReason);
        }

        public static bool isInfectableThing(ThingDef thingDef, out string FailReason)
        {
            FailReason = string.Empty;
            string NonBio = "Inorganic";
            if (thingDef.race == null)
            {
                FailReason = string.Format("{0} has No Race Properties", thingDef);
                return false; 
            }
            if (!settings.SettingsHelper.latest.AllowNonHumanlikeHosts && !thingDef.race.Humanlike)
            {
                FailReason = string.Format("Non-Humanlike");
                return false;
            }
            if (thingDef.isXenomorph() || thingDef.isNeomorph())
            {
                string str = thingDef.isXenomorph() ? "Xenomorph" : "Neomorph";
                FailReason = string.Format("{0}", str);
                return false;
            }
            bool pawnflag = !((UtilChjAndroids.ChjAndroid && UtilChjAndroids.isChjAndroid(thingDef)) || (UtilTieredAndroids.TieredAndroid && UtilTieredAndroids.isAtlasAndroid(thingDef)) || (UtilSynths.isAvPSynth(thingDef)));
            if (!pawnflag)
            {
                string str = string.Empty;
                if (UtilChjAndroids.ChjAndroid && UtilChjAndroids.isChjAndroid(thingDef))
                {
                    str = NonBio;
                }
                if (UtilTieredAndroids.TieredAndroid && UtilTieredAndroids.isAtlasAndroid(thingDef))
                {
                    str = NonBio;
                }

                if (UtilSynths.isAvPSynth(thingDef))
                {
                    str = NonBio;
                }
                FailReason = string.Format("{0}", str);
                return false; 
            }
            if (thingDef.race.IsMechanoid) { FailReason = NonBio;  return false; }
            if (thingDef.race.body.defName.Contains("AIRobot")) { FailReason = NonBio; return false; }
            if (thingDef.defName.Contains("TM_"))
            {
                if (thingDef.defName.Contains("Undead") || thingDef.defName.Contains("Minion") || thingDef.defName.Contains("Demon")) { FailReason = NonBio; return false; }
            }
            if (thingDef.race.FleshType.defName.Contains("TM_StoneFlesh")) { FailReason = NonBio; return false; }
            if (thingDef.race.FleshType.defName.Contains("Chaos") && thingDef.race.FleshType.defName.Contains("Deamon")) { FailReason = NonBio; return false; }
            if (thingDef.race.FleshType.defName.Contains("Construct") && thingDef.race.FleshType.defName.Contains("Flesh")) { FailReason = NonBio; return false; }
            if (thingDef.race.baseBodySize < 0.65f && !thingDef.race.Humanlike) { FailReason = string.Format("Too Small", thingDef); return false; }


            return true;
        }

        public static bool isInfectablePawn(Pawn pawn, bool allowinfected = false)
        {
            return isInfectablePawn(pawn, out string FailReason, allowinfected);
        }
        public static bool isInfectablePawn(Pawn pawn, out string FailReason, bool allowinfected = false)
        {
            return isInfectableThing(pawn.def, out FailReason) && (allowinfected || !pawn.isHost());
        }

        public static bool isInfectablePawnKind(PawnKindDef pawn)
        {
            return isInfectableThing(pawn.race, out string FailReason);
        }
        public static bool isInfectablePawnKind(PawnKindDef pawn, out string FailReason)
        {
            return isInfectableThing(pawn.race, out FailReason);
        }
        */
        public static List<Pawn> SpawnedInfectablePawns(Map map)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.isPotentialHost());
        }

        public static int TotalSpawnedInfectablePawnCount(Map map)
        {
            return SpawnedInfectablePawns(map).Count;
        }

        public static List<Pawn> SpawnedInfectablePawns(Map map, int radius, IntVec3 position)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.isPotentialHost() && XenomorphUtil.DistanceBetween(x.Position, position) < radius);
        }

        public static int TotalSpawnedInfectablePawnCount(Map map, int radius, IntVec3 position)
        {
        //    Log.Message(string.Format("TotalSpawnedInfectablePawnCount: {0}", SpawnedInfectablePawns(map, radius, position).Count));
            return SpawnedInfectablePawns(map, radius, position).Count;
        }

        public static List<Pawn> SpawnedInfectablePawns(Map map, int radius, IntVec3 position, IntVec3 otherposition)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.isPotentialHost() && XenomorphUtil.DistanceBetween(otherposition, position) < radius);
        }

        public static int TotalSpawnedInfectablePawnCount(Map map, int radius, IntVec3 position, IntVec3 otherposition)
        {
            return SpawnedInfectablePawns(map, radius, position, otherposition).Count;
        }

        public static bool isXenomorphInfectedPawn(Pawn pawn)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            if (hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection, false)) return true;
            if (hediffSet.HasHediff(XenomorphDefOf.RRY_HiddenXenomorphImpregnation, false)) return true;
            if (hediffSet.HasHediff(XenomorphDefOf.RRY_XenomorphImpregnation, false)) return true;
            return false;
        }

        public static bool isNeomorphInfectedPawn(Pawn pawn)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            if (hediffSet.HasHediff(XenomorphDefOf.RRY_HiddenNeomorphImpregnation, false)) return true;
            if (hediffSet.HasHediff(XenomorphDefOf.RRY_NeomorphImpregnation, false)) return true;
            return false;
        }

        public static bool IsInfectedPawn(Pawn pawn)
        {
            if (isXenomorphInfectedPawn(pawn) || isNeomorphInfectedPawn(pawn)) return true;
            return false;
        }

        public static List<Pawn> SpawnedInfectedPawns(Map map)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x));
        }

        public static int TotalSpawnedInfectedPawnCount(Map map)
        {
            return SpawnedInfectedPawns(map).Count;
        }

        public static bool IsXenomorphPawn(Pawn pawn)
        {
            if (pawn.RaceProps.FleshType == XenomorphRacesDefOf.RRY_Xenomorph) return true;
            /*
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_FaceHugger) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_RoyaleHugger) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Predalien) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Runner) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Drone) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Warrior) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen) return true;
            */
            return false;
        }

        public static bool IsXenomorphPawn(PawnKindDef pawn)
        {
            if (pawn.RaceProps.FleshType == XenomorphRacesDefOf.RRY_Xenomorph) return true;
            /*
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_FaceHugger) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_RoyaleHugger) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Predalien) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Runner) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Drone) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Warrior) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen) return true;
            */
            return false;
        }

        public static bool IsXenomorphFacehugger(Pawn pawn)
        {
            if (pawn.kindDef.race == XenomorphRacesDefOf.RRY_Xenomorph_FaceHugger) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_FaceHugger) return true;
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_RoyaleHugger) return true;
            return false;
        }

        public static bool IsNeomorphPawn(Pawn pawn)
        {
            if (pawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Neomorph) return true;
            if (pawn.RaceProps.FleshType == XenomorphRacesDefOf.RRY_Neomorph) return true;
            return false;
        }

        public static bool IsXenomorph(Pawn pawn)
        {
            if (IsXenomorphPawn(pawn) || IsNeomorphPawn(pawn)) return true;
            return false;
        }

        public static List<Pawn> SpawnedXenomorphPawns(Map map)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsXenomorph(x));
        }

        public static int TotalSpawnedXenomorphPawnCount(Map map)
        {
            return SpawnedXenomorphPawns(map).Count;
        }

        public static List<Pawn> SpawnedFacehuggerPawns(Map map)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsXenomorphFacehugger(x));
        }

        public static List<Pawn> SpawnedFacehuggerPawns(Map map, int radius, IntVec3 position)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsXenomorphFacehugger(x) && position.InHorDistOf(x.Position, radius));
        }

        public static List<Pawn> SpawnedFacehuggerPawns(Map map, int radius, IntVec3 position, IntVec3 otherposition)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsXenomorphFacehugger(x) && XenomorphUtil.DistanceBetween(otherposition, position) < radius);
        }

        public static int TotalSpawnedFacehuggerPawnCount(Map map)
        {
            return SpawnedFacehuggerPawns(map).Count;
        }

        public static int TotalSpawnedFacehuggerPawnCount(Map map, int radius, IntVec3 position)
        {
            return SpawnedFacehuggerPawns(map, radius, position).Count;
        }

        public static int TotalSpawnedFacehuggerPawnCount(Map map, int radius, IntVec3 position, IntVec3 otherposition)
        {
            return SpawnedFacehuggerPawns(map, radius, position, otherposition).Count;
        }

        public static List<Pawn> SpawnedFacehuggerPawns(Map map, int radius, Pawn pawn, IntVec3 otherposition)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsXenomorphFacehugger(x) && XenomorphUtil.DistanceBetween(otherposition, pawn.Position) < radius && pawn.CanReach(x, PathEndMode.ClosestTouch, Danger.Deadly));
        }

        public static int TotalSpawnedFacehuggerPawnCount(Map map, int radius, Pawn pawn, IntVec3 otherposition)
        {
            return SpawnedFacehuggerPawns(map, radius, pawn.Position, otherposition).Count;
        }

        public static List<Pawn> SpawnedFacehuggerPawns(Map map, int radius, Pawn pawn)
        {
            return map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsXenomorphFacehugger(x) && XenomorphUtil.DistanceBetween(x.Position, pawn.Position) < radius && x.CanReach(pawn, PathEndMode.ClosestTouch, Danger.Deadly));
        }

        public static int TotalSpawnedFacehuggerPawnCount(Map map, int radius, Thing thing)
        {
            return SpawnedFacehuggerPawns(map, radius, thing.Position).Count;
        }

        public static bool IsXenomorphCorpse(Corpse corpse)
        {
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_FaceHugger) return true;
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_RoyaleHugger) return true;
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Predalien) return true;
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Runner) return true;
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Drone) return true;
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Warrior) return true;
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen) return true;
            if (corpse.InnerPawn.RaceProps.FleshType == XenomorphRacesDefOf.RRY_Xenomorph) return true;
            return false;
        }

        public static bool IsNeomorphCorpse(Corpse corpse)
        {
            if (corpse.InnerPawn.kindDef == XenomorphDefOf.RRY_Xenomorph_Neomorph) return true;
            return false;
        }

        public static bool IsXenoCorpse(Corpse corpse)
        {
            if (IsXenomorphCorpse(corpse) || IsNeomorphCorpse(corpse)) return true;
            return false;
        }

        public static bool IsXenomorphFaction(Pawn pawn)
        {
            if (pawn.Faction == Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph)) return true;
            return false;
        }

        public static bool QueenPresent(Map map, out Pawn Queen)
        {
            foreach (var p in map.mapPawns.AllPawnsSpawned)
            {
                if (p.kindDef == XenomorphDefOf.RRY_Xenomorph_Queen)
                {
                    Queen = p;
                    return true;
                }
            }
            Queen = null;
            return false;
        }

        // Egg stuff
        public static bool EggsPresent(Map map)
        {
            return TotalSpawnedEggCount(map) > 0;
        }

        public static Thing ClosestReachableEgg(Pawn pawn)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_EggXenomorphFertilized), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, null, null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableEggNeedsHost(Pawn pawn)
        {
            List<Thing> list = SpawnedEggsNeedHosts(pawn.Map);
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_EggXenomorphFertilized), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => list.Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);

            return thing;
        }
        public static int TotalSpawnedEggCount(Map map)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_EggXenomorphFertilized).Count;
        }
        public static int TotalSpawnedEggNeedHostsCount(Map map)
        {
            return SpawnedEggsNeedHosts(map).Count;
        }
        public static List<Thing> SpawnedEggs(Map map)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_EggXenomorphFertilized);
        }
        public static int TotalSpawnedThingCount(ThingDef thing, Map map)
        {
            return map.listerThings.ThingsOfDef(thing).Count;
        }
        public static List<Thing> SpawnedThings(ThingDef thing, Map map)
        {
            return map.listerThings.ThingsOfDef(thing);
        }
        public static List<Thing> SpawnedEggsNeedHosts(Map map)
        {
            List<Thing> list = new List<Thing>();
            foreach (var item in SpawnedEggs(map))
            {
                Pawn host = (Pawn)GenClosest.ClosestThingReachable(item.Position, item.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 1, x => x.isPotentialHost(), null, 0, -1, false, RegionType.Set_Passable, false);
                if (host == null)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        // Cocoon stuff
        public static bool CocoonsPresent(Map map, ThingDef t)
        {
            return TotalSpawnedCocoonCount(map, t) > 0;
        }
        public static bool EmptyCocoonsPresent(Map map, ThingDef t)
        {
            return TotalSpawnedEmptyCocoonCount(map, t) > 0;
        }
        public static Thing ClosestReachableCocoon(Pawn pawn, ThingDef t)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, null, null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableEmptyCocoon(Pawn pawn, ThingDef t)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => (x is Building_XenomorphCocoon XC && XC.AnyUnoccupiedSleepingSlot)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableCocoonToEgg(Thing egg, ThingDef t)
        {
            Thing thing = GenClosest.ClosestThingReachable(egg.Position, egg.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 10f, null, null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableEmptyCocoonToEgg(Thing egg, ThingDef t)
        {
            Thing thing = GenClosest.ClosestThingReachable(egg.Position, egg.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 10f, (x => (x is Building_XenomorphCocoon XC && XC.AnyUnoccupiedSleepingSlot && XC.OwnersForReading.NullOrEmpty())), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableCocoonThatEggNeedsHost(Pawn pawn, ThingDef t)
        {
            Thing thing;
            List<Thing> list = SpawnedEggsNeedHosts(pawn.Map);
            thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_EggXenomorphFertilized), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => list.Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            thing = GenClosest.ClosestThingReachable(thing.Position, thing.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 10f, (x => (x is Building_XenomorphCocoon XC)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableEmptyCocoonThatEggNeedsHost(Pawn pawn, ThingDef t)
        {
            Thing thing;
            List<Thing> list = SpawnedEggsNeedHosts(pawn.Map);
            List<Thing> cocoonlist = SpawnedEmptyCocoons(pawn.Map, t);

            thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_EggXenomorphFertilized), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => list.Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            thing = GenClosest.ClosestThingReachable(thing.Position, thing.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 10f, (x => cocoonlist.Contains(x) && (x is Building_XenomorphCocoon XC && XC.AnyUnoccupiedSleepingSlot)), null, 0, -1, false, RegionType.Set_Passable, false);

            return thing;
        }

        public static int TotalSpawnedCocoonCount(Map map, ThingDef t)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid).Count;
        }
        public static List<Thing> SpawnedCocoons(Map map, ThingDef t)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid);
        }
        public static int TotalSpawnedEmptyCocoonCount(Map map, ThingDef t)
        {
            return SpawnedEmptyCocoons(map, t).Count;
        }
        public static List<Thing> SpawnedEmptyCocoons(Map map, ThingDef t)
        {
            return SpawnedCocoons(map, t).ToList().FindAll(x => (x is Building_XenomorphCocoon XC && XC.AnyUnoccupiedSleepingSlot));
        }


        // Hivelike stuff
        public static bool HivelikesPresent(Map map)
        {
            return TotalSpawnedHivelikeCount(map) > 0;
        }
        public static Thing ClosestReachableHivelike(Pawn pawn)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => XenomorphUtil.SpawnedHives(pawn.Map).Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableHivelike(Pawn pawn, List<Thing> Hivelikes)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => Hivelikes.Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableParentHivelike(Pawn pawn)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => XenomorphUtil.SpawnedParentHives(pawn.Map).Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableParentHivelike(Pawn pawn, List<Thing> Hivelikes)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => Hivelikes.Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableChildHivelike(Pawn pawn)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => XenomorphUtil.SpawnedChildHives(pawn.Map).Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }
        public static Thing ClosestReachableChildHivelike(Pawn pawn, List<Thing> Hivelikes)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, (x => Hivelikes.Contains(x)), null, 0, -1, false, RegionType.Set_Passable, false);
            return thing;
        }

        public static int TotalSpawnedHivelikeCount(Map map)
        {
            List<Thing> lista = map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive);
            List<Thing> listb = map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive_Child);
            return lista.Count() + listb.Count();
        }
        
        public static List<Thing> SpawnedHives(Map map)
        {
            List<Thing> lista = map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive);
            List<Thing> listb = map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive_Child);
            return lista.Concat(listb).ToList();
        }
        public static List<HiveLike> SpawnedHivelikes(Map map)
        {
            List<HiveLike> list = new List<HiveLike>();
            SpawnedHives(map).ForEach((x) => list.Add((HiveLike)x));
            return list;
        }

        public static List<Thing> SpawnedParentHives(Map map)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive);
        }
        public static List<HiveLike> SpawnedParentHivelikes(Map map)
        {
            List<HiveLike> list = new List<HiveLike>();
            SpawnedParentHives(map).ForEach((x) => list.Add((HiveLike)x));
            return list;
        }
        public static int TotalSpawnedParentHivelikeCount(Map map)
        {
            return SpawnedParentHivelikes(map).Count();
        }

        public static List<Thing> SpawnedChildHives(Map map)
        {
            return map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_Xenomorph_Hive_Child);
        }
        public static List<HiveLike> SpawnedChildHivelikes(Map map)
        {
            List<HiveLike> list = new List<HiveLike>();
            SpawnedChildHives(map).ForEach((x) => list.Add((HiveLike)x));
            return list;
        }
        public static int TotalSpawnedChildHivelikeCount(Map map)
        {
            return SpawnedChildHivelikes(map).Count(); 
        }

        // space between / distance between
        public static float DistanceBetween(IntVec3 a, IntVec3 b)
        {
            double distance = GetDistance(a.x, a.z, b.x, b.z);
            return (float)distance;
        }
        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

    }

    [StaticConstructorOnStartup]
    public static class XenomorphStaticUtil
    {
        // Token: 0x06003BF5 RID: 15349 RVA: 0x001C4174 File Offset: 0x001C2574
        public static Toil NPCWithProgressBar(this Toil toil, TargetIndex ind, Func<float> progressGetter, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f)
        {
            Effecter effecter = null;
            toil.AddPreTickAction(delegate
            {
                /*
                if (toil.actor.Faction != Faction.OfPlayer)
                {
                    return;
                }
                */
                if (effecter == null)
                {
                    EffecterDef progressBar = EffecterDefOf.ProgressBar;
                    effecter = progressBar.Spawn();
                }
                else
                {
                    LocalTargetInfo target = toil.actor.CurJob.GetTarget(ind);
                    if (!target.IsValid || (target.HasThing && !target.Thing.Spawned))
                    {
                        effecter.EffectTick(toil.actor, TargetInfo.Invalid);
                    }
                    else if (interpolateBetweenActorAndTarget)
                    {
                        effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map), toil.actor);
                    }
                    else
                    {
                        effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map), TargetInfo.Invalid);
                    }
                    MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                    if (mote != null)
                    {
                        mote.progress = Mathf.Clamp01(progressGetter());
                        mote.offsetZ = offsetZ;
                    }
                }
            });
            toil.AddFinishAction(delegate
            {
                if (effecter != null)
                {
                    effecter.Cleanup();
                    effecter = null;
                }
            });
            return toil;
        }

        // Token: 0x06003BF6 RID: 15350 RVA: 0x001C41E8 File Offset: 0x001C25E8
        public static Toil NPCWithProgressBarToilDelay(this Toil toil, TargetIndex ind, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f)
        {
            return toil.NPCWithProgressBar(ind, () => 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)toil.defaultDuration, interpolateBetweenActorAndTarget, offsetZ);
        }

        // Token: 0x06003BF7 RID: 15351 RVA: 0x001C421C File Offset: 0x001C261C
        public static Toil NPCWithProgressBarToilDelay(this Toil toil, TargetIndex ind, int toilDuration, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f)
        {
            return toil.NPCWithProgressBar(ind, () => 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)toilDuration, interpolateBetweenActorAndTarget, offsetZ);
        }

    }
}
