using RRYautja;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x02000330 RID: 816
	public class IncidentWorker_PowerCut : IncidentWorker
	{
		// Token: 0x06000E1B RID: 3611 RVA: 0x00069AE0 File Offset: 0x00067EE0
		protected override bool CanFireNowSub(IncidentParms parms)
		{
            Map map = (Map)parms.target;
            if (map.listerBuildings.allBuildingsColonist.Any(x=> x.TryGetComp<CompPowerPlant>()!=null))
            {
                IntVec3 intVec;
                Faction faction = Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph);
                return this.TryFindSpawnSpot(map, out intVec) && faction != null;
            }
            return false;
		}

		// Token: 0x06000E1C RID: 3612 RVA: 0x00069B78 File Offset: 0x00067F78
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
            Map map = (Map)parms.target;
            IntVec3 spawnSpot;
            if (!this.TryFindSpawnSpot(map, out spawnSpot))
            {
                return false;
            }
            int @int = Rand.Int;
            IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            raidParms.forced = true;
            raidParms.faction = parms.faction;
            raidParms.raidStrategy = XenomorphDefOf.RRY_PowerCut;
            raidParms.raidArrivalMode = XenomorphDefOf.RRY_DropThroughRoofNearPower;
            raidParms.spawnCenter = spawnSpot;
            raidParms.generateFightersOnly = true;
            raidParms.points = Mathf.Max((raidParms.points / 5) * IncidentWorker_PowerCut.RaidPointsFactorRange.RandomInRange, 500f);
            raidParms.pawnGroupMakerSeed = new int?(@int);
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms, true);
            defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, XenomorphDefOf.RRY_DropThroughRoofNearPower, XenomorphDefOf.RRY_PowerCut, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, raidParms.target);
            IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);

            base.SendStandardLetter(parms, null);
            QueuedIncident qi = new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, raidParms), 0, 0);
            Find.Storyteller.incidentQueue.Add(qi);
            return true;
        }
        
        // Token: 0x06000E92 RID: 3730 RVA: 0x0006CD54 File Offset: 0x0006B154
        private bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            Predicate<IntVec3> validator = (IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        // Token: 0x06000E93 RID: 3731 RVA: 0x0006CD8D File Offset: 0x0006B18D
        private bool TryFindEnemyFaction(out Faction enemyFac)
        {
            return (from f in Find.FactionManager.AllFactions
                    where !f.def.hidden && !f.defeated && f.HostileTo(Faction.OfPlayer)
                    select f).TryRandomElement(out enemyFac);
        }

        // Token: 0x04000949 RID: 2377
        private static readonly IntRange RaidDelay = new IntRange(900, 1500);

        // Token: 0x0400094A RID: 2378
        private static readonly FloatRange RaidPointsFactorRange = new FloatRange(1f, 1.6f);
    }
    // Token: 0x02000330 RID: 816
    
}
