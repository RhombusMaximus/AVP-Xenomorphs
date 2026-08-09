using System;
using UnityEngine;
using Verse;

namespace RRYautja
{
    public class Projectile_AbilityRRY_ThrownReturning : Projectile_AbilityRRY
    {
        public new int TicksToImpact => ticksToImpact;

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                var damageAmountBase = def.projectile.GetDamageAmount(1f);
                var equipmentDef = this.equipmentDef;
                var dinfo = new DamageInfo(def.projectile.damageDef, damageAmountBase, this.def.projectile.GetArmorPenetration(1f), ExactRotation.eulerAngles.y,
                    launcher, null, equipmentDef);
                hitThing.TakeDamage(dinfo);
                PostImpactEffects(hitThing);
            }
            PostPostImpactEffects(hitThing);
        }



#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public virtual void PostImpactEffects(Thing hitThing)
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
        {

        }

        public virtual void PostPostImpactEffects(Thing hitThing)
        {
            LocalTargetInfo targetInfo = new LocalTargetInfo(launcher.Position);
            Projectile projectile2 = (Projectile)ThingMaker.MakeThing(def, null);
            GenSpawn.Spawn(projectile2, base.PositionHeld, launcher.Map, 0);
            projectile2.Launch(this, base.PositionHeld.ToVector3(), launcher, launcher, ProjectileHitFlags.IntendedTarget, launcher);
        }
        
        
    }
}