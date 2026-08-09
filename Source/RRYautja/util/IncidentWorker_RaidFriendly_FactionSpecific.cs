using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x02000355 RID: 853
    public class IncidentWorker_RaidFriendly_FactionSpecific : IncidentWorker_RaidFriendly
    {
        public IncidentDef_FactionSpecific Def => (IncidentDef_FactionSpecific)this.def;
        
        // Token: 0x06000EBF RID: 3775 RVA: 0x0006E99C File Offset: 0x0006CD9C
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
                for (int i = 0; i < this.Def.Factions.Count * 10; i++)
                {
                    Faction faction = Find.FactionManager.FirstFactionOfDef(this.Def.Factions.RandomElement());
                    if (faction != null && !faction.HostileTo(Faction.OfPlayer))
                    {
                        parms.faction = faction;
                        break;
                    }
                }
            }
            if (parms.faction != null)
            {
                if (parms.faction.HostileTo(Faction.OfPlayer))
                {
                    return false;
                }
                return true;
            }
            if (!base.CandidateFactions(parms, false).Any<Faction>())
            {
                return false;
            }
            parms.faction = base.CandidateFactions(parms, false).RandomElementByWeight((Faction fac) => (float)fac.PlayerGoodwill + 120.000008f);
            return true;
        }

        // Token: 0x06000EC0 RID: 3776 RVA: 0x0006EA07 File Offset: 0x0006CE07
        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        // Token: 0x06000EC1 RID: 3777 RVA: 0x0006EA20 File Offset: 0x0006CE20
        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        // Token: 0x06000EC2 RID: 3778 RVA: 0x0006EA43 File Offset: 0x0006CE43
        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelFriendly;
        }

        // Token: 0x06000EC3 RID: 3779 RVA: 0x0006EA50 File Offset: 0x0006CE50
        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = string.Format(parms.raidArrivalMode.textFriendly, parms.faction.def.pawnsPlural, parms.faction.Name);
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextFriendly;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                text += "\n\n";
                text += "FriendlyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
            }
            return text;
        }

        // Token: 0x06000EC4 RID: 3780 RVA: 0x0006EB1A File Offset: 0x0006CF1A
        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.PositiveEvent;
        }

        // Token: 0x06000EC5 RID: 3781 RVA: 0x0006EB21 File Offset: 0x0006CF21
        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidFriendly".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
	}
}
