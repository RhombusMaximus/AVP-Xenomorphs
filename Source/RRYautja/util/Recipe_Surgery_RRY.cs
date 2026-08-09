using RimWorld;
using RRYautja.settings;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000480 RID: 1152
    public class Recipe_Surgery_RRY : RecipeWorker
    {
        // Token: 0x06001465 RID: 5221 RVA: 0x0009C6B4 File Offset: 0x0009AAB4
        protected virtual bool CheckSurgeryFail(Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
        {
            IntVec3 patientLoc = patient.Position;
            Map patientMap = patient.Map;

            float num = 1f;
            if (!patient.RaceProps.IsMechanoid)
            {
                num *= surgeon.GetStatValue(StatDefOf.MedicalSurgerySuccessChance, true);
            }
            if (patient.InBed())
            {
                num *= patient.CurrentBed().GetStatValue(StatDefOf.SurgerySuccessChanceFactor, true);
            }
            num *= Recipe_Surgery_RRY.MedicineMedicalPotencyToSurgeryChanceFactor.Evaluate(this.GetAverageMedicalPotency(ingredients, bill));
            num *= this.recipe.surgerySuccessChanceFactor;
            if (surgeon.InspirationDef == DefDatabase<InspirationDef>.GetNamedSilentFail("Inspired_Surgery") && !patient.RaceProps.IsMechanoid)
            {
                num *= 2f;
                surgeon.mindState.inspirationHandler.EndInspiration(DefDatabase<InspirationDef>.GetNamedSilentFail("Inspired_Surgery"));
            }
            num = Mathf.Min(num, 0.98f);
            if (!Rand.Chance(num))
            {
                float faildeathchance;
                if (this.recipe == XenomorphDefOf.RRY_FaceHuggerRemoval)
                {
                    faildeathchance = SettingsHelper.latest.fachuggerRemovalFailureDeathChance;
                }
                else
                {
                    faildeathchance = SettingsHelper.latest.embryoRemovalFailureDeathChance;
                }
                if (Rand.Chance(faildeathchance))
                {
                    HealthShardTendUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
                    if (!patient.Dead)
                    {
                        patient.Kill(null, null);
                    }
                    Messages.Message("RRYMessageMedicalOperationFailureFatal".Translate(surgeon.LabelShort, patient.LabelShort, this.recipe.LabelCap, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient, MessageTypeDefOf.NegativeHealthEvent, true);
                }
                else if (Rand.Chance(0.5f))
                {
                    if (Rand.Chance(0.1f))
                    {
                        /*
                        surgeon.health.AddHediff(patient.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection), patient.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection).Part);
                        */
                        patient.health.RemoveHediff(patient.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection));
                        Messages.Message("RRYMessageMedicalOperationFailureRidiculous".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient, MessageTypeDefOf.NegativeHealthEvent, true);
                        HealthShardTendUtility.GiveInjuriesOperationFailureRidiculous(patient);

                    }
                    else
                    {
                        if (Rand.Chance(0.15f))
                        {
                            /*
                            surgeon.health.AddHediff(patient.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection), patient.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection).Part);
                            */
                            patient.health.RemoveHediff(patient.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection));
                        }
                        else if (Rand.Chance(0.35f))
                        {
                            GenSpawn.Spawn(ThingMaker.MakeThing(XenomorphDefOf.RRY_FilthBloodXenomorph_Active), surgeon.Position, surgeon.Map);
                        }
                        int a = Rand.Range(0,5);
                        for (int i = 0; i < a; i++)
                        {
                            GenSpawn.Spawn(ThingMaker.MakeThing(XenomorphDefOf.RRY_FilthBloodXenomorph_Active), patientLoc.RandomAdjacentCell8Way(), surgeon.Map);
                        }
                        Messages.Message("RRYMessageMedicalOperationFailureCatastrophic".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient, MessageTypeDefOf.NegativeHealthEvent, true);
                        HealthShardTendUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
                    }
                }
                else
                {
                    if (Rand.Chance(0.35f))
                    {
                        if (Rand.Chance(0.35f))
                        {
                            GenSpawn.Spawn(ThingMaker.MakeThing(XenomorphDefOf.RRY_FilthBloodXenomorph_Active), surgeon.Position, surgeon.Map);
                        }
                        else GenSpawn.Spawn(ThingMaker.MakeThing(XenomorphDefOf.RRY_FilthBloodXenomorph_Active), patientLoc.RandomAdjacentCell8Way(), surgeon.Map);
                        Messages.Message("RRYMessageMedicalOperationFailureMinorB".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient, MessageTypeDefOf.NegativeHealthEvent, true);
                        HealthShardTendUtility.GiveInjuriesOperationFailureMinor(patient, part);
                    }
                    else
                    {
                        Messages.Message("RRYMessageMedicalOperationFailureMinorA".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient, MessageTypeDefOf.NegativeHealthEvent, true);
                        HealthShardTendUtility.GiveInjuriesOperationFailureMinor(patient, part);
                    }
                }
                if (!patient.Dead)
                {
                    this.TryGainBotchedSurgeryThought(patient, surgeon);
                }
                return true;
            }
            return false;
        }

        // Token: 0x06001466 RID: 5222 RVA: 0x0009C93A File Offset: 0x0009AD3A
        private void TryGainBotchedSurgeryThought(Pawn patient, Pawn surgeon)
        {
            if (!patient.RaceProps.Humanlike)
            {
                return;
            }
            patient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BotchedMySurgery, surgeon);
        }

        // Token: 0x06001467 RID: 5223 RVA: 0x0009C970 File Offset: 0x0009AD70
        private float GetAverageMedicalPotency(List<Thing> ingredients, Bill bill)
        {
            ThingDef thingDef;
            if (bill is Bill_Medical bill_Medical)
            {
                thingDef = bill_Medical.consumedMedicine.Keys.FirstOrDefault();
            }
            else
            {
                thingDef = null;
            }
            int num = 0;
            float num2 = 0f;
            if (thingDef != null)
            {
                num++;
                num2 += thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
            }
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredients[i] is Medicine medicine)
                {
                    num += medicine.stackCount;
                    num2 += medicine.GetStatValue(StatDefOf.MedicalPotency, true) * (float)medicine.stackCount;
                }
            }
            if (num == 0)
            {
                return 1f;
            }
            return num2 / (float)num;
        }

        // Token: 0x04000C5C RID: 3164
        private const float MaxSuccessChance = 0.98f;

        // Token: 0x04000C5D RID: 3165
        private const float CatastrophicFailChance = 0.5f;

        // Token: 0x04000C5E RID: 3166
        private const float RidiculousFailChanceFromCatastrophic = 0.1f;

        // Token: 0x04000C5F RID: 3167
        private const float InspiredSurgerySuccessChanceFactor = 2f;

        // Token: 0x04000C60 RID: 3168
        private static readonly SimpleCurve MedicineMedicalPotencyToSurgeryChanceFactor = new SimpleCurve
        {
            {
                new CurvePoint(0f, 0.7f),
                true
            },
            {
                new CurvePoint(1f, 1f),
                true
            },
            {
                new CurvePoint(2f, 1.3f),
                true
            }
        };
    }
}
