using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RRYautja
{
    // Token: 0x02000003 RID: 3
    public class Projectile_AcidSpit : Projectile
    {
        // Token: 0x06000003 RID: 3 RVA: 0x000020FC File Offset: 0x000010FC
        protected override void Tick()
        {
            base.Tick();
            checked
            {
                this.TicksforAppearence--;
                bool flag = this.TicksforAppearence == 0 && base.Map != null;
                if (flag)
                {
                    TrailThrower.ThrowSmokeTrail(base.Position.ToVector3Shifted(), 0.7f, base.Map, "Mote_Acidtrail");
                    this.TicksforAppearence = 6;
                }
            }
        }

        // Token: 0x04000001 RID: 1
        private int TicksforAppearence = 3;
    }
}
