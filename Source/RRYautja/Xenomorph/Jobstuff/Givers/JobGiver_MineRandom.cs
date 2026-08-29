using RRYautja;
using RRYautja.ExtensionMethods;
using RRYautja.Xenomorph.Hives;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x020000A4 RID: 164
    public class JobGiver_MineRandomNearHive : ThinkNode_JobGiver
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_MineRandomNearHive jobGiver_MineRandomNearHive = (JobGiver_MineRandomNearHive)base.DeepCopy(resolve);
            jobGiver_MineRandomNearHive.MiningRange = this.MiningRange;
            return jobGiver_MineRandomNearHive;
        }

        private int MiningRange = 5;

        // Token: 0x0600041E RID: 1054 RVA: 0x0002CA54 File Offset: 0x0002AE54
        protected override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 hivec = pawn.mindState.duty.focus.Cell;
            List<IntVec3> pillarLoc = new List<IntVec3>()
            {
                // Cardinals
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x + 3,
                    z = pawn.mindState.duty.focus.Cell.z
                },
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x - 3,
                    z = pawn.mindState.duty.focus.Cell.z
                },
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x,
                    z = pawn.mindState.duty.focus.Cell.z + 3
                },
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x,
                    z = pawn.mindState.duty.focus.Cell.z - 3
                },
                // Corners
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x + 2,
                    z = pawn.mindState.duty.focus.Cell.z + 2
                },
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x - 2,
                    z = pawn.mindState.duty.focus.Cell.z + 2
                },
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x + 2,
                    z = pawn.mindState.duty.focus.Cell.z - 2
                },
                new IntVec3
                {
                    x = pawn.mindState.duty.focus.Cell.x - 2,
                    z = pawn.mindState.duty.focus.Cell.z - 2
                }
            };
            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }

            bool flag1 = hivec.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive) != null;
            bool flag2 = hivec.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive_Child) != null;
            bool flag3 = hivec.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive_Slime) != null;
            bool flag4 = !flag1 && !flag2 && !flag3;
            if (flag4 )
            {
                if (hivec.Filled(pawn.Map) && pawn.CanReach(hivec, PathEndMode.OnCell, Danger.Deadly))
                {
                    Building edifice = hivec.GetEdifice(pawn.Map);
                    return new Job(JobDefOf.Mine, edifice)
                    {
                        ignoreDesignations = true
                    };
                }
                if (!hivec.Filled(pawn.Map) && pawn.CanReach(hivec, PathEndMode.OnCell, Danger.Deadly))
                {
                    GenSpawn.Spawn(XenomorphDefOf.RRY_Xenomorph_Hive_Slime, pawn.mindState.duty.focus.Cell, pawn.Map);
                }
            }
            for (int i = 0; i < 40; i++)
            {
                IntVec3 randomCell = region.RandomCell;
                for (int j = 0; j < 4; j++)
                {
                    IntVec3 c = randomCell + GenAdj.CardinalDirections[j];
                    if (c.InBounds(pawn.Map) && c.Roofed(pawn.Map))
                    {

                        Building edifice = c.GetEdifice(pawn.Map);
                        if (edifice != null && edifice.def.mineable && (edifice.def.passability == Traversability.Impassable || edifice.def.IsDoor) && edifice.def.size == IntVec2.One && edifice.def != ThingDefOf.CollapsedRocks && edifice.def != XenomorphDefOf.RRY_Xenomorph_Hive_Wall && pawn.CanReserve(edifice, 1, -1, null, false) && XenomorphUtil.DistanceBetween(edifice.Position, pawn.mindState.duty.focus.Cell) <= MiningRange)
                        {
                            if (!pillarLoc.Contains(edifice.Position))
                            {
                                return new Job(JobDefOf.Mine, edifice)
                                {
                                    ignoreDesignations = true
                                };
                            }
                        }
                        /*
                        else if (edifice==null)
                        {
                            if (!pillarLoc.Contains(edifice.Position))
                            {
                                return new Job(JobDefOf.Mine, edifice)
                                {
                                    ignoreDesignations = true
                                };
                            }
                        }
                        */
                    }
                }
            }
            return null;
        }

    }

    // Token: 0x020000A4 RID: 164
    public class JobGiver_MineOutHive : ThinkNode_JobGiver
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_MineOutHive jobGiver_MineOutHive = (JobGiver_MineOutHive)base.DeepCopy(resolve);
            jobGiver_MineOutHive.MiningRange = this.MiningRange;
            return jobGiver_MineOutHive;
        }

        private int MiningRange;

        private float CellsInScanRadius
        {
            get
            {
                return (float)GenRadial.NumCellsInRadius(MiningRange);
            }
        }

        // Token: 0x0600041E RID: 1054 RVA: 0x0002CA54 File Offset: 0x0002AE54
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Map==null)
            {
            //    Log.Message("map == null");
                return null;
            }
            Map map = pawn.Map;
            if (!pawn.isXenomorph(out Comp_Xenomorph _Xenomorph))
            {
            //    Log.Message("not xenomorph");
                return null;
            }
            IntVec3 HiveCenter = IntVec3.Invalid;
            if (_Xenomorph.HiveLoc.IsValid && _Xenomorph.HiveLoc.InBounds(map) && _Xenomorph.HiveLoc != IntVec3.Zero)
            {
                HiveCenter = _Xenomorph.HiveLoc;
            }
            else
            {
            //    Log.Message(string.Format("not _Xenomorph.HiveLoc({0})", _Xenomorph.HiveLoc));
                return null;
            }

            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }
            bool flag1 = HiveCenter.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive) != null;
            bool flag2 = HiveCenter.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive_Child) != null;
            bool flag3 = HiveCenter.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive_Slime) != null;
            if (pawn.GetLord() is Lord L && L != null)
            {
                if ((L.CurLordToil is LordToil_DefendAndExpandHiveLike || L.CurLordToil is LordToil_DefendHiveLikeAggressively) && L.CurLordToil is LordToil_HiveLikeRelated THL)
                {
                    if (THL != null)
                    {
                        if (THL.Data!=null)
                        {
                            HiveLike hive = THL.Data.assignedHiveLikes.TryGetValue(pawn);
                            if (hive!=null)
                            {
                                flag1 = hive.def == XenomorphDefOf.RRY_Xenomorph_Hive;
                                flag2 = hive.def == XenomorphDefOf.RRY_Xenomorph_Hive_Child;
                            }
                        }
                    }
                }
            }
            if (flag1)
            {
                MiningRange = 7;
            }
            else
            if (flag2)
            {
                MiningRange = 3;
            }
            else
            {
                MiningRange = 5;
            }
            bool canReachMapEdge = pawn.CanReachMapEdge();
            if (!canReachMapEdge)
            {
                MiningRange = 10;
            }
            for (int i = 0; i < GenRadial.NumCellsInRadius(MiningRange); i++)
            {
                IntVec3 c = HiveCenter + GenRadial.RadialPattern[i];
                if (HiveStructure.HiveStruct(HiveCenter).Contains(c) || HiveStructure.HiveWalls(HiveCenter).Contains(c))
                    continue;
                Building edifice = c.GetEdifice(pawn.Map);
                if (edifice == null || edifice.def.size != IntVec2.One || edifice.def == ThingDefOf.CollapsedRocks || edifice.def == XenomorphDefOf.RRY_Xenomorph_Hive_Wall)
                    continue;
                // Only mine natural rock/mountains, not player-built structures
                if (!edifice.def.mineable)
                    continue;
                if (XenomorphUtil.DistanceBetween(edifice.Position, HiveCenter) > MiningRange)
                    continue;
                // Only do expensive pathfinding on valid candidates
                if (!pawn.CanReserveAndReach(edifice, PathEndMode.ClosestTouch, Danger.Deadly, 1, 1))
                    continue;
                bool xenobuilding = edifice.GetType() != typeof(Building_XenoEgg) && edifice.GetType() != typeof(Building_XenomorphCocoon) && edifice.GetType() != typeof(HiveLike);
                if (xenobuilding)
                {
                    return new Job(JobDefOf.Mine, edifice)
                    {
                        ignoreDesignations = true
                    };
                }
                }

                // If the drone can't reach the map edge, it might be stuck inside hive walls.
                // Allow mining through hive walls to escape. Reuse cached result (no second pathfind).
                if (!canReachMapEdge)
                {
                    for (int i = 0; i < GenRadial.NumCellsInRadius(3); i++)
                    {
                        IntVec3 c = pawn.Position + GenRadial.RadialPattern[i];
                        if (c.InBounds(pawn.Map))
                        {
                            Building edifice = c.GetEdifice(pawn.Map);
                            if (edifice != null && edifice.def == XenomorphDefOf.RRY_Xenomorph_Hive_Wall && pawn.CanReserveAndReach(edifice, PathEndMode.Touch, Danger.Deadly, 1, 1))
                            {
                                return new Job(JobDefOf.Mine, edifice)
                                {
                                    ignoreDesignations = true
                                };
                            }
                        }
                    }
                }

            // Nothing left to mine — go back to the tunnel briefly
            // This staggers think cycles and reduces the lag spike when all drones finish mining at once
            HiveLike tunnel = JobGiver_EnterHiveTunnel.FindMyTunnel(pawn);
            if (tunnel != null && tunnel.HitPoints > 0 && !tunnel.spawnedPawns.Contains(pawn))
            {
                if (pawn.CanReach(tunnel, PathEndMode.Touch, Danger.Deadly))
                {
                    return new Job(XenomorphDefOf.RRY_Job_Xenomorph_EnterHiveTunnel, tunnel);
                }
                // Can't reach tunnel — try to dig to it
                // Find the first mineable blocker between the pawn and the tunnel
                PawnPath path = pawn.Map.pathFinder.FindPathNow(pawn.Position, tunnel.Position,
                    TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings), null, PathEndMode.Touch);
                if (path.Found)
                {
                    IntVec3 cellBeforeBlocker = pawn.Position;
                    List<IntVec3> nodes = path.NodesReversed;
                    for (int i = nodes.Count - 1; i >= 0; i--)
                    {
                        IntVec3 cell = nodes[i];
                        Building edifice = cell.GetEdifice(pawn.Map);
                        if (edifice != null && edifice.def.passability == Traversability.Impassable
                            && edifice.def != XenomorphDefOf.RRY_Xenomorph_Hive_Wall
                            && edifice.def != ThingDefOf.CollapsedRocks)
                        {
                            Job digJob = DigUtility.PassBlockerJob(pawn, edifice, cellBeforeBlocker, true, true);
                            if (digJob != null)
                            {
                                path.Dispose();
                                return digJob;
                            }
                        }
                        cellBeforeBlocker = cell;
                    }
                }
                path?.Dispose();
            }

            return null;
        }

    }

    // Token: 0x020000A4 RID: 164
    public class JobGiver_ClearHiveEggZone : ThinkNode_JobGiver
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_ClearHiveEggZone jobGiver_ClearHiveEggZone = (JobGiver_ClearHiveEggZone)base.DeepCopy(resolve);
            jobGiver_ClearHiveEggZone.ClearingRange = this.ClearingRange;
            return jobGiver_ClearHiveEggZone;
        }
        
        private int ClearingRange = 2;

        // Token: 0x0600041E RID: 1054 RVA: 0x0002CA54 File Offset: 0x0002AE54
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Map == null)
            {
                return null;
            }
            Map map = pawn.Map;
            if (!pawn.isXenomorph(out Comp_Xenomorph _Xenomorph))
            {
                return null;
            }

            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }

            IntVec3 hiveCell;
            if (!XenomorphUtil.ClosestReachableParentHivelike(pawn).DestroyedOrNull())
            {
                hiveCell = XenomorphUtil.ClosestReachableParentHivelike(pawn).Position;
            }
            else if (!XenomorphUtil.ClosestReachableHiveSlime(pawn).DestroyedOrNull())
            {
                hiveCell = XenomorphUtil.ClosestReachableHiveSlime(pawn).Position;
            }
            else
            {
                hiveCell = _Xenomorph.HiveLoc == IntVec3.Invalid || _Xenomorph.HiveLoc == IntVec3.Zero || !_Xenomorph.HiveLoc.InBounds(map) ? pawn.mindState.duty.focus.Cell : _Xenomorph.HiveLoc;
            }
            if (hiveCell == IntVec3.Invalid || hiveCell == IntVec3.Zero || !hiveCell.InBounds(map))
            {
                return null;
            }
            foreach (var c in GenAdj.AdjacentCellsAndInside)
            {
                if ((hiveCell+c).GetFirstHaulable(pawn.Map) is Thing h && !(h is Pawn) && !(h is Building_XenoEgg) && !(h is Building_XenomorphCocoon))
                {
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} TryGiveJob {1} @ {2} 1", this, pawn.LabelShortCap, (hiveCell + c)));
                    if (pawn.CanReserve(h, 1, -1, null, false))
                    {
                        if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} TryGiveJob {1} @ {2} 2", this, pawn.LabelShortCap, (hiveCell + c)));
                        if (!XenomorphUtil.CanHaulAside(pawn, h, hiveCell, ClearingRange, out IntVec3 c2))
                        {
                            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} TryGiveJob {1} @ {2} 3", this, pawn.LabelShortCap, (hiveCell + c)));
                            return null;
                        }
                        if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} TryGiveJob {1} @ {2} 4", this, pawn.LabelShortCap, (hiveCell + c)));
                        return new Job(JobDefOf.HaulToCell, h, c2)
                        {
                            count = 99999,
                            haulOpportunisticDuplicates = false,
                            haulMode = HaulMode.ToCellNonStorage,
                            ignoreDesignations = true
                        };
                    }
                }
            }
            return null;
        }
    }
    // Token: 0x020000A4 RID: 164
    public class JobGiver_ClearNearHive : ThinkNode_JobGiver
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_ClearNearHive jobGiver_ClearNearHive = (JobGiver_ClearNearHive)base.DeepCopy(resolve);
            jobGiver_ClearNearHive.minClearingRange = this.minClearingRange;
            jobGiver_ClearNearHive.maxClearingRange = this.maxClearingRange;
            return jobGiver_ClearNearHive;
        }

        private int minClearingRange = 1;
        private int maxClearingRange = 5;

        // Token: 0x0600041E RID: 1054 RVA: 0x0002CA54 File Offset: 0x0002AE54
        protected override Job TryGiveJob(Pawn pawn)
        {
            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }
            IntVec3 hiveCell;
            if (!XenomorphUtil.ClosestReachableParentHivelike(pawn).DestroyedOrNull())
            {
                hiveCell = XenomorphUtil.ClosestReachableParentHivelike(pawn).Position;
            }
            else if (!XenomorphUtil.ClosestReachableHiveSlime(pawn).DestroyedOrNull())
            {
                hiveCell = XenomorphUtil.ClosestReachableHiveSlime(pawn).Position;
            }
            else
            {
                hiveCell = pawn.mindState.duty.focus.Cell;
            }
            for (int i = 0; i < 40; i++)
            {
                IntVec3 randomCell = region.RandomCell;

                for (int j = 0; j < 8; j++)
                {
                    IntVec3 c = randomCell + GenAdj.AdjacentCellsAround[j];
                    if (c.InBounds(pawn.Map))
                    {

                        Thing thing = c.GetFirstHaulable(pawn.Map);
                    //    Log.Message(string.Format("thing: {0}", thing));
                        if (thing != null && (thing.def.passability == Traversability.Impassable || thing.def.IsDoor) && thing.def.size == IntVec2.One && thing.def != ThingDefOf.CollapsedRocks && pawn.CanReserve(thing, 1, -1, null, false) && XenomorphUtil.DistanceBetween(thing.Position, pawn.mindState.duty.focus.Cell) >= minClearingRange && XenomorphUtil.DistanceBetween(thing.Position, pawn.mindState.duty.focus.Cell) <= maxClearingRange)
                        {
                            return XenomorphUtil.HaulAsideJobFor(pawn, thing, hiveCell, maxClearingRange);
                        }
                    }
                }
            }
            return null;
        }
    }
}
