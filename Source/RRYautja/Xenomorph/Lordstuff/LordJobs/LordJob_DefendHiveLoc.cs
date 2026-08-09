using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x02000170 RID: 368
    public class LordJob_DefendHiveLoc : LordJob
    {
        // Token: 0x0600079B RID: 1947 RVA: 0x0004321B File Offset: 0x0004161B
        public LordJob_DefendHiveLoc()
        {
        }

        // Token: 0x0600079C RID: 1948 RVA: 0x00043223 File Offset: 0x00041623
        public LordJob_DefendHiveLoc(Faction faction, IntVec3 baseCenter)
        {
            this.faction = faction;
            this.baseCenter = baseCenter;
        }

        // Token: 0x0600079D RID: 1949 RVA: 0x0004323C File Offset: 0x0004163C
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendHiveLoc lordToil_DefendBase = new LordToil_DefendHiveLoc(this.baseCenter);
            stateGraph.StartingToil = lordToil_DefendBase;
            LordToil_DefendHiveLoc lordToil_DefendBase2 = new LordToil_DefendHiveLoc(this.baseCenter);
            stateGraph.AddToil(lordToil_DefendBase2);
            LordToil_XenomrophAssaultColony lordToil_AssaultColony = new LordToil_XenomrophAssaultColony(true);
            lordToil_AssaultColony.useAvoidGrid = true;
            stateGraph.AddToil(lordToil_AssaultColony);
            Transition transition = new Transition(lordToil_DefendBase, lordToil_DefendBase2, false, true);
            transition.AddSource(lordToil_AssaultColony);
            transition.AddTrigger(new Trigger_BecameNonHostileToPlayer());
            stateGraph.AddTransition(transition, false);
            Transition transition2 = new Transition(lordToil_DefendBase2, lordToil_DefendBase, false, true);
            transition2.AddTrigger(new Trigger_BecamePlayerEnemy());
            stateGraph.AddTransition(transition2, false);
            Transition transition3 = new Transition(lordToil_DefendBase, lordToil_AssaultColony, false, true);
            transition3.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
            transition3.AddTrigger(new Trigger_PawnHarmed(0.4f, true)); //, null));
            //    transition3.AddTrigger(new Trigger_ChanceOnTickInterval(2500, 0.03f));
            //    transition3.AddTrigger(new Trigger_TicksPassed(251999));
            transition3.AddTrigger(new Trigger_UrgentlyHungry());
            transition3.AddTrigger(new Trigger_ChanceOnPlayerHarmNPCBuilding(0.4f));
            transition3.AddPostAction(new TransitionAction_WakeAll());
            string message = "MessageDefendersAttacking".Translate(this.faction.def.pawnsPlural, this.faction.Name, Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
            transition3.AddPreAction(new TransitionAction_Message(message, MessageTypeDefOf.ThreatBig, null, 1f));
            stateGraph.AddTransition(transition3, false);

            Transition transition4 = new Transition(lordToil_AssaultColony, lordToil_DefendBase2, false, true);
                transition4.AddTrigger(new Trigger_ChanceOnTickInterval(2500, 0.03f));
                transition4.AddTrigger(new Trigger_TicksPassedWithoutHarm(9000));
            transition4.AddPostAction(new TransitionAction_EndAllJobs());
            string message2 = "MessageDefendersAttacking".Translate(this.faction.def.pawnsPlural, this.faction.Name, Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
            transition4.AddPreAction(new TransitionAction_Message(message2, MessageTypeDefOf.NeutralEvent, null, 1f));
            stateGraph.AddTransition(transition4, false);
            return stateGraph;
        }

        // Token: 0x0600079E RID: 1950 RVA: 0x000433C0 File Offset: 0x000417C0
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Faction>(ref this.faction, "faction", false);
            Scribe_Values.Look<IntVec3>(ref this.baseCenter, "baseCenter", default(IntVec3), false);
        }

        // Token: 0x04000348 RID: 840
        private Faction faction;

        // Token: 0x04000349 RID: 841
        private IntVec3 baseCenter;
    }
}
