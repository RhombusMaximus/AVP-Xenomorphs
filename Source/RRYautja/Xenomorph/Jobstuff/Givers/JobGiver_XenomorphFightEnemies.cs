using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x020000B4 RID: 180
    public class JobGiver_XenomorphFightEnemies : JobGiver_AIFightEnemy
    {
        // Token: 0x06000458 RID: 1112 RVA: 0x0002C7FC File Offset: 0x0002ABFC
        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            int forceMeleeRange = 7;
            bool allowManualCastWeapons = !pawn.IsColonist;
            bool canReach = (pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly, false));
            bool meleeRange = (pawn.Position.InHorDistOf(enemyTarget.Position, forceMeleeRange));
            Pawn tPawn = enemyTarget as Pawn;
            bool targetPawn = (tPawn != null && tPawn.Awake())|| tPawn==null;
            bool allowRanged = (Rand.Chance(0.25f) && targetPawn && (canReach && !meleeRange)) || !canReach;
            Verb verb = allowRanged ? pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons) : pawn.meleeVerbs.TryGetMeleeVerb(enemyTarget);
            if (verb == null)
            {
            //    Log.Message(string.Format("JobGiver_XenomorphFightEnemies Allow ranged: {0}, Verb: {1}", verb, allowRanged));
                dest = IntVec3.Invalid;
                return false;
            }
            if (allowRanged)
            {
                return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
                {
                    caster = pawn,
                    target = enemyTarget,
                    verb = verb,
                    maxRangeFromTarget = verb.verbProps.range,
                    wantCoverFromTarget = true
                }, out dest);
            }
            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = verb,
                maxRangeFromTarget = 1,
                wantCoverFromTarget = false
            }, out dest);
        }
    }
}
