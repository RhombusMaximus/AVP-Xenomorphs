using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x0200025D RID: 605
    public class CompProperties_HiveTunnel : CompProperties
    {
        // Token: 0x06000ACB RID: 2763 RVA: 0x00056376 File Offset: 0x00054776
        public CompProperties_HiveTunnel()
        {
            this.compClass = typeof(CompHiveTunnel);
        }

        // Token: 0x040004D6 RID: 1238
        public float massCapacity = 150f;

        // Token: 0x040004D7 RID: 1239
        public float restEffectiveness;
    }
    // Token: 0x02000771 RID: 1905
    [StaticConstructorOnStartup]
    public class CompHiveTunnel : ThingComp, IThingHolder
    {
        // Token: 0x06002A20 RID: 10784 RVA: 0x0013ED55 File Offset: 0x0013D155
        public CompHiveTunnel()
        {
            this.innerContainer = new ThingOwner<Thing>(this);
        }

        // Token: 0x17000684 RID: 1668
        // (get) Token: 0x06002A21 RID: 10785 RVA: 0x0013ED70 File Offset: 0x0013D170
        public CompProperties_HiveTunnel Props
        {
            get
            {
                return (CompProperties_HiveTunnel)this.props;
            }
        }

        // Token: 0x17000685 RID: 1669
        // (get) Token: 0x06002A22 RID: 10786 RVA: 0x0013ED7D File Offset: 0x0013D17D
        public Map Map
        {
            get
            {
                return this.parent.MapHeld;
            }
        }
        
        // Token: 0x06002A2B RID: 10795 RVA: 0x0013F0EC File Offset: 0x0013D4EC
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }

        // Token: 0x06002A2C RID: 10796 RVA: 0x0013F154 File Offset: 0x0013D554
        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        // Token: 0x06002A2D RID: 10797 RVA: 0x0013F15C File Offset: 0x0013D55C
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        // Token: 0x06002A2E RID: 10798 RVA: 0x0013F16C File Offset: 0x0013D56C
        public override void CompTick()
        {
            base.CompTick();
            this.innerContainer.DoTick(true);
        }

        // Token: 0x06002A31 RID: 10801 RVA: 0x0013F2F8 File Offset: 0x0013D6F8
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map);
            this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near, null, null);
        }

        // Token: 0x06002A32 RID: 10802 RVA: 0x0013F348 File Offset: 0x0013D748
        public override string CompInspectStringExtra()
        {
            return "Contents".Translate() + ": " + this.innerContainer.ContentsString.CapitalizeFirst();
        }

        // Token: 0x06002A34 RID: 10804 RVA: 0x0013F3F8 File Offset: 0x0013D7F8
        public void Notify_ThingAdded(Thing t)
        {

        }

        // Token: 0x06002A35 RID: 10805 RVA: 0x0013F407 File Offset: 0x0013D807
        public void Notify_ThingAddedAndMergedWith(Thing t, int mergedCount)
        {

        }

        // Token: 0x06002A39 RID: 10809 RVA: 0x0013F4B0 File Offset: 0x0013D8B0
        public void CleanUpLoadingVars(Map map)
        {
            this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near, null, null);
        }
        
        // Token: 0x04001757 RID: 5975
        public ThingOwner innerContainer;
    }
}
