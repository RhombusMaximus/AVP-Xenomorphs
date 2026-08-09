using RRYautja;
using RRYautja.ExtensionMethods;
using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x020000A2 RID: 162
	public class JobGiver_HiveLikeDefense : JobGiver_XenomorphFightEnemies
	{
		// Token: 0x06000416 RID: 1046 RVA: 0x0002C898 File Offset: 0x0002AC98
		protected override IntVec3 GetFlagPosition(Pawn pawn)
		{
            if (pawn.mindState.duty.focus.Thing is HiveLike hivelike && hivelike.Spawned)
            {
                return hivelike.Position;
            }
			else
			{
				if (pawn.isXenomorph(out Comp_Xenomorph xenomorph))
				{
					return xenomorph.HiveLoc;
				}
			}
            return pawn.Position;
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0002C8DE File Offset: 0x0002ACDE
		protected override float GetFlagRadius(Pawn pawn)
		{
			return pawn.mindState.duty.radius;
		}

		protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
		{
			bool result = base.ExtraTargetValidator(pawn, target);
			string reason = "ExtraTargetValidator: base";
			string tar = string.Empty;
			Pawn pawn1 = target as Pawn;
			if (pawn1 != null)
			{
				if (!pawn1.isXenomorph())
				{
					if (!pawn1.isXenoHost())
					{
						if (!pawn1.isPotentialHost(out string failreason))
						{
							if (pawn1.Position.InHorDistOf(GetFlagPosition(pawn), 12) && pawn1.AnimalOrWildMan() && !pawn1.isXenoHost() && !pawn1.isXenomorph())
							{
								reason = string.Format("ExtraTargetValidator: {0} AnimalOrWildMan within 12 of {1}", pawn1, GetFlagPosition(pawn));
								result = true;
							}
							if (pawn1.LastAttackedTarget.HasThing)
							{
								Pawn pawn2 = pawn1.LastAttackedTarget.Thing as Pawn;
								if (pawn2 != null)
								{
									int limit = pawn1.isXenoHost() ? 3000 : 15000;
									if ((pawn2.isXenomorph() || pawn2.isXenoHost()) && Find.TickManager.TicksGame - pawn1.LastAttackTargetTick < limit)
									{
										reason += string.Format(": {0} Attacked {1} recently, Host: {2}", pawn1, pawn2, pawn1.isXenoHost());
										result = base.ExtraTargetValidator(pawn, target);
									}
									else
									{
										reason += string.Format(": {0} Hasnt Attacked Any Xenomorph recently, Host: {1}", pawn1, pawn1.isXenoHost());
										result = false;
									}
								}
							}
							if (!pawn1.LastAttackedTarget.HasThing)
							{
								reason += string.Format(": {0} Hasnt Attacked Any Xenomorph recently, Host: {1}", pawn1, pawn1.isXenoHost());
								result = false;
							}
							reason += " Fail reason:" + failreason;
						}
					}
				}
				else
				{
					reason += " Fail reason: isXenomorph";
					result = false;
				}
				tar = "Pawn " + pawn1;
			}
			else
			{
				tar = "Non-Pawn " + pawn1;
			}

			if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0}: {1} Vs {2}: {3}: {4}", this, pawn, tar, result, reason));
			return result;
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0002C8F0 File Offset: 0x0002ACF0
		protected override Job MeleeAttackJob(Thing enemyTarget)
		{
			Job job = base.MeleeAttackJob(enemyTarget);
			job.attackDoorIfTargetLost = true;
            job.canBash = true;
            
			return job;
		}
	}
}
