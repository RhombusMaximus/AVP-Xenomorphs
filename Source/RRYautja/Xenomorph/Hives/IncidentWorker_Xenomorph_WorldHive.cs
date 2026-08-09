using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RRYautja;
using RimWorld.Planet;

namespace RimWorld
{
    // Token: 0x02000340 RID: 832
    public class IncidentWorker_Xenomorph_WorldHive : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            RimWorld.Planet.PlanetTile num = this.worldObject.Tile;
            if (num == -1)
            {
                if (!TileFinder.TryFindNewSiteTile(out num, 7, 27, false, null, 0.5f, true, TileFinderMode.Near, false, false))
                {
                    num = -1;
                }
            }
            else if (Find.WorldObjects.AnyWorldObjectAt(num))
            {
                if (!TileFinder.TryFindPassableTileWithTraversalDistance(num, 1, 50, out num, (PlanetTile x) => !Find.WorldObjects.AnyWorldObjectAt(x), false, TileFinderMode.Random, false))
                {
                    num = -1;
                }
            }
            if (num != -1)
            {
                this.worldObject.Tile = num;
                Find.WorldObjects.Add(this.worldObject);
                this.spawned = true;
            }
            return this.spawned;
        }
        // Token: 0x0400214A RID: 8522
        public string inSignal;

        // Token: 0x0400214B RID: 8523
        public WorldObject worldObject;

        // Token: 0x0400214C RID: 8524
        private bool spawned;
    }
}
