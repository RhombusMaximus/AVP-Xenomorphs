using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RRYautja
{
    // Token: 0x02000695 RID: 1685
    public class MinifiedXenoEgg : MinifiedThing
    {
        // Token: 0x17000538 RID: 1336
        // (get) Token: 0x06002377 RID: 9079 RVA: 0x0010F388 File Offset: 0x0010D788
        public override Graphic Graphic
        {
            get
            {
                if (this.cachedGraphic == null)
                {
                    this.cachedGraphic = this.InnerThing.Graphic.ExtractInnerGraphicFor(this.InnerThing);
                    if ((float)this.InnerThing.def.size.x > 1.1f || (float)this.InnerThing.def.size.z > 1.1f)
                    {
                        Vector2 minifiedDrawSize = this.GetMinifiedDrawSize(this.InnerThing.def.size.ToVector2(), 1.1f);
                        Vector2 newDrawSize = new Vector2(minifiedDrawSize.x / (float)this.InnerThing.def.size.x * this.cachedGraphic.drawSize.x, minifiedDrawSize.y / (float)this.InnerThing.def.size.z * this.cachedGraphic.drawSize.y);
                        this.cachedGraphic = this.cachedGraphic.GetCopy(newDrawSize);
                    }
                }
                return this.cachedGraphic;
            }
        }

        // Token: 0x17000539 RID: 1337
        // (get) Token: 0x06002378 RID: 9080 RVA: 0x0010F49A File Offset: 0x0010D89A
        public override string LabelNoCount
        {
            get
            {
                return this.InnerThing.LabelNoCount;
            }
        }

        // Token: 0x1700053A RID: 1338
        // (get) Token: 0x06002379 RID: 9081 RVA: 0x0010F4A7 File Offset: 0x0010D8A7
        public override string DescriptionDetailed
        {
            get
            {
                return this.InnerThing.DescriptionDetailed;
            }
        }

        // Token: 0x1700053B RID: 1339
        // (get) Token: 0x0600237A RID: 9082 RVA: 0x0010F4B4 File Offset: 0x0010D8B4
        public override string DescriptionFlavor
        {
            get
            {
                return this.InnerThing.DescriptionFlavor;
            }
        }

        // Token: 0x0600237B RID: 9083 RVA: 0x0010F4C4 File Offset: 0x0010D8C4
        protected override void Tick()
        {
            if (this.InnerThing == null)
            {
            //    Log.Error("MinifiedThing with null InnerThing. Destroying.", false);
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            base.Tick();
            if (this.InnerThing.TryGetComp<CompXenoHatcher>() is CompXenoHatcher comp)
            {
                comp.CompTick();
                if (this.MapHeld != null && this.Map == null)
                {
                //    Log.Message("MinifiedThing being carried.", false);
                }
                else
                {
                    ticksTillInstall--;
                    if (ticksTillInstall==0)
                    {
                        Map map = this.Map;
                        IntVec3 c = this.Position;
                        this.InnerThing.SetFaction(null);
                        if (this.Spawned) this.DeSpawn();
                        try
                        {
                            GenPlace.TryPlaceThing(this.InnerThing, c, map, ThingPlaceMode.Direct);
                        }
                        catch (Exception)
                        {
                            CellFinder.TryFindRandomCellNear(c, map, 2, (x => GridsUtility.GetFirstBuilding(x, this.Map) == null), out c);
                            GenPlace.TryPlaceThing(this.InnerThing, c, map, ThingPlaceMode.Direct);
                            throw;
                        }
                        ticksTillInstall = 1000;
                    }
                //    
                //    Log.Message("MinifiedThing on the ground.", false);
                }
            }
        }
        
        // Token: 0x06002381 RID: 9089 RVA: 0x0010F610 File Offset: 0x0010DA10
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(this);
            if (blueprint_Install != null)
            {
                GenDraw.DrawLineBetween(this.TrueCenter(), blueprint_Install.TrueCenter());
            }
        }

        // Token: 0x06002382 RID: 9090 RVA: 0x0010F644 File Offset: 0x0010DA44
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (this.crateFrontGraphic == null)
            {
                this.crateFrontGraphic = GraphicDatabase.Get<Graphic_Single>("DummyTexture", ShaderDatabase.Cutout, this.GetMinifiedDrawSize(this.InnerThing.def.size.ToVector2(), 1.1f) * 1.16f, Color.white);
            }
            this.crateFrontGraphic.DrawFromDef(drawLoc + Altitudes.AltIncVect * 0.1f, Rot4.North, null, 0f);
            if (this.Graphic is Graphic_Single)
            {
                this.Graphic.Draw(drawLoc, Rot4.North, this, 0f);
            }
            else
            {
                this.Graphic.Draw(drawLoc, Rot4.South, this, 0f);
            }
        }
        
        // Token: 0x06002386 RID: 9094 RVA: 0x0010F7B8 File Offset: 0x0010DBB8
        public override string GetInspectString()
        {
            string text = "NotInstalled".Translate();
            string inspectString = this.InnerThing.GetInspectString();
            if (!inspectString.NullOrEmpty())
            {
                text += "\n";
                text += inspectString;
            }
            return text;
        }

        // Token: 0x06002387 RID: 9095 RVA: 0x0010F7FC File Offset: 0x0010DBFC
        private Vector2 GetMinifiedDrawSize(Vector2 drawSize, float maxSideLength)
        {
            float num = maxSideLength / Mathf.Max(drawSize.x, drawSize.y);
            if (num >= 1f)
            {
                return drawSize;
            }
            return drawSize * num;
        }

        // Token: 0x0400144A RID: 5194
        private const float MaxMinifiedGraphicSize = 1.1f;

        // Token: 0x0400144B RID: 5195
        private const float CrateToGraphicScale = 1.16f;

        private int ticksTillInstall = 1000;
        // Token: 0x0400144D RID: 5197
        private Graphic cachedGraphic;

        // Token: 0x0400144E RID: 5198
        private Graphic crateFrontGraphic;
    }
}
