using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000275 RID: 629
    public class IngestionOutcomeDoer_RemoveHediff : IngestionOutcomeDoer
    {
        // Token: 0x06000AED RID: 2797 RVA: 0x00057114 File Offset: 0x00055514
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            bool hashediff = pawn.health.hediffSet.hediffs.Any(x=> x.def == hediffDef || hediffDefs.Contains(x.def));
            if (hashediff)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs.Find(x => x.def == hediffDef || hediffDefs.Contains(x.def));
                if (hediffDef == null)
                {
                    hediffDef = hediff.def;
                }
                float factor = range.RandomInRange;
                float num = (hediff.Severity <= hediffDef.initialSeverity ? hediffDef.initialSeverity : hediffDef.maxSeverity) * factor;
                Mathf.Clamp(num, hediffDef.minSeverity, hediffDef.maxSeverity);
                hediff.Severity = hediff.Severity - num;
                if (hediff.Severity <= Math.Max(0f, hediffDef.minSeverity))
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        // Token: 0x06000AEE RID: 2798 RVA: 0x00057198 File Offset: 0x00055598
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (parentDef.IsDrug && this.chance >= 1f)
            {
                foreach (StatDrawEntry s in this.hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
                {
                    yield return s;
                }
            }
            yield break;
        }

        // Token: 0x0400053A RID: 1338
        public HediffDef hediffDef;
        public List<HediffDef> hediffDefs = new List<HediffDef>();

        public FloatRange range = FloatRange.ZeroToOne;
    }
}
