using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
    // Token: 0x02000356 RID: 854
    public class IncidentWorker_RaidEnemy_FactionSpecific : IncidentWorker_RaidEnemy
    {
        public IncidentDef_FactionSpecific Def => (IncidentDef_FactionSpecific)this.def;
        // Token: 0x06000ECD RID: 3789 RVA: 0x0006EC50 File Offset: 0x0006D050
        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (this.Def.Faction != null)
            {
                if (Find.FactionManager.FirstFactionOfDef(this.Def.Faction) != null)
                {
                    parms.faction = Find.FactionManager.FirstFactionOfDef(this.Def.Faction);
                }
            }
            if (!this.Def.Factions.NullOrEmpty())
            {
                for (int i = 0; i < this.Def.Factions.Count*10; i++)
                {
                    Faction faction = Find.FactionManager.AllFactionsListForReading.FindAll(x=> this.Def.Factions.Any(y=> x.def==y && x.HostileTo(Faction.OfPlayer))).RandomElementByWeight(z=> z.def.RaidCommonalityFromPoints(parms.points));
                    if (faction != null && faction.HostileTo(Faction.OfPlayer))
                    {
                        parms.faction = faction;
                        break;
                    }
                }
            }
            if (parms.faction != null)
            {
                if (!parms.faction.HostileTo(Faction.OfPlayer))
                {
                    return false;
                }
                return true;
            }
            float num = parms.points;
            if (num <= 0f)
            {
                num = 999999f;
            }
            return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, map, false), true, true, true, true) || PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, map, true), true, true, true, true);
        }

        // Token: 0x06000ECE RID: 3790 RVA: 0x0006ECE7 File Offset: 0x0006D0E7
        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        // Token: 0x06000ECF RID: 3791 RVA: 0x0006ED18 File Offset: 0x0006D118
        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (Def.raidStrategyDefDef != null)
            {
                parms.raidStrategy = Def.raidStrategyDefDef;
            }
            else if (!Def.raidStrategyDefDefs.NullOrEmpty())
            {
                parms.raidStrategy = Def.raidStrategyDefDefs.RandomElement();
            }
            if (parms.raidStrategy != null)
            {
                return;
            }
            Map map = (Map)parms.target;
            if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs
                  where d.Worker.CanUseWith(parms, groupKind) && (parms.raidArrivalMode != null || (d.arriveModes != null && d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms))))
                  select d).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeight(map, parms.points), out parms.raidStrategy))
            {
                Log.Error(string.Concat(new object[]
                {
                    "No raid stategy for ",
                    parms.faction,
                    " with points ",
                    parms.points,
                    ", groupKind=",
                    groupKind,
                    "\nparms=",
                    parms
                }));
                if (!Prefs.DevMode)
                {
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                }
            }
        }

        public override void ResolveRaidArriveMode(IncidentParms parms)
        {
            if (Def.pawnsArrivalModeDef!=null)
            {
                parms.raidArrivalMode = Def.pawnsArrivalModeDef;
            }
            else if (!Def.pawnsArrivalModeDefs.NullOrEmpty())
            {
                parms.raidArrivalMode = Def.pawnsArrivalModeDefs.RandomElement();
            }
            else
            {
                base.ResolveRaidArriveMode(parms);
            }
        }

        // Token: 0x06000ED0 RID: 3792 RVA: 0x0006EE15 File Offset: 0x0006D215
        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy;
        }

        // Token: 0x06000ED1 RID: 3793 RVA: 0x0006EE24 File Offset: 0x0006D224
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                text += "\n\n";
                text += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
            }
            return text;
        }

        // Token: 0x06000ED2 RID: 3794 RVA: 0x0006EEEE File Offset: 0x0006D2EE
        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        // Token: 0x06000ED3 RID: 3795 RVA: 0x0006EEF5 File Offset: 0x0006D2F5
        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
    }
}
