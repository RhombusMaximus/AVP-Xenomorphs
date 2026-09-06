using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
    /// <summary>
    /// Custom RaidStrategyWorker that creates a LordJob_XenomorphAssaultColony instead
    /// of the vanilla LordJob_AssaultColony. The custom LordJob uses the
    /// RRY_Xenomorph_AssaultColony duty (kidnap-first, no building trash).
    /// This mirrors RaidStrategyWorker_ImmediateAttackSmart_CutPower which overrides
    /// MakeLordJob to return a custom LordJob.
    /// </summary>
    public class RaidStrategyWorker_XenomorphImmediateAttack : RaidStrategyWorker_ImmediateAttack
    {
        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            return new LordJob_XenomorphAssaultColony(parms.faction, true, false);
        }
    }
}