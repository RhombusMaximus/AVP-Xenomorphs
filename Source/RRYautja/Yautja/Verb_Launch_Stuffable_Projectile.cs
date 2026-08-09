using System;
using RimWorld;
using RRYautja;
using UnityEngine;

namespace Verse
{
    // Token: 0x0200102C RID: 4140
    public class Verb_Launch_Stuffable_Projectile : Verb
    {
        // Token: 0x1700104C RID: 4172
        // (get) Token: 0x060064F2 RID: 25842 RVA: 0x001B8B58 File Offset: 0x001B6F58
        public virtual ThingDef Projectile
        {
            get
            {
                if (base.EquipmentSource != null)
                {
                    CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                    if (comp != null && comp.Loaded)
                    {
                        return comp.Projectile;
                    }
                }
                return this.verbProps.defaultProjectile;
            }
        }

        // Token: 0x060064F3 RID: 25843 RVA: 0x001B8BA0 File Offset: 0x001B6FA0
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(this.caster, (!this.currentTarget.HasThing) ? null : this.currentTarget.Thing, base.EquipmentSource?.def, this.Projectile, this.ShotsPerBurst > 1));
        }

        // Token: 0x060064F4 RID: 25844 RVA: 0x001B8C14 File Offset: 0x001B7014
        protected override bool TryCastShot()
        {
        //    Log.Message(string.Format("{0}, {1}, {2}, {3}", CasterPawn.equipment.Primary != null , CasterPawn.equipment.Primary.def.IsRangedWeapon , CasterPawn.equipment.PrimaryEq.PrimaryVerb.HarmsHealth() , !CasterPawn.equipment.PrimaryEq.PrimaryVerb.UsesExplosiveProjectiles()));
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
            {
                return false;
            }
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return false;
            }
            bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out ShootLine shootLine);
            if (this.verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }
            if (base.EquipmentSource != null)
            {
                CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                if (comp != null)
                {
                    comp.Notify_ProjectileLaunched();
                }
            }
            Thing launcher = this.caster;
            Thing equipment = base.EquipmentSource;
            CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = this.caster;
            }
            Vector3 drawPos = this.caster.DrawPos;
            Stuffable_Projectile projectile2 = (Stuffable_Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.caster.Map, WipeMode.Vanish);
            if (this.verbProps.ForcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(this.verbProps.ForcedMissRadius, this.currentTarget.Cell - this.caster.Position);
                if (num > 0.5f)
                {
                    int max = GenRadial.NumCellsInRadius(num);
                    int num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num2];
                        this.ThrowDebugText("ToRadius");
                        this.ThrowDebugText("Rad\nDest", c);
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }
                        if (!this.canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                        }
                        projectile2.Launch(launcher, drawPos, c, this.currentTarget, projectileHitFlags, false, equipment, null);
                        return true;
                    }
                }
            }
            ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = randomCoverToMissInto?.def;
            if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget, false, __instance.caster.Map);
                this.ThrowDebugText("ToWild" + ((!this.canHitNonTargetPawnsNow) ? string.Empty : "\nchntp"));
                this.ThrowDebugText("Wild\nDest", shootLine.Dest);
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f) && this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags2, false, equipment, targetCoverDef);
                return true;
            }
            if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
            {
                this.ThrowDebugText("ToCover" + ((!this.canHitNonTargetPawnsNow) ? string.Empty : "\nchntp"));
                this.ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                if (this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, false, equipment, targetCoverDef);
                return true;
            }
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (this.canHitNonTargetPawnsNow)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }
            this.ThrowDebugText("ToHit" + ((!this.canHitNonTargetPawnsNow) ? string.Empty : "\nchntp"));
            if (this.currentTarget.Thing != null)
            {
                projectile2.Launch(launcher, drawPos, this.currentTarget, this.currentTarget, projectileHitFlags4, false, equipment, targetCoverDef);
                this.ThrowDebugText("Hit\nDest", this.currentTarget.Cell);
            }
            else
            {
                projectile2.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags4, false, equipment, targetCoverDef);
                this.ThrowDebugText("Hit\nDest", shootLine.Dest);
            }
            return true;
        }

        // Token: 0x060064F5 RID: 25845 RVA: 0x001B907E File Offset: 0x001B747E
        private void ThrowDebugText(string text)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, text, -1f);
            }
        }

        // Token: 0x060064F6 RID: 25846 RVA: 0x001B90AB File Offset: 0x001B74AB
        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), this.caster.Map, text, -1f);
            }
        }

        // Token: 0x060064F7 RID: 25847 RVA: 0x001B90D4 File Offset: 0x001B74D4
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return 0f;
            }
            return projectile.projectile.explosionRadius;
        }

        // Token: 0x060064F8 RID: 25848 RVA: 0x001B9104 File Offset: 0x001B7504
        public override bool Available()
        {
            if (!base.Available())
            {
                return false;
            }
            if (base.CasterIsPawn)
            {
                Pawn casterPawn = base.CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }
            return this.Projectile != null;
        }
    }
}
