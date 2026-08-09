using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RRYautja.ExtensionMethods;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RRYautja
{
    // Token: 0x02000743 RID: 1859
    public class CompProperties_XenoHatcher : CompProperties
    {
        // Token: 0x060028EA RID: 10474 RVA: 0x00136AE7 File Offset: 0x00134EE7
        public CompProperties_XenoHatcher()
        {
            this.compClass = typeof(CompXenoHatcher);
        }

        // Token: 0x040016C3 RID: 5827
        public float hatcherDaystoHatch = 1f;
        public float triggerRadius = 10f;
        public float minGestationTemp = -30f;
    }

    // Token: 0x02000744 RID: 1860
    public class CompXenoHatcher : ThingComp
    {
        public CompProperties_XenoHatcher Props => (CompProperties_XenoHatcher)this.props;
        public override void CompTick()
        {

        }

        // Token: 0x0600295E RID: 10590 RVA: 0x00139BAC File Offset: 0x00137FAC
        public override void CompTickRare()
        {

        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
        {
            base.PostPostGeneratedForTrader(trader, forTile, forFaction);
        }
        /*
        public override string CompInspectStringExtra()
        {
            if (!this.TemperatureDamaged)
            {
                if (this.mutateProgress > 0f && !QueenPresent && Prefs.DevMode && DebugSettings.godMode)
                {
                    return "Xeno_Egg_Gestation_Progress".Translate() + ": " + this.gestateProgress.ToStringPercent() + "\n" + GetDescription(eggType)+ " " + "Xeno_Egg_Mutation_Progress".Translate() + ": " + this.mutateProgress.ToStringPercent();
                }
                return "Xeno_Egg_Gestation_Progress".Translate() + ": " + this.gestateProgress.ToStringPercent() + "\n" + GetDescription(eggType);
            }
            return null;
        }
        */
    }
}
