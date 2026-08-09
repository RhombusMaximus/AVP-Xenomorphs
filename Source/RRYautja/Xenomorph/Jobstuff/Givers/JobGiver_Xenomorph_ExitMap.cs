using System;
using System.Linq;
using RimWorld;

namespace Verse.AI
{
	// Token: 0x02000A2F RID: 2607
	public abstract class JobGiver_Xenomorph_ExitMap : ThinkNode_JobGiver
	{
		// Token: 0x06003DA2 RID: 15778 RVA: 0x0016D5CC File Offset: 0x0016B7CC
		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Xenomorph_ExitMap jobGiver_ExitMap = (JobGiver_Xenomorph_ExitMap)base.DeepCopy(resolve);
			jobGiver_ExitMap.defaultLocomotion = this.defaultLocomotion;
			jobGiver_ExitMap.jobMaxDuration = this.jobMaxDuration;
            // TODO: canBash removed from Job in 1.6
            // jobGiver_ExitMap.canBash = this.canBash;
			jobGiver_ExitMap.forceCanDig = this.forceCanDig;
			jobGiver_ExitMap.forceCanDigIfAnyHostileActiveThreat = this.forceCanDigIfAnyHostileActiveThreat;
			jobGiver_ExitMap.forceCanDigIfCantReachMapEdge = this.forceCanDigIfCantReachMapEdge;
			jobGiver_ExitMap.failIfCantJoinOrCreateCaravan = this.failIfCantJoinOrCreateCaravan;
			return jobGiver_ExitMap;
		}

		// Token: 0x06003DA3 RID: 15779 RVA: 0x0016D63C File Offset: 0x0016B83C
		protected override Job TryGiveJob(Pawn pawn)
		{
			Room room = pawn.GetRoom(RegionType.Set_Passable);
			if (room.PsychologicallyOutdoors && room.TouchesMapEdge)
			{
				return null;
			}
			if (!pawn.CanReachMapEdge())
			{
				return null;
			}
			bool flag = this.forceCanDig || (pawn.mindState.duty != null && pawn.mindState.duty.canDig && !pawn.CanReachMapEdge()) || (this.forceCanDigIfCantReachMapEdge && !pawn.CanReachMapEdge()) || (this.forceCanDigIfAnyHostileActiveThreat && pawn.Faction != null && GenHostility.AnyHostileActiveThreatTo(pawn.Map, pawn.Faction, true));
			IntVec3 c;
			if (!this.TryFindGoodExitDest(pawn, flag, out c))
			{
				return null;
			}
			if (flag)
			{
				using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, c, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false), null, PathEndMode.OnCell))
				{
					IntVec3 cellBeforeBlocker;
					Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
					if (thing.def == XenomorphDefOf.RRY_Xenomorph_Hive_Wall)
					{
						CellRect rect = new CellRect(thing.Position.x - 1, thing.Position.y - 1, 3,3);
						foreach (IntVec3 cell in rect)
						{
							if (cell.InBounds(thing.Map))
							{
								if (cell.GetThingList(thing.Map).All(x=> x.def!= XenomorphDefOf.RRY_Xenomorph_Hive_Wall))
								{
									thing = cell.GetFirstMineable(thing.Map);
									break;
								}
							}
						}

					}
					if (thing != null && thing.def!=XenomorphDefOf.RRY_Xenomorph_Hive_Wall)
					{
						Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true, true);
						if (job != null)
						{
							return job;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06003DA4 RID: 15780
		protected abstract bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 dest);

		// Token: 0x04002A78 RID: 10872
		protected LocomotionUrgency defaultLocomotion;

		// Token: 0x04002A79 RID: 10873
		protected int jobMaxDuration = 999999;

		// Token: 0x04002A7A RID: 10874
		protected bool canBash;

		// Token: 0x04002A7B RID: 10875
		protected bool forceCanDig;

		// Token: 0x04002A7C RID: 10876
		protected bool forceCanDigIfAnyHostileActiveThreat;

		// Token: 0x04002A7D RID: 10877
		protected bool forceCanDigIfCantReachMapEdge;

		// Token: 0x04002A7E RID: 10878
		protected bool failIfCantJoinOrCreateCaravan;
	}
}
