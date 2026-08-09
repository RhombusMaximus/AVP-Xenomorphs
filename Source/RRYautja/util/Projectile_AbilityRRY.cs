using UnityEngine;
using Verse;

namespace RRYautja
{
    public class Projectile_AbilityRRY : Projectile
    {
        public int TicksToImpact => ticksToImpact;
        
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                var damageAmountBase = def.projectile.GetDamageAmount(1f, null);
                var equipmentDef = this.equipmentDef;
                var dinfo = new DamageInfo(def.projectile.damageDef, damageAmountBase, this.def.projectile.GetArmorPenetration(null), ExactRotation.eulerAngles.y,
                    launcher, null, equipmentDef);
                hitThing.TakeDamage(dinfo);
                PostImpactEffects(hitThing);
            }
        }

        public virtual void PostImpactEffects(Thing hitThing)
        {

        }
        
    }
}