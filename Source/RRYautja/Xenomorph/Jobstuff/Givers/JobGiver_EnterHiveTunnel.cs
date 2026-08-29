using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Linq;

namespace RRYautja
{
    // RRYautja.JobGiver_EnterHiveTunnel
    public class JobGiver_EnterHiveTunnel : ThinkNode_JobGiver
    {
        // Token: 0x060004C5 RID: 1221 RVA: 0x00030B6C File Offset: 0x0002EF6C
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null) return null;
            MapComponent_HiveGrid _HiveGrid = pawn.Map.GetComponent<MapComponent_HiveGrid>();
            HiveLike Tunnel = null;
            Tunnel = JobGiver_EnterHiveTunnel.FindMyTunnel(pawn);
            if (Tunnel == null)
            {
                return null;
            }
            if (Tunnel.HitPoints==0)
            {
                return null;
            }
            bool flag1 = Tunnel.def == XenomorphDefOf.RRY_Xenomorph_Hive;
            bool flag2 = Tunnel.def == XenomorphDefOf.RRY_Xenomorph_Hive_Child;
            bool flag3 = Tunnel.Map.mapPawns.AllPawnsSpawned.Any(x => x.isPotentialHost() && pawn.TryGetComp<Comp_Xenomorph>().IsAcceptablePreyFor(pawn, x, true));
            bool flag4 = _HiveGrid.HiveGuardlist.Contains(pawn) || (Tunnel.hiveMaintainer!=null && Tunnel.hiveMaintainer.CurStage != MaintainableStage.Healthy && (_HiveGrid.HiveWorkerlist.Contains(pawn) || _HiveGrid.HiveWorkerlist.NullOrEmpty()));

            // If pawn is already inside the tunnel, don't re-enter (prevents in-out loop)
            if (Tunnel.spawnedPawns.Contains(pawn))
            {
                return null;
            }
            if (flag3 || flag4 || Tunnel == null || !pawn.CanReach(Tunnel, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return null;
            }
            if (Tunnel.hiveDormant && !_HiveGrid.HiveGuardlist.Contains(pawn))
            {
                return new Job(XenomorphDefOf.RRY_Job_Xenomorph_EnterHiveTunnel, Tunnel);
            }
            return new Job(XenomorphDefOf.RRY_Job_Xenomorph_EnterHiveTunnel, Tunnel);
        }

        // Token: 0x060004C6 RID: 1222 RVA: 0x00030C6C File Offset: 0x0002F06C
        public static HiveLike FindMyTunnel(Pawn pawn)
        {
            if (pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen)
            {
                if (XenomorphUtil.HivelikesPresent(pawn.Map))
                {
                    HiveLike hive = (HiveLike)XenomorphUtil.ClosestReachableHivelike(pawn);
                    if (hive != null && !hive.hasQueen)
                    {
                        if (hive.Lord!=null && hive.Lord!=pawn.GetLord())
                        {
                            pawn.SwitchToLord(hive.Lord);
                        }
                        return hive;
                    }
                }
            }
            if (pawn.GetLord() != null && pawn.GetLord() is Lord lord)
            {
                if (lord.LordJob is LordJob_DefendAndExpandHiveLike hivejob)
                {
                    if (lord.CurLordToil is LordToil_DefendAndExpandHiveLike hivetoil)
                    {
                        if (hivetoil.Data.assignedHiveLikes.TryGetValue(pawn) != null)
                        {
                            return hivetoil.Data.assignedHiveLikes.TryGetValue(pawn);
                        }
                    }
                }
                else if (lord.CurLordToil is LordToil_DefendHiveLikeAggressively hivetoilA)
                {
                    if (hivetoilA.Data.assignedHiveLikes.TryGetValue(pawn) != null)
                    {
                        return hivetoilA.Data.assignedHiveLikes.TryGetValue(pawn);
                    }
                }
                else
                {
                    if (XenomorphUtil.HivelikesPresent(pawn.Map))
                    {
                        HiveLike hive = (HiveLike)XenomorphUtil.ClosestReachableHivelike(pawn);
                        if (hive.Lord != null && hive.Lord != pawn.GetLord())
                        {
                            pawn.SwitchToLord(hive.Lord);
                        }
                        return (HiveLike)XenomorphUtil.ClosestReachableHivelike(pawn);
                    }
                }
            }
            return null;
        }
    }
}
