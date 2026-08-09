using RRYautja;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RimWorld
{
    // Token: 0x020006FF RID: 1791
    public class Returning_Projectile : Projectile
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.timesBounced, "timesBounced");
            Scribe_References.Look<Pawn>(ref this.OriginalPawn, "OriginalPawnRef");//, Props.pawn);
            Scribe_References.Look<Thing>(ref this.OriginalWeapon, "OriginalWeaponRef");//, Props.pawn);
            Scribe_References.Look<Projectile>(ref this.OriginalProjectile, "OriginalProjectileRef");//, Props.pawn);

            /*
            Scribe_Defs.Look<HediffDef>(ref MarkedhediffDef, "MarkedhediffDef");
            Scribe_References.Look<Corpse>(ref this.corpse, "corpseRef");//, Props.corpse);//
            Scribe_References.Look<Pawn>(ref this.pawn, "pawnRef");//, Props.pawn);
            Scribe_Values.Look<String>(ref this.MarkHedifftype, "thisMarktype");//, Props.Marklabel);
            Scribe_Values.Look<String>(ref this.MarkHedifflabel, "thislabel");//, Props.Marklabel);
            Scribe_Values.Look<bool>(ref this.predator, "thisPred");
            Scribe_Values.Look<float>(ref this.combatPower, "thiscombatPower");
            Scribe_Values.Look<float>(ref this.BodySize, "thisBodySize");
            */
        }
        public bool canBounce = false;
        public int ExtraTargets
        {
            get
            {
                int count = 0;
                bool enablebounce = false;
                if (OriginalPawn.Faction == Faction.OfPlayer)
                {
                    if (YautjaDefOf.RRY_YautjaRanged_Basic.IsFinished)
                    {
                        count++;
                    }
                    if (YautjaDefOf.RRY_YautjaRanged_Med.IsFinished)
                    {
                        count++;
                    }
                    if (YautjaDefOf.RRY_YautjaRanged_Adv.IsFinished)
                    {
                        count++;
                    }
                }

                if (OriginalPawn.apparel.WornApparelCount > 0)
                {
                    foreach (var item in OriginalPawn.apparel.WornApparel)
                    {
                        if (item.def.defName.Contains("RRY_Apparel_") && item.def.defName.Contains("BioMask"))
                        {
                            enablebounce = true;
                            this.canBounce = enablebounce;
                        }
                    }
                }

                if (!enablebounce)
                {
                    count = 0;
                }
                return count;
            }
        }

        public ThingDef BounceDef = (DefDatabase<ThingDef>.GetNamed("RRY_SmartDisk_Thrown"));
        public ThingDef ReturnDef;

        public Pawn OriginalPawn;
        public Thing OriginalWeapon;
        public Projectile OriginalProjectile;
        public int timesBounced = 0;
        
        // Token: 0x06002721 RID: 10017 RVA: 0x0012A314 File Offset: 0x00128714
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (this.timesBounced == 0)
            {
                this.OriginalPawn = (Pawn)launcher;
                this.OriginalWeapon = launcher;
                this.OriginalProjectile = this;
            }
            if (hitThing != null)
            {
                DamageDef damageDef = this.def.projectile.damageDef;
                float amount = (float)base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = this.launcher;
                ThingDef equipmentDef = this.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetration, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                if (hitThing is Pawn pawn && pawn.stances != null && pawn.BodySize <= this.def.projectile.stoppingPower + 0.001f)
                {
                    pawn.stances.StaggerFor(95);
                }
            }
            else
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map, false));
                MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    FleckMaker.WaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)base.DamageAmount) * 1f, 4f);
                }
            }
            if (OriginalPawn!=null)
            {

                if (OriginalPawn.kindDef.race != YautjaDefOf.RRY_Alien_Yautja && Rand.Chance(0.5f))
                {
                    ReturnDef = YautjaDefOf.RRY_SmartDisk_Thrown;
                }
                else
                {
                    ReturnDef = YautjaDefOf.RRY_SmartDisk_Returning;
                }
            }
            PostPostImpactEffects(hitThing);
            this.DeSpawn();
        }

        protected override void Tick()
        {
            base.Tick();
            /*
            if (this.usedTarget.Thing.Position != this.intendedTarget.Thing.Position)
            {
                this.Launch(launcher, this.ExactPosition, usedTarget, intendedTarget, ProjectileHitFlags.IntendedTarget, launcher);
            }
            */
        }

        public virtual void PostPostImpactEffects(Thing hitThing)
        {
            if (hitThing!= null && hitThing is Pawn hitPawn && !hitPawn.Dead && !hitPawn.Downed)
            {
            //    Log.Message(string.Format("hit {0}", hitThing));
                if (OriginalPawn != null)
                {
                //    Log.Message(string.Format("OriginalPawn {0}", OriginalPawn));
                }
                else
                {
                //    Log.Message(string.Format("OriginalPawn null {0}", OriginalPawn));
                }
                Hediff hediff = HediffMaker.MakeHediff(YautjaDefOf.RRY_Hediff_BouncedProjectile, hitPawn);
                Hediff_Bouncer bouncer = (Hediff_Bouncer)hediff;
                bouncer.OriginalPawn = this.OriginalPawn;
                bouncer.OriginalWeapon = this.OriginalWeapon;
                bouncer.OriginalProjectile = this.OriginalProjectile;
                bouncer.TimesBounced = this.timesBounced;
                if (hitPawn!=OriginalPawn) hitPawn.health.AddHediff(bouncer);
                else
                {
                //    Log.Message(string.Format("hitPawn:{0} != OriginalPawn: {1}", hitThing, OriginalPawn));
                //    Log.Message(string.Format("Making Projectile projectile"));
                    Projectile projectile = (Projectile)ThingMaker.MakeThing(ReturnDef, null);
                    
                //    Log.Message(string.Format("Target OriginalPawn {0}", OriginalPawn));
                    GenSpawn.Spawn(projectile, base.Position, launcher.Map, 0);
                //    Log.Message(string.Format("Launch projectile2 {0} at {1}", projectile, OriginalPawn));
                    projectile.Launch(OriginalWeapon, new LocalTargetInfo(OriginalPawn), OriginalPawn, ProjectileHitFlags.IntendedTarget, false, OriginalWeapon);
                }
            }
            else
            {
                if (hitThing!=null)
                {
                //    Log.Message(string.Format("hit non Pawn {0}", hitThing));
                }
                else
                {
                //    Log.Message(string.Format("hit null {0}", hitThing));
                }
                if (OriginalPawn != null)
                {
                //    Log.Message(string.Format("OriginalPawn {0}", OriginalPawn, OriginalPawn.kindDef.race));
                    if (OriginalPawn.kindDef.race != YautjaDefOf.RRY_Alien_Yautja && Rand.Chance(0.5f))
                    {
                        ReturnDef = YautjaDefOf.RRY_SmartDisk_Thrown;
                    }
                    else
                    {
                        ReturnDef = YautjaDefOf.RRY_SmartDisk_Returning;
                    }
                //    Log.Message(string.Format("hit ReturnDef: {0}", ReturnDef));
                }
                else
                {
                //    Log.Message(string.Format("OriginalPawn null {0}", OriginalPawn));
                }
            //    Log.Message(string.Format("Making Projectile projectile"));
                Projectile projectile = (Projectile)ThingMaker.MakeThing(ReturnDef, null);

            //    Log.Message(string.Format("ConvertingReturning_Projectile projectile2"));
                Returning_Projectile Rprojectile = new Returning_Projectile()
                {
                    def = ReturnDef
                };
                /*
            //    Log.Message(string.Format("projectile2: {0}", Rprojectile));
                Rprojectile.OriginalPawn = this.OriginalPawn;
            //    Log.Message(string.Format("projectile2.OriginalPawn: {0}", Rprojectile.OriginalPawn));
                Rprojectile.OriginalWeapon = this.OriginalWeapon;
            //    Log.Message(string.Format("projectile2.OriginalWeapon: {0}", Rprojectile.OriginalWeapon));
                //    projectile2.OriginalProjectile = this.OriginalProjectile;
                Rprojectile.timesBounced = this.timesBounced;
            //    Log.Message(string.Format("projectile2.timesBounced: {0}", Rprojectile.timesBounced));
                */
            //    Log.Message(string.Format("Target OriginalPawn {0}", OriginalPawn));
                GenSpawn.Spawn(projectile, base.Position, launcher.Map, 0);
            //    Log.Message(string.Format("Launch projectile2 {0} at {1}", Rprojectile, OriginalPawn));
                projectile.Launch(OriginalWeapon, new LocalTargetInfo(OriginalPawn), OriginalPawn, ProjectileHitFlags.IntendedTarget, false, OriginalWeapon);
            }
        }

    }
}
