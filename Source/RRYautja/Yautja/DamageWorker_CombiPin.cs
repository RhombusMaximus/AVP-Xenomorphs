using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000D4B RID: 3403
    public class DamageWorker_CombiPin : DamageWorker
    {
        // Token: 0x06004B83 RID: 19331 RVA: 0x002341B8 File Offset: 0x002325B8
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            Pawn pawn = thing as Pawn;
            if (pawn == null)
            {
                return base.Apply(dinfo, thing);
            }
            return this.ApplyToPawn(dinfo, pawn);
        }

        // Token: 0x06004B84 RID: 19332 RVA: 0x002341E4 File Offset: 0x002325E4
        private DamageWorker.DamageResult ApplyToPawn(DamageInfo dinfo, Pawn pawn)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
            if (dinfo.Amount <= 0f)
            {
                return damageResult;
            }
            if (!DebugSettings.enablePlayerDamage && pawn.Faction == Faction.OfPlayer)
            {
                return damageResult;
            }
            Map mapHeld = pawn.MapHeld;
            bool spawnedOrAnyParentSpawned = pawn.SpawnedOrAnyParentSpawned;
            if (dinfo.AllowDamagePropagation && dinfo.Amount >= (float)dinfo.Def.minDamageToFragment)
            {
                int num = Rand.RangeInclusive(2, 4);
                for (int i = 0; i < num; i++)
                {
                    DamageInfo dinfo2 = dinfo;
                    dinfo2.SetAmount(dinfo.Amount / (float)num);
                    this.ApplyDamageToPart(dinfo2, pawn, damageResult);
                }
            }
            else
            {
                this.ApplyDamageToPart(dinfo, pawn, damageResult);
                this.ApplySmallPawnDamagePropagation(dinfo, pawn, damageResult);
            }
            if (damageResult.wounded)
            {
                DamageWorker_CombiPin.PlayWoundedVoiceSound(dinfo, pawn);
                pawn.Drawer.Notify_DamageApplied(dinfo);
            }
            if (damageResult.headshot && pawn.Spawned)
            {
                MoteMaker.ThrowText(new Vector3((float)pawn.Position.x + 1f, (float)pawn.Position.y, (float)pawn.Position.z + 1f), pawn.Map, "Headshot".Translate(), Color.white, -1f);
                if (dinfo.Instigator != null)
                {
                    if (dinfo.Instigator is Pawn pawn2)
                    {
                        pawn2.records.Increment(RecordDefOf.Headshots);
                    }
                }
            }
            if ((damageResult.deflected || damageResult.diminished) && spawnedOrAnyParentSpawned)
            {
                EffecterDef effecterDef;
                if (damageResult.deflected)
                {
                    if (damageResult.deflectedByMetalArmor && dinfo.Def.canUseDeflectMetalEffect)
                    {
                        if (dinfo.Def == DamageDefOf.Bullet)
                        {
                            effecterDef = EffecterDefOf.Deflect_Metal_Bullet;
                        }
                        else
                        {
                            effecterDef = EffecterDefOf.Deflect_Metal;
                        }
                    }
                    else if (dinfo.Def == DamageDefOf.Bullet)
                    {
                        effecterDef = EffecterDefOf.Deflect_General_Bullet;
                    }
                    else
                    {
                        effecterDef = EffecterDefOf.Deflect_General;
                    }
                }
                else if (damageResult.diminishedByMetalArmor)
                {
                    effecterDef = EffecterDefOf.DamageDiminished_Metal;
                }
                else
                {
                    effecterDef = EffecterDefOf.DamageDiminished_General;
                }
                if (pawn.health.deflectionEffecter == null || pawn.health.deflectionEffecter.def != effecterDef)
                {
                    if (pawn.health.deflectionEffecter != null)
                    {
                        pawn.health.deflectionEffecter.Cleanup();
                        pawn.health.deflectionEffecter = null;
                    }
                    pawn.health.deflectionEffecter = effecterDef.Spawn();
                }
                pawn.health.deflectionEffecter.Trigger(pawn, dinfo.Instigator ?? pawn);
                if (damageResult.deflected)
                {
                    pawn.Drawer.Notify_DamageDeflected(dinfo);
                }
            }
            if (!damageResult.deflected && spawnedOrAnyParentSpawned)
            {
                ImpactSoundUtility.PlayImpactSound(pawn, dinfo.Def.impactSoundType, mapHeld);
            }
            return damageResult;
        }

        // Token: 0x06004B85 RID: 19333 RVA: 0x002344F0 File Offset: 0x002328F0
        private void CheckApplySpreadDamage(DamageInfo dinfo, Thing t)
        {
            if (dinfo.Def == DamageDefOf.Flame && !t.FlammableNow)
            {
                return;
            }
            if (Rand.Chance(0.5f))
            {
                dinfo.SetAmount((float)Mathf.CeilToInt(dinfo.Amount * Rand.Range(0.35f, 0.7f)));
                t.TakeDamage(dinfo);
            }
        }

        // Token: 0x06004B86 RID: 19334 RVA: 0x00234558 File Offset: 0x00232958
        private void ApplySmallPawnDamagePropagation(DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
        {
            if (!dinfo.AllowDamagePropagation)
            {
                return;
            }
            if (result.LastHitPart != null && dinfo.Def.harmsHealth && result.LastHitPart != pawn.RaceProps.body.corePart && result.LastHitPart.parent != null && pawn.health.hediffSet.GetPartHealth(result.LastHitPart.parent) > 0f && result.LastHitPart.parent.coverageAbs > 0f && dinfo.Amount >= 10f && pawn.HealthScale <= 0.5001f)
            {
                DamageInfo dinfo2 = dinfo;
                dinfo2.SetHitPart(result.LastHitPart.parent);
                this.ApplyDamageToPart(dinfo2, pawn, result);
            }
        }

        // Token: 0x06004B87 RID: 19335 RVA: 0x00234638 File Offset: 0x00232A38
        private void ApplyDamageToPart(DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
        {
            BodyPartRecord exactPartFromDamageInfo = this.GetExactPartFromDamageInfo(dinfo, pawn);
            if (exactPartFromDamageInfo == null)
            {
                return;
            }
            dinfo.SetHitPart(exactPartFromDamageInfo);
            float num = dinfo.Amount;
            bool flag = !dinfo.InstantPermanentInjury;
            bool deflectedByMetalArmor = false;
            if (flag)
            {
                DamageDef def = dinfo.Def;
                num = ArmorUtility.GetPostArmorDamage(pawn, num, dinfo.ArmorPenetrationInt, dinfo.HitPart, ref def, out deflectedByMetalArmor, out bool diminishedByMetalArmor);
                dinfo.Def = def;
                if (num < dinfo.Amount)
                {
                    result.diminished = true;
                    result.diminishedByMetalArmor = diminishedByMetalArmor;
                }
            }
            if (num <= 0f)
            {
                result.AddPart(pawn, dinfo.HitPart);
                result.deflected = true;
                result.deflectedByMetalArmor = deflectedByMetalArmor;
                return;
            }
            if (DamageWorker_CombiPin.IsHeadshot(dinfo, pawn))
            {
                result.headshot = true;
            }
            if (dinfo.InstantPermanentInjury)
            {
                HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart);
                if (hediffDefFromDamage.CompPropsFor(typeof(HediffComp_GetsPermanent)) == null || dinfo.HitPart.def.permanentInjuryChanceFactor == 0f || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(dinfo.HitPart))
                {
                    return;
                }
            }
            if (!dinfo.AllowDamagePropagation)
            {
                this.FinalizeAndAddInjury(pawn, num, dinfo, result);
                return;
            }
            this.ApplySpecialEffectsToPart(pawn, num, dinfo, result);
        }

        // Token: 0x06004B88 RID: 19336 RVA: 0x0023478F File Offset: 0x00232B8F
        protected virtual void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            totalDamage = this.ReduceDamageToPreserveOutsideParts(totalDamage, dinfo, pawn);
            this.FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
            this.CheckDuplicateDamageToOuterParts(dinfo, pawn, totalDamage, result);
        }

        // Token: 0x06004B89 RID: 19337 RVA: 0x002347B4 File Offset: 0x00232BB4
        protected float FinalizeAndAddInjury(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            if (pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
            {
                return 0f;
            }
            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart);
            Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn, null);
            hediff_Injury.Part = dinfo.HitPart;
            hediff_Injury.source = dinfo.Weapon;
            hediff_Injury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
            hediff_Injury.sourceHediffDef = dinfo.WeaponLinkedHediff;
            hediff_Injury.Severity = totalDamage;
            if (dinfo.InstantPermanentInjury)
            {
                HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff_Injury.TryGetComp<HediffComp_GetsPermanent>();
                if (hediffComp_GetsPermanent != null)
                {
                    hediffComp_GetsPermanent.IsPermanent = true;
                }
                else
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Tried to create instant permanent injury on Hediff without a GetsPermanent comp: ",
                        hediffDefFromDamage,
                        " on ",
                        pawn
                    });
                }
            }
            return this.FinalizeAndAddInjury(pawn, hediff_Injury, dinfo, result);
        }

        // Token: 0x06004B8A RID: 19338 RVA: 0x0023489C File Offset: 0x00232C9C
        protected float FinalizeAndAddInjury(Pawn pawn, Hediff_Injury injury, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            HediffComp_GetsPermanent hediffComp_GetsPermanent = injury.TryGetComp<HediffComp_GetsPermanent>();
            if (hediffComp_GetsPermanent != null)
            {
                hediffComp_GetsPermanent.PreFinalizeInjury();
            }
            pawn.health.AddHediff(injury, null, new DamageInfo?(dinfo), result);
            float num = Mathf.Min(injury.Severity, pawn.health.hediffSet.GetPartHealth(injury.Part));
            result.totalDamageDealt += num;
            result.wounded = true;
            result.AddPart(pawn, injury.Part);
            result.AddHediff(injury);
            return num;
        }

        // Token: 0x06004B8B RID: 19339 RVA: 0x00234924 File Offset: 0x00232D24
        private void CheckDuplicateDamageToOuterParts(DamageInfo dinfo, Pawn pawn, float totalDamage, DamageWorker.DamageResult result)
        {
            if (!dinfo.AllowDamagePropagation)
            {
                return;
            }
            if (dinfo.Def.harmAllLayersUntilOutside && dinfo.HitPart.depth == BodyPartDepth.Inside)
            {
                BodyPartRecord parent = dinfo.HitPart.parent;
                do
                {
                    if (pawn.health.hediffSet.GetPartHealth(parent) != 0f && parent.coverageAbs > 0f)
                    {
                        HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, parent);
                        Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn, null);
                        hediff_Injury.Part = parent;
                        hediff_Injury.source = dinfo.Weapon;
                        hediff_Injury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
                        hediff_Injury.Severity = totalDamage;
                        if (hediff_Injury.Severity <= 0f)
                        {
                            hediff_Injury.Severity = 1f;
                        }
                        this.FinalizeAndAddInjury(pawn, hediff_Injury, dinfo, result);
                    }
                    if (parent.depth == BodyPartDepth.Outside)
                    {
                        break;
                    }
                    parent = parent.parent;
                }
                while (parent != null);
            }
        }

        // Token: 0x06004B8C RID: 19340 RVA: 0x00234A2D File Offset: 0x00232E2D
        private static bool IsHeadshot(DamageInfo dinfo, Pawn pawn)
        {
            return !dinfo.InstantPermanentInjury && dinfo.HitPart.groups.Contains(BodyPartGroupDefOf.FullHead) && dinfo.Def == DamageDefOf.Bullet;
        }

        // Token: 0x06004B8D RID: 19341 RVA: 0x00234A6C File Offset: 0x00232E6C
        private BodyPartRecord GetExactPartFromDamageInfo(DamageInfo dinfo, Pawn pawn)
        {
            if (dinfo.HitPart != null)
            {
                return (!pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Any((BodyPartRecord x) => x == dinfo.HitPart)) ? null : dinfo.HitPart;
            }
            BodyPartRecord bodyPartRecord = this.ChooseHitPart(dinfo, pawn);
            if (bodyPartRecord == null)
            {
            //    Log.Warning("ChooseHitPart returned null (any part).");
            }
            return bodyPartRecord;
        }

        // Token: 0x06004B8E RID: 19342 RVA: 0x00234AF2 File Offset: 0x00232EF2
        protected virtual BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        {
            return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth, null);
        }

        // Token: 0x06004B8F RID: 19343 RVA: 0x00234B1C File Offset: 0x00232F1C
        private static void PlayWoundedVoiceSound(DamageInfo dinfo, Pawn pawn)
        {
            if (pawn.Dead)
            {
                return;
            }
            if (dinfo.InstantPermanentInjury)
            {
                return;
            }
            if (!pawn.SpawnedOrAnyParentSpawned)
            {
                return;
            }
            if (dinfo.Def.ExternalViolenceFor(pawn))
            {
                LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge ls) => ls.soundWounded, 1f);
            }
        }

        // Token: 0x06004B90 RID: 19344 RVA: 0x00234B88 File Offset: 0x00232F88
        protected float ReduceDamageToPreserveOutsideParts(float postArmorDamage, DamageInfo dinfo, Pawn pawn)
        {
            if (!DamageWorker_CombiPin.ShouldReduceDamageToPreservePart(dinfo.HitPart))
            {
                return postArmorDamage;
            }
            float partHealth = pawn.health.hediffSet.GetPartHealth(dinfo.HitPart);
            if (postArmorDamage < partHealth)
            {
                return postArmorDamage;
            }
            float maxHealth = dinfo.HitPart.def.GetMaxHealth(pawn);
            float num = postArmorDamage - partHealth;
            float f = num / maxHealth;
            float chance = this.def.overkillPctToDestroyPart.InverseLerpThroughRange(f);
            if (Rand.Chance(chance))
            {
                return postArmorDamage;
            }
            return postArmorDamage = partHealth - 1f;
        }

        // Token: 0x06004B91 RID: 19345 RVA: 0x00234C0F File Offset: 0x0023300F
        public static bool ShouldReduceDamageToPreservePart(BodyPartRecord bodyPart)
        {
            return bodyPart.depth == BodyPartDepth.Outside && !bodyPart.IsCorePart;
        }
    }
}
