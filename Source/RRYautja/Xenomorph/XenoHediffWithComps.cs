using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000D7E RID: 3454
    public class XenoHediffWithComps : HediffWithComps
    {
        // Token: 0x06004CB6 RID: 19638 RVA: 0x0009BB84 File Offset: 0x00099F84
        public override void PostTick()
        {
            base.PostTick();
            if (this.comps != null)
            {
                float num = 0f;
                HediffComp_XenoSpawner _XenoSpawner = this.TryGetComp<HediffComp_XenoSpawner>();
                for (int i = 0; i < this.comps.Count; i++)
                {
                    this.comps[i].CompPostTick(ref num);
                }
                if (num != 0f)
                {
                    if (_XenoSpawner != null)
                    {
                        if (_XenoSpawner.RoyaleHugger && _XenoSpawner.Impregnations == 0)
                        {
                            num /= 5;
                        }
                    }
                    this.Severity += num;
                }
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override string LabelBase
        {
            get
            {
                HediffComp_XenoSpawner _XenoSpawner = this.TryGetComp<HediffComp_XenoSpawner>();
                if (_XenoSpawner!=null)
                {
                    if (_XenoSpawner.RoyaleHugger && _XenoSpawner.Impregnations==0)
                    {
                        return "xenomorph Royal embryo";
                    }
                }
                return this.def.label;
            }
        }

    }
}
