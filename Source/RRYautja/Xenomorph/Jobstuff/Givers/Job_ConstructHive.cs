using RRYautja;
using RRYautja.ExtensionMethods;
using RRYautja.Xenomorph.Hives;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x020000A4 RID: 164
    public class JobGiver_ConstructHive : ThinkNode_JobGiver
    {
        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_ConstructHive jobGiver_ConstructHive = (JobGiver_ConstructHive)base.DeepCopy(resolve);
            jobGiver_ConstructHive.MiningRange = this.MiningRange;
            return jobGiver_ConstructHive;
        }

        private int MiningRange;
        // Token: 0x0600041E RID: 1054 RVA: 0x0002CA54 File Offset: 0x0002AE54

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.isXenomorph(out Comp_Xenomorph _Xenomorph))
            {
                return null;
            }
            if (pawn.def != XenomorphRacesDefOf.RRY_Xenomorph_Queen && pawn.def != XenomorphRacesDefOf.RRY_Xenomorph_Drone && pawn.def != XenomorphRacesDefOf.RRY_Xenomorph_Predalien)
            {
                return null;
            }
            Map map = pawn.Map;
            IntVec3 pos = pawn.Position;
            IntVec3 HiveCenter = _Xenomorph.HiveLoc;
            Region region = pawn.GetRegion(RegionType.Set_Passable);
            if (region == null)
            {
                return null;
            }
            if (HiveCenter == pawn.InteractionCell)
            {
                return null;
            }
            bool centerNode = HiveCenter.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive) != null;
            bool centerChild = HiveCenter.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive_Child) != null;
            bool centerSlime = HiveCenter.GetFirstThing(pawn.Map, XenomorphDefOf.RRY_Xenomorph_Hive_Slime) != null;
            bool centerFilled = HiveCenter.Filled(pawn.Map);

            if (centerNode)
            {
                MiningRange = 6;
            }
            else
            {
                MiningRange = 3;
                if (!centerChild)
                {

                    if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen || pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone)
                    {
                        if (!centerNode)
                        {
                            IntVec3 c = HiveCenter;
                            if (c.InBounds(pawn.Map) && c.Roofed(pawn.Map) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.Deadly))
                            {
                                return new Job(XenomorphDefOf.RRY_Job_Xenomorph_Construct_Hive_Node, c)
                                {
                                    ignoreDesignations = false
                                };
                            }
                        }
                        return null;
                    }
                }
            }
            if (!centerChild)
            {
                foreach (var structure in HiveStructure.HiveStruct(HiveCenter).Where(x => x.GetThingList(map).Any(z => !z.def.defName.Contains("RRY_Xenomorph_Hive")) && x.DistanceTo(HiveCenter) <= MiningRange && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly, layer: ReservationLayerDefOf.Floor) && x.GetFirstBuilding(map) == null))
                {
                    // Don't build a wall that would trap the drone — check path to hive center after wall
                    if (WouldTrapPawn(pawn, structure, HiveCenter, map)) continue;
                    return new Job(XenomorphDefOf.RRY_Job_Xenomorph_Construct_Hive_Wall, structure)
                    {
                        ignoreDesignations = false
                    };
                }
                foreach (var structure in HiveStructure.HiveWallGen(HiveCenter, MiningRange).Where(x => x.GetThingList(map).Any(z => !z.def.defName.Contains("RRY_Xenomorph_Hive")) && x.DistanceTo(HiveCenter) <= MiningRange + 2 && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Deadly, layer: ReservationLayerDefOf.Floor) && x.GetFirstBuilding(map) == null))
                {
                    // Don't build a wall that would trap the drone
                    if (WouldTrapPawn(pawn, structure, HiveCenter, map)) continue;
                    return new Job(XenomorphDefOf.RRY_Job_Xenomorph_Construct_Hive_Wall, structure)
                    {
                        ignoreDesignations = false
                    };
                }
                Predicate<IntVec3> validator2 = delegate (IntVec3 y)
                {
                    bool roofed = (y.Roofed(pawn.Map));
                    bool result = !roofed && XenomorphUtil.DistanceBetween(y, HiveCenter) <= MiningRange && pawn.CanReserveAndReach(y, PathEndMode.OnCell, Danger.Deadly, layer: ReservationLayerDefOf.Ceiling);
                    return result;
                };
                if (RCellFinder.TryFindRandomCellNearWith(HiveCenter, validator2, pawn.Map, out IntVec3 c2, MiningRange, MiningRange))
                {
                    if (c2.InBounds(pawn.Map) && pawn.CanReserveAndReach(c2, PathEndMode.OnCell, Danger.Deadly))
                    {
                        return new Job(XenomorphDefOf.RRY_Job_Xenomorph_Construct_Hive_Roof, c2)
                        {
                            ignoreDesignations = false
                        };
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if building a wall at the given cell would trap the pawn
        /// (cut off its path to the hive center or map edge).
        /// Temporarily marks the cell as blocked, checks reachability, then restores.
        /// </summary>
        private bool WouldTrapPawn(Pawn pawn, IntVec3 wallCell, IntVec3 hiveCenter, Map map)
        {
            // If the pawn is standing on the wall cell, it would definitely be trapped
            if (pawn.Position == wallCell) return true;

            // Quick check: if the cell is already impassable, building a wall changes nothing
            if (wallCell.Filled(map)) return false;

            // Temporarily simulate the wall by checking if pawn can still reach hive center
            // without going through that cell. We use a fake edifice check:
            // If all 4 adjacent cells of the pawn (or the wall cell) are walls/filled, the pawn is trapped.
            int blockedAdjacents = 0;
            foreach (var dir in GenAdj.CardinalDirections)
            {
                IntVec3 adj = wallCell + dir;
                if (!adj.InBounds(map)) { blockedAdjacents++; continue; }
                if (adj.Filled(map) || adj.GetEdifice(map)?.def == XenomorphDefOf.RRY_Xenomorph_Hive_Wall)
                    blockedAdjacents++;
            }
            // If 3+ sides are blocked, building a wall here would create a dead-end
            if (blockedAdjacents >= 3) return true;

            return false;
        }

    }

    // Token: 0x02000089 RID: 137
    public class JobDriver_ConstructHive_Wall : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
        }

        // Token: 0x06000392 RID: 914 RVA: 0x00024590 File Offset: 0x00022990
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, ReservationLayerDefOf.Floor, errorOnFailed);
        }

        public ThingDef MyDef { get { return XenomorphDefOf.RRY_Xenomorph_Hive_Wall; } }

        // Token: 0x06000393 RID: 915 RVA: 0x000245C8 File Offset: 0x000229C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.A);
            prepare.NPCWithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            prepare.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
            prepare.PlaySustainerOrSound(() => SoundDefOf.Vomit);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                IntVec3 cell = TargetA.Cell;
                if (cell.GetFirstBuilding(actor.Map) != null) return;
                Thing thing = ThingMaker.MakeThing(MyDef);
                GenSpawn.Spawn(thing, cell, actor.Map, Rot4.South, WipeMode.Vanish, false);
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = 600;
    }

    // Token: 0x02000089 RID: 137
    public class JobDriver_ConstructHive_Node : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
        }

        // Token: 0x06000392 RID: 914 RVA: 0x00024590 File Offset: 0x00022990
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        public ThingDef MyDef { get { return XenomorphDefOf.RRY_Xenomorph_Hive; } }

        // Token: 0x06000393 RID: 915 RVA: 0x000245C8 File Offset: 0x000229C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.None);
            prepare.NPCWithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            prepare.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
            prepare.PlaySustainerOrSound(() => SoundDefOf.Vomit);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                IntVec3 cell = TargetA.Cell;
                if (cell.GetFirstBuilding(actor.Map) != null) return;
                Thing thing = ThingMaker.MakeThing(MyDef);
                HiveLike hive = (HiveLike)thing;
                hive.active = false;
                hive.canSpawnPawns = false;
                hive.getsQueen = false;
                hive.InitialPawnsPoints = 0;
                hive.MaxSpawnedPawnsPoints = 0;
                GenSpawn.Spawn(thing, cell, actor.Map, Rot4.South, WipeMode.Vanish, false);
                hive.Lord.AddPawn(actor);
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = 2400;
    }

    // Token: 0x02000089 RID: 137
    public class JobDriver_ConstructHive_Child : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
        }

        // Token: 0x06000392 RID: 914 RVA: 0x00024590 File Offset: 0x00022990
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        public ThingDef MyDef { get { return XenomorphDefOf.RRY_Xenomorph_Hive_Child; } }

        // Token: 0x06000393 RID: 915 RVA: 0x000245C8 File Offset: 0x000229C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.None);
            prepare.NPCWithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            prepare.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
            prepare.PlaySustainerOrSound(() => SoundDefOf.Vomit);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                IntVec3 cell = TargetA.Cell;
                if (cell.GetFirstBuilding(actor.Map) != null) return;
                Thing thing = ThingMaker.MakeThing(MyDef);
                GenSpawn.Spawn(thing, cell, actor.Map, Rot4.South, WipeMode.Vanish, false);
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = 2400;
    }

    // Token: 0x02000089 RID: 137
    public class JobDriver_ConstructHive_Slime : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
        }

        // Token: 0x06000392 RID: 914 RVA: 0x00024590 File Offset: 0x00022990
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        private ThingDef _myDef; public ThingDef MyDef { get { return _myDef ?? (_myDef = XenomorphDefOf.RRY_Xenomorph_Hive_Slime); } set { _myDef = value; } }

        // Token: 0x06000393 RID: 915 RVA: 0x000245C8 File Offset: 0x000229C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.None);
            prepare.NPCWithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            prepare.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
            prepare.PlaySustainerOrSound(() => SoundDefOf.Vomit);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                IntVec3 cell = TargetA.Cell;
                if (cell.GetFirstBuilding(actor.Map) != null) return;
                MyDef = XenomorphDefOf.RRY_Xenomorph_Hive;
                Thing thing = ThingMaker.MakeThing(MyDef);
                HiveLike hive = (HiveLike)thing;
                GenSpawn.Spawn(thing, cell, actor.Map, Rot4.South, WipeMode.Vanish, false);
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = 60;
    }
 
    // Token: 0x02000089 RID: 137
    public class JobDriver_ConstructHive_Roof : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
        }

        // Token: 0x06000392 RID: 914 RVA: 0x00024590 File Offset: 0x00022990
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return true;
        }
        

        // Token: 0x06000393 RID: 915 RVA: 0x000245C8 File Offset: 0x000229C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.None);
            prepare.NPCWithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            prepare.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
            prepare.PlaySustainerOrSound(() => SoundDefOf.Vomit);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                base.Map.roofGrid.SetRoof(TargetA.Cell, RoofDefOf.RoofConstructed);
                MoteMaker.PlaceTempRoof(TargetA.Cell, base.Map);
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = 60;
    }
}
