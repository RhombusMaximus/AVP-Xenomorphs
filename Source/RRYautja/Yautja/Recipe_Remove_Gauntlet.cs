using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RRYautja
{
    // Token: 0x0200047F RID: 1151
    public class Recipe_Remove_Gauntlet : Recipe_Surgery
    {
        // Token: 0x06001462 RID: 5218 RVA: 0x0009DD30 File Offset: 0x0009C130
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < allHediffs.Count; i++)
            {
                if (allHediffs[i].Part != null)
                {
                    if (allHediffs[i].def == recipe.removesHediff)
                    {
                        yield return allHediffs[i].Part;
                    }
                }
            }
            yield break;
        }

        // Token: 0x06001463 RID: 5219 RVA: 0x0009DD5C File Offset: 0x0009C15C
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            
            bool flag = MedicalRecipesUtility.IsClean(pawn, part);
            bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
                {
                    string text;
                    if (!this.recipe.successfullyRemovedHediffMessage.NullOrEmpty())
                    {
                        text = string.Format(this.recipe.successfullyRemovedHediffMessage, billDoer.LabelShort, pawn.LabelShort);
                    }
                    else
                    {
                        text = "MessageSuccessfullyRemovedHediff".Translate(billDoer.LabelShort, pawn.LabelShort, this.recipe.removesHediff.label.Named("HEDIFF"), billDoer.Named("SURGEON"), pawn.Named("PATIENT"));
                    }
                    Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent, true);
                }
            }
            Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == this.recipe.removesHediff && x.Part == part);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
                DamageDef surgicalCut = DamageDefOf.SurgicalCut;
                float amount = 99999f;
                float armorPenetration = 999f;
                pawn.TakeDamage(new DamageInfo(surgicalCut, amount, armorPenetration, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                if (flag)
                {
                    if (pawn.Dead)
                    {
                        ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, null, PawnExecutionKind.OrganHarvesting);
                    }
                    ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, null);
                }
                if (flag2 && pawn.Faction != null && billDoer != null && billDoer.Faction != null)
                {
                    Faction faction = pawn.Faction;
                    Faction faction2 = billDoer.Faction;
                    int goodwillChange = -15;
                    string reason = "GoodwillChangedReason_RemovedBodyPart".Translate(part.LabelShort);
                    GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(pawn);
                    faction.TryAffectGoodwillWith(faction2, goodwillChange, true, true, null, reason, lookTarget);
                }
            }

            /*
            bool flag = MedicalRecipesUtility.IsClean(pawn, part);
            bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
                MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
            }
            DamageDef surgicalCut = DamageDefOf.SurgicalCut;
            float amount = 99999f;
            float armorPenetration = 999f;
            pawn.TakeDamage(new DamageInfo(surgicalCut, amount, armorPenetration, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
            if (flag)
            {
                if (pawn.Dead)
                {
                    ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, null, PawnExecutionKind.OrganHarvesting);
                }
                ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, null);
            }
            if (flag2 && pawn.Faction != null && billDoer != null && billDoer.Faction != null)
            {
                Faction faction = pawn.Faction;
                Faction faction2 = billDoer.Faction;
                int goodwillChange = -15;
                string reason = "GoodwillChangedReason_RemovedBodyPart".Translate(part.LabelShort);
                GlobalTargetInfo? lookTarget = new GlobalTargetInfo?(pawn);
                faction.TryAffectGoodwillWith(faction2, goodwillChange, true, true, null, reason, lookTarget);
            }
            */
        }

        // Token: 0x06001460 RID: 5216 RVA: 0x0009D948 File Offset: 0x0009BD48
        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            if (pawn.RaceProps.IsMechanoid || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
            {
                return RecipeDefOf.RemoveBodyPart.label;
            }
            return "RRY_Remove_Gauntlet".Translate();
        }
    }
}
