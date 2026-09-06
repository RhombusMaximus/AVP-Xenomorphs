using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    /// <summary>
    /// Custom LordJob for Xenomorph raids that uses the RRY_Xenomorph_AssaultColony duty
    /// (kidnap-first, no building trash) instead of vanilla LordJob_AssaultColony which
    /// includes JobGiver_AITrashColonyClose and JobGiver_AITrashBuildingsDistant.
    ///
    /// The state graph mirrors LordJob_AssaultColony_CutPower but swaps the assault toil
    /// for LordToil_XenomrophAssaultColony (sets duty RRY_Xenomorph_AssaultColony).
    /// Xenomorphs are not humanlike, so the kidnap/steal/timeout subgraphs from the
    /// vanilla assault are omitted — the duty itself handles kidnap-first behaviour.
    /// </summary>
    public class LordJob_XenomorphAssaultColony : LordJob
    {
        private Faction assaulterFaction;
        private bool canTimeoutOrFlee;
        private bool useAvoidGridSmart;

        public LordJob_XenomorphAssaultColony()
        {
        }

        public LordJob_XenomorphAssaultColony(Faction assaulterFaction, bool canTimeoutOrFlee = true, bool useAvoidGridSmart = false)
        {
            this.assaulterFaction = assaulterFaction;
            this.canTimeoutOrFlee = canTimeoutOrFlee;
            this.useAvoidGridSmart = useAvoidGridSmart;
        }

        public override bool GuiltyOnDowned
        {
            get
            {
                return true;
            }
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            // Single assault toil that assigns the kidnap-first RRY_Xenomorph_AssaultColony duty.
            LordToil lordToil = new LordToil_XenomrophAssaultColony(false);
            if (this.useAvoidGridSmart)
            {
                lordToil.useAvoidGrid = true;
            }
            stateGraph.AddToil(lordToil);

            // Exit toil for when the raid is over (flee/timeout/became non-hostile).
            LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, false);
            lordToil_ExitMap.useAvoidGrid = true;
            stateGraph.AddToil(lordToil_ExitMap);

            if (this.canTimeoutOrFlee && this.assaulterFaction != null && this.assaulterFaction.def.humanlikeFaction)
            {
                // Timeout: give up after a long assault and leave.
                Transition transitionTimeout = new Transition(lordToil, lordToil_ExitMap, false, true);
                transitionTimeout.AddTrigger(new Trigger_TicksPassed(AssaultTimeBeforeGiveUp.RandomInRange));
                transitionTimeout.AddPreAction(new TransitionAction_Message(
                    "MessageRaidersGivenUpLeaving".Translate(this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(), this.assaulterFaction.Name),
                    null, 1f));
                stateGraph.AddTransition(transitionTimeout, false);

                // Flee when enough colony damage has been taken.
                Transition transitionFlee = new Transition(lordToil, lordToil_ExitMap, false, true);
                FloatRange floatRange = new FloatRange(0.25f, 0.35f);
                float randomInRange = floatRange.RandomInRange;
                transitionFlee.AddTrigger(new Trigger_FractionColonyDamageTaken(randomInRange, 900f));
                transitionFlee.AddPreAction(new TransitionAction_Message(
                    "MessageRaidersSatisfiedLeaving".Translate(this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(), this.assaulterFaction.Name),
                    null, 1f));
                stateGraph.AddTransition(transitionFlee, false);
            }

            // Leave if the faction becomes non-hostile to the player.
            Transition transitionNonHostile = new Transition(lordToil, lordToil_ExitMap, false, true);
            transitionNonHostile.AddTrigger(new Trigger_BecameNonHostileToPlayer());
            if (this.assaulterFaction != null)
            {
                transitionNonHostile.AddPreAction(new TransitionAction_Message(
                    "MessageRaidersLeaving".Translate(this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(), this.assaulterFaction.Name),
                    null, 1f));
            }
            stateGraph.AddTransition(transitionNonHostile, false);

            // Re-trigger assault duty assignment when a pawn is lost or harmed (self-loop).
            Transition transitionReTrigger = new Transition(lordToil, lordToil, true, true);
            transitionReTrigger.AddTrigger(new Trigger_PawnLost());
            stateGraph.AddTransition(transitionReTrigger, false);

            Transition transitionHarmed = new Transition(lordToil, lordToil, true, false);
            transitionHarmed.AddTrigger(new Trigger_PawnHarmed(1f, false, null));
            transitionHarmed.AddPostAction(new TransitionAction_CheckForJobOverride());
            stateGraph.AddTransition(transitionHarmed, false);

            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Faction>(ref this.assaulterFaction, "assaulterFaction", false);
            Scribe_Values.Look<bool>(ref this.canTimeoutOrFlee, "canTimeoutOrFlee", true, false);
            Scribe_Values.Look<bool>(ref this.useAvoidGridSmart, "useAvoidGridSmart", false, false);
        }

        private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);
    }
}