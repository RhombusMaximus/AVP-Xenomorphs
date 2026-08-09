using RRYautja;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x020000D6 RID: 214
    public class JobGiver_Xenomorph_Cocoon : ThinkNode_JobGiver
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
            JobGiver_Xenomorph_Cocoon jobGiver_XenosKidnap = (JobGiver_Xenomorph_Cocoon)base.DeepCopy(resolve);
            jobGiver_XenosKidnap.MaxRange = this.MaxRange;
            jobGiver_XenosKidnap.MinRange = this.MinRange;
            jobGiver_XenosKidnap.forceRoofed = this.forceRoofed;
            jobGiver_XenosKidnap.allowCocooned = this.allowCocooned;
            jobGiver_XenosKidnap.canBash = this.canBash;
            jobGiver_XenosKidnap.forceCanDig = this.forceCanDig;
            jobGiver_XenosKidnap.forceCanDigIfAnyHostileActiveThreat = this.forceCanDigIfAnyHostileActiveThreat;
            jobGiver_XenosKidnap.forceCanDigIfCantReachMapEdge = this.forceCanDigIfCantReachMapEdge;
            return jobGiver_XenosKidnap;
        }


        private float MaxRange = 9999f;
        private float MinRange = 0f;

        // Token: 0x060004D1 RID: 1233 RVA: 0x0003100C File Offset: 0x0002F40C
        protected override Job TryGiveJob(Pawn pawn)
        {
            float Searchradius = MaxRange;
            IntVec3 c = IntVec3.Invalid;
            if (Searchradius==0)
            {
                if (pawn.mindState.duty!=null)
                {
                    if (pawn.mindState.duty.focus.Cell.GetThingList(pawn.Map).Any(x => x.def == XenomorphDefOf.RRY_Xenomorph_Hive))
                    {
                        Searchradius = 7f;
                    }
                    if (pawn.mindState.duty.focus.Cell.GetThingList(pawn.Map).Any(x => x.def == XenomorphDefOf.RRY_Xenomorph_Hive_Child))
                    {
                        Searchradius = 3f;
                    }
                    if (pawn.mindState.duty.focus.Cell.GetThingList(pawn.Map).Any(x => x.def == XenomorphDefOf.RRY_Xenomorph_Hive_Slime))
                    {
                        Searchradius = 5f;
                    }
                }
            }
            if (XenomorphKidnapUtility.TryFindGoodKidnapVictim(pawn, Searchradius, out Pawn t, null, this.forceRoofed, true, MinRange) && !GenAI.InDangerousCombat(pawn))
            {
                if (XenomorphKidnapUtility.TryFindGoodHiveLoc(pawn, out c, t, true, !this.forceRoofed, this.forceCanDig))
                {
                    ThingDef namedA = XenomorphDefOf.RRY_Xenomorph_Cocoon_Humanoid;
                    ThingDef namedB = XenomorphDefOf.RRY_Xenomorph_Cocoon_Animal;
                    bool selected = pawn.Map != null ? Find.Selector.SelectedObjects.Contains(pawn) && (Prefs.DevMode) : false;
                    if (c != IntVec3.Invalid && t != null && pawn.CanReach(c, PathEndMode.ClosestTouch, Danger.Deadly, true, false, TraverseMode.PassAllDestroyableThings))
                    {
                        Predicate<IntVec3> validator = delegate (IntVec3 y)
                        {
                            bool roofed = (y.Roofed(pawn.Map) && this.forceRoofed) || !this.forceRoofed;
                            bool adjacent = c.AdjacentTo8WayOrInside(y);
                            bool filled = y.Filled(pawn.Map);
                            bool edifice = y.GetEdifice(pawn.Map).DestroyedOrNull();
                            bool building = y.GetFirstBuilding(pawn.Map).DestroyedOrNull();
                            bool thingA = y.GetFirstThing(pawn.Map, namedA).DestroyedOrNull();
                            bool thingB = y.GetFirstThing(pawn.Map, namedB).DestroyedOrNull();
                            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0}, adjacent: {1}, filled: {2}, edifice: {3}, building: {4}", y, !adjacent, !filled, edifice, building));
                            return !adjacent && !filled && edifice && building && thingA && thingB && roofed;
                        };
                        if (pawn.GetLord() != null && pawn.GetLord() is Lord lord)
                        {
                            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("TryFindGoodHiveLoc pawn.GetLord() != null"));
                        }
                        else
                        {
                            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("TryFindGoodHiveLoc pawn.GetLord() == null"));
                        }
                        if (pawn.mindState.duty.def != XenomorphDefOf.RRY_Xenomorph_DefendAndExpandHive && pawn.mindState.duty.def != XenomorphDefOf.RRY_Xenomorph_DefendHiveAggressively)
                        {
                            pawn.mindState.duty = new PawnDuty(XenomorphDefOf.RRY_Xenomorph_DefendAndExpandHive, c, 40f);
                        }
                        if (RCellFinder.TryFindRandomCellNearWith(c, validator, pawn.Map, out IntVec3 lc, 2, 6))
                        {
                            return new Job(XenomorphDefOf.RRY_Job_Xenomorph_Kidnap)
                            {
                                targetA = t,
                                targetB = lc,
                                count = 1
                            };
                        }
                    }
                    else
                    {
                        if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} something went wrong", this));
                    }
                }
                else
                {
                    if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} No Cocooning spot Found", this));
                }
            }
            else
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} No Victim Found", this));
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
        protected bool allowCocooned = false;
        protected bool forceCanDig = true;
        protected bool forceRoofed = false;
        protected bool forceCanDigIfAnyHostileActiveThreat;
        protected bool forceCanDigIfCantReachMapEdge = true;
    }
}
