using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using RRYautja.settings;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class AvP_IncidentWorker_RaidEnemy_TryExecute_PowerCut_Patch
    {
        public static void Postfix(IncidentWorker_RaidEnemy __instance, ref IncidentParms parms, bool __result)
        {
            if (__result)
            {
                if (parms.faction.def == XenomorphDefOf.RRY_Xenomorph && parms.raidStrategy != XenomorphDefOf.RRY_PowerCut)
                {

                    Map map = (Map)parms.target;
                    if (map.listerBuildings.allBuildingsColonist.Any(x => x.TryGetComp<CompPowerPlant>() != null))
                    {
                        Rand.PushState();
                        int @int = Rand.Int;
                        bool chance = Rand.ChanceSeeded(0.1f,@int);
                        Rand.PopState();
                        if (AvP_IncidentWorker_RaidEnemy_TryExecute_PowerCut_Patch.TryFindSpawnSpot(map, out IntVec3 intVec) && chance)
                        {
                            IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                            raidParms.forced = true;
                            raidParms.faction = parms.faction;
                            raidParms.raidStrategy = XenomorphDefOf.RRY_PowerCut;
                            raidParms.raidArrivalMode = XenomorphDefOf.RRY_DropThroughRoofNearPower;
                            raidParms.spawnCenter = intVec;
                            raidParms.generateFightersOnly = true;
                            Rand.PushState();
                            raidParms.points = Mathf.Max((raidParms.points / 5) * AvP_IncidentWorker_RaidEnemy_TryExecute_PowerCut_Patch.RaidPointsFactorRange.RandomInRange, 500f);
                            Rand.PopState();
                            raidParms.pawnGroupMakerSeed = new int?(@int);
                            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms, true);
                            defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, XenomorphDefOf.RRY_DropThroughRoofNearPower, XenomorphDefOf.RRY_PowerCut, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, raidParms.target);
                            IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);

                            QueuedIncident qi = new QueuedIncident(new FiringIncident(XenomorphDefOf.RRY_PowerCut_Xenomorph, null, raidParms), Find.TickManager.TicksGame + AvP_IncidentWorker_RaidEnemy_TryExecute_PowerCut_Patch.RaidDelay.RandomInRange, 0);
                            Find.Storyteller.incidentQueue.Add(qi);
                        }
                    }
                }
            }
        }
        // Token: 0x06000E92 RID: 3730 RVA: 0x0006CD54 File Offset: 0x0006B154
        private static bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            Predicate<IntVec3> validator = (IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        // Token: 0x04000949 RID: 2377
        private static readonly IntRange RaidDelay = new IntRange(900, 1500);

        // Token: 0x0400094A RID: 2378
        private static readonly FloatRange RaidPointsFactorRange = new FloatRange(1f, 1.6f);
    }
}