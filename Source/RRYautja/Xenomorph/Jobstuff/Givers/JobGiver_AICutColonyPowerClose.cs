using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x020000C1 RID: 193
    public class JobGiver_AICutColonyPowerClose : ThinkNode_JobGiver 
    {
        // Token: 0x0600048F RID: 1167 RVA: 0x0002F174 File Offset: 0x0002D574
        protected override Job TryGiveJob(Pawn pawn)
        {
            Map map = pawn?.Map;
            if (map == null) return null;
            if (!pawn.HostileTo(Faction.OfPlayer))
            {
                return null;
            }
            bool flag = pawn.HostileTo(Faction.OfPlayer);
            CellRect cellRect = CellRect.CenteredOn(pawn.Position, 5);
            List<Building> list = map.listerBuildings.allBuildingsColonist.FindAll(x => x.TryGetComp<CompPowerPlant>() != null || x.TryGetComp<CompPowerBattery>() != null);
            for (int i = 0; i < 35; i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                if (randomCell.InBounds(pawn.Map))
                {
                    Building edifice = randomCell.GetEdifice(pawn.Map);
                    if (flag)
                    {
                        if (edifice != null && TrashUtility.ShouldTrashBuilding(pawn, edifice, false) && GenSight.LineOfSight(pawn.Position, randomCell, pawn.Map, false, null, 0, 0) && (edifice.TryGetComp<CompPowerPlant>() != null || edifice.TryGetComp<CompPowerBattery>() != null || edifice.TryGetComp<CompPowerTransmitter>() != null))
                        {
                            if (DebugViewSettings.drawDestSearch && Find.CurrentMap == pawn.Map)
                            {
                                Find.CurrentMap.debugDrawer.FlashCell(randomCell, 1f, "trash bld", 50);
                            }
                            Job job;

                            Building building = edifice as Building;
                            bool flagb = building != null && building.def.building.isInert;
                            if (flagb)
                            {
                                return null;
                            }
                            job = new Job(JobDefOf.AttackMelee, edifice);
                            if (job != null)
                            {
                                return job;
                            }
                        }
                    }
                    if (DebugViewSettings.drawDestSearch && Find.CurrentMap == pawn.Map)
                    {
                        Find.CurrentMap.debugDrawer.FlashCell(randomCell, 0f, "trash no", 50);
                    }
                }
            }
            return null;
        }

        // Token: 0x0400029B RID: 667
        private const int CloseSearchRadius = 5;
    }
}
