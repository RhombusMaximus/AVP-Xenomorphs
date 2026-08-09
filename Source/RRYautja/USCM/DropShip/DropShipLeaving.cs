using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace RRYautja
{
    public class DropShipLeaving : Skyfaller, IActiveTransporter, IThingHolder
    {
        public int groupID = -1;

        public PlanetTile destinationTile = PlanetTile.Invalid;

        public TransportersArrivalAction arrivalAction;

        private bool alreadyLeft;

        private static List<Thing> tmpActiveDropPods = new List<Thing>();

        public ActiveTransporterInfo Contents
        {
            get
            {
                if (innerContainer.Count > 0 && innerContainer[0] is ActiveTransporter at)
                    return at.Contents;
                return null;
            }
            set
            {
                if (innerContainer.Count > 0 && innerContainer[0] is ActiveTransporter at)
                    at.Contents = value;
            }
        }

        public IThingHolder ParentHolder => base.ParentHolder;

        public ThingOwner GetDirectlyHeldThings()
        {
            return null;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }
        // Token: 0x170005F0 RID: 1520
        // Token: 0x0600271D RID: 10013 RVA: 0x00129CD4 File Offset: 0x001280D4
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
            Scribe_Deep.Look<TransportersArrivalAction>(ref this.arrivalAction, "arrivalAction", new object[0]);
            Scribe_Values.Look<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
        }

        // Token: 0x0600271E RID: 10014 RVA: 0x00129D34 File Offset: 0x00128134
        protected override void LeaveMap()
        {
            if (this.groupID < 0 && this.destinationTile < 0)
            {
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            if (this.alreadyLeft)
            {
                base.LeaveMap();
                return;
            }
            if (this.groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + this.groupID);
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            if (this.destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + this.destinationTile);
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            Lord lord = TransporterUtility.FindLord(this.groupID, base.Map);
            if (lord != null)
            {
                base.Map.lordManager.RemoveLord(lord);
            }
            TravellingTransporters travelingTransportPods = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("RRY_USCM_TravelingDropshipUD4L", true));
            travelingTransportPods.Tile = base.Map.Tile;
            travelingTransportPods.SetFaction(Faction.OfPlayer);
            travelingTransportPods.destinationTile = this.destinationTile;
            travelingTransportPods.arrivalAction = this.arrivalAction;
            Find.WorldObjects.Add(travelingTransportPods);
            DropShipLeaving.tmpActiveTransporters.Clear();
            DropShipLeaving.tmpActiveTransporters.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveTransporter));
            for (int i = 0; i < DropShipLeaving.tmpActiveTransporters.Count; i++)
            {
                DropShipLeaving DropshipLeaving = DropShipLeaving.tmpActiveTransporters[i] as DropShipLeaving;
                if (DropshipLeaving != null && DropshipLeaving.groupID == this.groupID)
                {
                    DropshipLeaving.alreadyLeft = true;
                    travelingTransportPods.AddPod(DropshipLeaving.Contents, true);
                    DropshipLeaving.Contents = null;
                    DropshipLeaving.Destroy(DestroyMode.Vanish);
                }
            }
        }

        // Token: 0x04001628 RID: 5672
        private static List<Thing> tmpActiveTransporters = new List<Thing>();
    }
}
