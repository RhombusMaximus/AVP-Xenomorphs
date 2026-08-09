using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RRYautja
{
    public class Projectile_AbilityRRY_ThrownWeapon : Projectile_AbilityRRY
    {

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
            }
            if (launcher is Pawn launcherPawn)
            {
            //    Log.Message(string.Format("launcherPawn: {0}", launcherPawn.LabelShortCap));
                PostPostImpactEffects();
            }
        }

        protected override void Tick()
        {
            base.Tick();
        }
        

        public virtual void PostPostImpactEffects()
        {
            if (launcher is Pawn launcherPawn)
            { // this.targetVec;
                ThingWithComps weapon = launcherPawn.equipment.Primary;
                GenSpawn.Spawn(weapon, base.PositionHeld, launcherPawn.Map);
            }
        }
        
    }
}