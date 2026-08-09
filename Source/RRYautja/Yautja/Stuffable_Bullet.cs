using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RRYautja
{
    // Token: 0x020006FF RID: 1791
    public class Stuffable_Bullet : Stuffable_Projectile
    {
        // Token: 0x06002721 RID: 10017 RVA: 0x0012A314 File Offset: 0x00128714
        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                DamageDef damageDef = this.def.projectile.damageDef;
                float amount = (float)base.DamageAmount;
            //    Log.Message(string.Format("Stuffable_Bullet Impact DamageAmount: {0}", amount));
                float armorPenetration = base.ArmorPenetration;
            //    Log.Message(string.Format("Stuffable_Bullet Impact armorPenetration: {0}", armorPenetration));
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
                FleckMaker.Static(this.ExactPosition, map, FleckDefOf.ShotHit_Dirt, 1f);
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    FleckMaker.WaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)base.DamageAmount) * 1f, 4f);
                }
            }
        }
    }
}
