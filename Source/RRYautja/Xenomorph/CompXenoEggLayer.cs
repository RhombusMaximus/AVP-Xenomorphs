using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x0200024A RID: 586
    public class CompProperties_XenoEggLayer : CompProperties
    {
        // Token: 0x06000AA8 RID: 2728 RVA: 0x00055568 File Offset: 0x00053968
        public CompProperties_XenoEggLayer()
        {
            this.compClass = typeof(CompXenoEggLayer);
        }

        // Token: 0x04000483 RID: 1155
        public float eggLayIntervalDays = 1f;

        // Token: 0x04000484 RID: 1156
        public IntRange eggCountRange = IntRange.one;
        
        // Token: 0x04000486 RID: 1158
        public ThingDef eggDef;

        // Token: 0x04000487 RID: 1159
        public int eggCountMax = 1;

        // Token: 0x04000488 RID: 1160
        public bool eggLayFemaleOnly = true;
        
    }

    // Token: 0x02000737 RID: 1847
    public class CompXenoEggLayer : ThingComp
    {
        // Token: 0x17000619 RID: 1561
        // (get) Token: 0x0600288E RID: 10382 RVA: 0x00134C34 File Offset: 0x00133034
        private bool Active
        {
            get
            {
                Pawn pawn = this.parent as Pawn;
                return (!this.Props.eggLayFemaleOnly || pawn == null || pawn.gender == Gender.Female) && (pawn == null || pawn.ageTracker.CurLifeStage.milkable);
            }
        }

        // Token: 0x1700061A RID: 1562
        // (get) Token: 0x0600288F RID: 10383 RVA: 0x00134C8F File Offset: 0x0013308F
        public bool CanLayNow
        {
            get
            {
                return this.Active && this.eggProgress >= 1f;
            }
        }

        // Token: 0x1700061B RID: 1563
        // (get) Token: 0x06002890 RID: 10384 RVA: 0x00134CAE File Offset: 0x001330AE
        public bool FullyFertilized
        {
            get
            {
                return this.fertilizationCount >= this.Props.eggCountMax;
            }
        }

        // Token: 0x1700061C RID: 1564
        // (get) Token: 0x06002891 RID: 10385 RVA: 0x00134CC6 File Offset: 0x001330C6
        private bool ProgressStoppedBecauseUnfertilized
        {
            get
            {
                return false;
            }
        }

        // Token: 0x1700061D RID: 1565
        // (get) Token: 0x06002892 RID: 10386 RVA: 0x00134CEC File Offset: 0x001330EC
        public CompProperties_XenoEggLayer Props
        {
            get
            {
                return (CompProperties_XenoEggLayer)this.props;
            }
        }

        // Token: 0x06002893 RID: 10387 RVA: 0x00134CFC File Offset: 0x001330FC
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.eggProgress, "eggProgress", 0f, false);
            Scribe_Values.Look<int>(ref this.fertilizationCount, "fertilizationCount", 0, false);
            Scribe_References.Look<Pawn>(ref this.fertilizedBy, "fertilizedBy", false);
        }

        // Token: 0x06002894 RID: 10388 RVA: 0x00134D48 File Offset: 0x00133148
        public override void CompTick()
        {
            if (this.Active)
            {
                float num = 1f / (this.Props.eggLayIntervalDays * 60000f);
                if (this.parent is Pawn pawn)
                {
                    num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
                }
                this.eggProgress += num;
                if (this.eggProgress > 1f)
                {
                    this.eggProgress = 1f;
                }
            }
        }

        // Token: 0x06002895 RID: 10389 RVA: 0x00134DD8 File Offset: 0x001331D8
        public void Fertilize(Pawn male)
        {
            this.fertilizationCount = this.Props.eggCountMax;
            this.fertilizedBy = male;
        }

        // Token: 0x06002896 RID: 10390 RVA: 0x00134DF4 File Offset: 0x001331F4
        public virtual List<Thing> ProduceEggs()
        {
            List<Thing> eggs = new List<Thing>();
            if (!this.Active)
            {
                return eggs;
            }
            this.eggProgress = 0f;
            int count = this.Props.eggCountRange.RandomInRange;
            if (count == 0)
            {
                return eggs;
            }
            this.fertilizationCount = Mathf.Max(0, this.fertilizationCount - count);

            Map eggMap = this.parent.MapHeld ?? this.parent.Map;
            IntVec3 eggPos = this.parent.PositionHeld;
            if (eggPos == IntVec3.Invalid) eggPos = this.parent.Position;

            for (int i = 0; i < count; i++)
            {
                Thing egg = ThingMaker.MakeThing(this.Props.eggDef, null);
                Building_XenoEgg hatcher = egg as Building_XenoEgg;
                if (hatcher != null)
                {
                    hatcher.hatcheeFaction = this.parent.Faction;
                    if (this.parent is Pawn pawn)
                    {
                        hatcher.hatcheeParent = pawn;
                    }
                }
                eggs.Add(egg);
            }
            return eggs;
        }

        public virtual Thing ProduceEgg()
        {
            // Legacy method — returns first egg only
            var eggs = ProduceEggs();
            // Spawn extras at adjacent cells
            Map eggMap = this.parent.MapHeld ?? this.parent.Map;
            if (eggs.Count > 1 && eggMap != null)
            {
                IntVec3 eggPos = this.parent.PositionHeld;
                if (eggPos == IntVec3.Invalid) eggPos = this.parent.Position;
                for (int i = 1; i < eggs.Count; i++)
                {
                    IntVec3 spawnLoc = CellFinder.RandomClosewalkCellNear(eggPos, eggMap, 2);
                    GenSpawn.Spawn(eggs[i], spawnLoc, eggMap, WipeMode.Vanish);
                }
            }
            return eggs.Count > 0 ? eggs[0] : null;
        }

        // Token: 0x06002897 RID: 10391 RVA: 0x00134EE8 File Offset: 0x001332E8
        public override string CompInspectStringExtra()
        {
            if (!this.Active)
            {
                return null;
            }
            string text = "EggProgress".Translate() + ": " + this.eggProgress.ToStringPercent();
            if (this.fertilizationCount > 0)
            {
                text = text + "\n" + "Fertilized".Translate();
            }
            else if (this.ProgressStoppedBecauseUnfertilized)
            {
                text = text + "\n" + "ProgressStoppedUntilFertilized".Translate();
            }
            return text;
        }

        // Token: 0x040016AB RID: 5803
        private float eggProgress;

        // Token: 0x040016AC RID: 5804
        private int fertilizationCount;

        // Token: 0x040016AD RID: 5805
        private Pawn fertilizedBy;
    }
}
