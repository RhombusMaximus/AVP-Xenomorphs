using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RRYautja
{
    public class HediffCompProperties_Overheating : HediffCompProperties
    {
        public HediffCompProperties_Overheating()
        {
            this.compClass = typeof(HediffComp_Overheating);
        }
    }

    public class HediffComp_Overheating : HediffComp
    {
        public HediffCompProperties_Overheating Props => (HediffCompProperties_Overheating)this.props;
        public HediffDef burn = DefDatabase<HediffDef>.GetNamed("Burn");
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref this.ticksUntilSmoke, "ticksUntilSmoke");
        }
        
        private int ticksUntilSmoke;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            int ind = parent.CurStageIndex;
            this.ticksUntilSmoke--;
            if (ind >= 4)
            {
                if (Find.TickManager.TicksGame % 60 == 0 && Pawn.Spawned && Pawn.Map != null)
                {
                    float chance = parent.Severity / parent.def.lethalSeverity;
                    if (ind == 5)
                    {
                        Rand.PushState();
                        if (Rand.Chance(chance / 5))
                        {
                            BodyPartRecord damaged = this.Pawn.health.hediffSet.GetRandomNotMissingPart(null, BodyPartHeight.Undefined, BodyPartDepth.Inside);
                            if (damaged != null)
                            {
                                Pawn.health.AddHediff(HediffMaker.MakeHediff(burn, Pawn, damaged));
                                if (Rand.Chance(chance / 10))
                                {
                                    Pawn.TryAttachFire(chance / 4, null);
                                }
                                else
                                {
                                    IntVec3 @int = Pawn.Position.RandomAdjacentCell8Way();
                                }
                            }
                        }
                        Rand.PopState();
                    }
                    if (parent.Severity > 1f && Rand.Value < parent.Severity)
                    {
                        FleckMaker.ThrowMicroSparks(Pawn.DrawPos, Pawn.Map);
                    }
                }
                if (this.ticksUntilSmoke <= 0)
                {
                    this.SpawnSmokeParticles();
                }
            }
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            if (parent.Severity > 1f && Find.TickManager.TicksGame % 60 == 0)
            {
                float chance = parent.Severity / parent.def.lethalSeverity;
                if (chance > 0.75f)
                {
                    if (Rand.Chance(chance))
                    {
                        GenExplosion.DoExplosion(Pawn.Position,Pawn.Map, parent.Severity, DamageDefOf.Burn, Pawn, 1,2,SoundDefOf.FireBurning, chanceToStartFire: chance);
                    }
                }
            }

        }

        private void SpawnSmokeParticles()
        {
            if (parent.CurStageIndex > 2)
            {
                FleckMaker.ThrowSmoke(Pawn.DrawPos, Pawn.Map, this.parent.Severity);
            }
            if (parent.Severity > 1.25f && this.parent == null)
            {
                FleckMaker.ThrowFireGlow(Pawn.Position.ToVector3Shifted(), Pawn.Map, Pawn.BodySize);
            }
            float num = Pawn.BodySize / 2f;
            if (num > 1f)
            {
                num = 1f;
            }
            num = 1f - num;
            this.ticksUntilSmoke = HediffComp_Overheating.SmokeIntervalRange.Lerped(num) + (int)(10f * Rand.Value);
        }

        private void DoOverheatDamage(Thing targ)
        {
            int num = GenMath.RoundRandom(Mathf.Clamp(0.0125f + 0.0036f * Pawn.BodySize, 0.0125f, 0.05f) * 150f);
            if (num < 1)
            {
                num = 1;
            }
            Pawn pawn = targ as Pawn;
            if (pawn != null)
            {
                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
                Find.BattleLog.Add(battleLogEntry_DamageTaken);
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, (float)num, 0f, -1f, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                targ.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
                Apparel apparel;
                if (pawn.apparel != null && pawn.apparel.WornApparel.TryRandomElement(out apparel))
                {
                    apparel.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num, 0f, -1f, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    return;
                }
            }
            else
            {
                targ.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num, 0f, -1f, Pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
            }
        }
        private static readonly IntRange SmokeIntervalRange = new IntRange(130, 200);
    }
}
