using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x020000D6 RID: 214
    public class JobGiver_XenosKidnap : ThinkNode_JobGiver
    {
        private List<Rot4> Rotlist = new List<Rot4>
        {
            Rot4.North,
            Rot4.South,
            Rot4.East,
            Rot4.West
        };

        // Token: 0x0600041A RID: 1050 RVA: 0x0002C918 File Offset: 0x0002AD18
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_XenosKidnap jobGiver_XenosKidnap = (JobGiver_XenosKidnap)base.DeepCopy(resolve);
            jobGiver_XenosKidnap.HuntingRange = this.HuntingRange;
            jobGiver_XenosKidnap.forceRoofed = this.forceRoofed;
            // TODO: canBash removed from Job in 1.6
            // jobGiver_XenosKidnap.canBash = this.canBash;
            jobGiver_XenosKidnap.forceCanDig = this.forceCanDig;
            jobGiver_XenosKidnap.allowCocooned = this.allowCocooned;
            jobGiver_XenosKidnap.allowHosts = this.allowHosts;
            jobGiver_XenosKidnap.minRadius = this.minRadius;
            jobGiver_XenosKidnap.forceCanDigIfAnyHostileActiveThreat = this.forceCanDigIfAnyHostileActiveThreat;
            jobGiver_XenosKidnap.forceCanDigIfCantReachMapEdge = this.forceCanDigIfCantReachMapEdge;
            return jobGiver_XenosKidnap;
        }


        private float HuntingRange = 20f;

        // Token: 0x060004D1 RID: 1233 RVA: 0x0003100C File Offset: 0x0002F40C
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!JobGiverTickThrottle.ShouldRun(pawn)) return null;
            if (pawn == null || pawn.Map == null) return null;
            float Searchradius = HuntingRange;
            Map map = pawn.Map;
            IntVec3 c = IntVec3.Invalid;
            Pawn Victim = null;
            if (!pawn.isXenomorph(out Comp_Xenomorph xenomorph) || map == null)
            {
                return null;
            }
            MapComponent_HiveGrid hiveGrid = pawn.Map.HiveGrid();
            /*
            if (GenAI.InDangerousCombat(pawn))
            {
                Log.Warning(string.Format("{0} is InDangerousCombat", pawn.NameShortColored));
            }
            */
            if (XenomorphKidnapUtility.TryFindGoodKidnapVictim(pawn, Searchradius, out Victim, null,forceRoofed, allowCocooned, minRadius, allowHosts))
            {
                // Limit to 1 drone per downed victim to prevent pathfinding lag
                int maxDronesPerVictim = 1;
                int alreadyTargeting = 0;
                foreach (var otherPawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (otherPawn == pawn || !otherPawn.isXenomorph()) continue;
                    var otherJob = otherPawn.CurJob;
                    if (otherJob != null && otherJob.def == XenomorphDefOf.RRY_Job_Xenomorph_Kidnap && otherJob.targetA.Thing == Victim)
                    {
                        alreadyTargeting++;
                    }
                }
                if (alreadyTargeting >= maxDronesPerVictim)
                {
                    return null; // Another drone already going for this victim
                }
                if (xenomorph.HiveLoc.IsValid && xenomorph.HiveLoc.InBounds(map) && xenomorph.HiveLoc != IntVec3.Zero)
                {
                    c = xenomorph.HiveLoc;
                }
                /*
                else
                if (!hiveGrid.Hivelist.NullOrEmpty())
                {
                    c = hiveGrid.Hivelist.RandomElement().Position;
                }
                else
                if (!hiveGrid.HiveLoclist.NullOrEmpty())
                {
                    c = hiveGrid.HiveLoclist.RandomElement();
                }
                else
                */
                bool selected = pawn.Map != null ? Find.Selector.SelectedObjects.Contains(pawn) && (Prefs.DevMode) : false;
                if (c != IntVec3.Invalid && Victim != null && pawn.CanReach(c, PathEndMode.ClosestTouch, Danger.Deadly, true, false, TraverseMode.PassAllDestroyableThings))
                {
                    Predicate<IntVec3> validator = delegate (IntVec3 y)
                    {
                        bool roofed = (y.Roofed(pawn.Map) && this.forceRoofed) || !this.forceRoofed;
                        bool adjacent = c.AdjacentTo8WayOrInside(y);
                        bool filled = y.Filled(pawn.Map);
                        bool edifice = y.GetEdifice(pawn.Map).DestroyedOrNull();
                        bool building = y.GetFirstBuilding(pawn.Map).DestroyedOrNull();
                        bool thingA = y.GetThingList(pawn.Map).Any(x => x.GetType() == typeof(Building_XenoEgg) && x.GetType() == typeof(Building_XenomorphCocoon) && x.GetType() == typeof(HiveLike));
                        //    Log.Message(string.Format("{0}, adjacent: {1}, filled: {2}, edifice: {3}, building: {4}", y, !adjacent, !filled, edifice, building));
                        return !adjacent && !filled && edifice && building && !thingA && roofed && pawn.CanReserveAndReach(y, PathEndMode.OnCell, Danger.Deadly, layer: ReservationLayerDefOf.Floor);
                    };
                    if (pawn.GetLord() != null && pawn.GetLord() is Lord lord)
                    {
                        //    Log.Message(string.Format("TryFindGoodHiveLoc pawn.GetLord() != null"));
                    }
                    else
                    {
                        //   Log.Message(string.Format("TryFindGoodHiveLoc pawn.GetLord() == null"));
                    }
                    if (pawn.mindState.duty.def != XenomorphDefOf.RRY_Xenomorph_DefendAndExpandHive && pawn.mindState.duty.def != XenomorphDefOf.RRY_Xenomorph_DefendHiveAggressively)
                    {
                        pawn.mindState.duty = new PawnDuty(XenomorphDefOf.RRY_Xenomorph_DefendAndExpandHive, c, 40f);
                    }
                    if (RCellFinder.TryFindRandomCellNearWith(c, validator, pawn.Map, out IntVec3 lc, 2, 8))
                    {
                        return new Job(XenomorphDefOf.RRY_Job_Xenomorph_Kidnap)
                        {
                            targetA = Victim,
                            targetB = lc,
                            targetC = lc.RandomAdjacentCell8Way(),
                            count = 1
                        };
                    }
                }
                else
                {
                    // No suitable hive location found
                }
            }
            else
            {
                // No suitable Victim found
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
        
        public const float VictimSearchRadiusInitial = 8f;

        private const float VictimSearchRadiusOngoing = 18f;

        protected bool canBash = false;

        protected bool forceCanDig = true;

        protected bool forceRoofed = false;
        protected bool allowHosts = false;
        protected bool allowCocooned = false;
        protected int minRadius = 0;
        protected bool forceCanDigIfAnyHostileActiveThreat;

        protected bool forceCanDigIfCantReachMapEdge = true;
    }
}
