using System;
using RimWorld;
using Verse;

namespace RRYautja
{
    // Token: 0x02000016 RID: 22
    public class HediffComp_Blur : HediffComp
    {
        // Token: 0x17000026 RID: 38
        // (get) Token: 0x060000BA RID: 186 RVA: 0x00009714 File Offset: 0x00007914
        public string labelCap
        {
            get
            {
                return base.Def.LabelCap;
            }
        }

        // Token: 0x17000027 RID: 39
        // (get) Token: 0x060000BB RID: 187 RVA: 0x00009734 File Offset: 0x00007934
        public string label
        {
            get
            {
                return base.Def.label;
            }
        }

        // Token: 0x060000BC RID: 188 RVA: 0x00009754 File Offset: 0x00007954
        private void Initialize()
        {
            bool spawned = base.Pawn.Spawned;
            bool flag = spawned && base.Pawn.Map != null;
            if (flag)
            {
                FleckMaker.ThrowLightningGlow(GenThing.TrueCenter(base.Pawn), base.Pawn.Map, 3f);
            }
        }

        // Token: 0x060000BD RID: 189 RVA: 0x000097AC File Offset: 0x000079AC
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            bool flag = base.Pawn != null;
            bool flag2 = flag;
            if (flag2)
            {
                bool flag3 = this.initializing;
                if (flag3)
                {
                    this.initializing = false;
                    this.Initialize();
                }
            }
        }

        // Token: 0x0400008F RID: 143
        private bool initializing = true;

        // Token: 0x04000090 RID: 144
        public int blurTick = 0;
    }
}
