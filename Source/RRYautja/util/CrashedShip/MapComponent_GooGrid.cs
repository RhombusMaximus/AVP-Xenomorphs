using RimWorld;
using RRYautja.ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static RRYautja.XenomorphHiveUtility;

namespace RRYautja
{
    // Token: 0x02000067 RID: 103
    public class MapComponent_GooGrid : MapComponent
    {
        // Token: 0x06000217 RID: 535 RVA: 0x0000B430 File Offset: 0x00009630
        public MapComponent_GooGrid(Map map) : base(map)
        {
            this.map = map;
            this.depthGrid = new float[map.cellIndices.NumGridCells];
        }
        
        internal float[] DepthGridDirect_Unsafe
        {
            get
            {
                return this.depthGrid;
            }
        }

        public float TotalDepth
        {
            get
            {
                return (float)this.totalDepth;
            }
        }
        
        private static ushort GooFloatToShort(float depth)
        {
            depth = Mathf.Clamp(depth, 0f, 1f);
            depth *= 65535f;
            return (ushort)Mathf.RoundToInt(depth);
        }

        private static float GoohortToFloat(ushort depth)
        {
            return (float)depth / 65535f;
        }

        private bool CanHaveGoo(int ind)
        {
            Building building = this.map.edificeGrid[ind];
            if (building != null && !MapComponent_GooGrid.CanCoexistWithGoo(building.def))
            {
                return false;
            }
            TerrainDef terrainDef = this.map.terrainGrid.TerrainAt(ind);
            return terrainDef.passability != Traversability.Impassable;// terrainDef == null || terrainDef.holdSnow;
        }

        public static bool CanCoexistWithGoo(ThingDef def)
        {
            return def.category != ThingCategory.Building || def.Fillage != FillCategory.Full || def == XenomorphDefOf.RRY_XenomorphCrashedShipPart;
        }

        public void AddDepth(IntVec3 c, float depthToAdd)
        {
            int num = this.map.cellIndices.CellToIndex(c);
            float num2 = this.depthGrid[num];
            if (num2 <= 0f && depthToAdd < 0f)
            {
                return;
            }
            if (num2 >= 0.999f && depthToAdd > 1f)
            {
                return;
            }
            if (!this.CanHaveGoo(num))
            {
                this.depthGrid[num] = 0f;
                return;
            }
            float num3 = num2 + depthToAdd;
            num3 = Mathf.Clamp(num3, 0f, 1f);
            float num4 = num3 - num2;
            this.totalDepth += (double)num4;
            if (Mathf.Abs(num4) > 0.0001f)
            {
                this.depthGrid[num] = num3;
                this.CheckVisualOrPathCostChange(c, num2, num3);
            }
        }

        public void SetDepth(IntVec3 c, float newDepth)
        {
            int num = this.map.cellIndices.CellToIndex(c);
            if (!this.CanHaveGoo(num))
            {
                this.depthGrid[num] = 0f;
                return;
            }
            newDepth = Mathf.Clamp(newDepth, 0f, 1f);
            float num2 = this.depthGrid[num];
            this.depthGrid[num] = newDepth;
            float num3 = newDepth - num2;
            this.totalDepth += (double)num3;
            this.CheckVisualOrPathCostChange(c, num2, newDepth);
        }

        private void CheckVisualOrPathCostChange(IntVec3 c, float oldDepth, float newDepth)
        {
            if (!Mathf.Approximately(oldDepth, newDepth))
            {
                if (Mathf.Abs(oldDepth - newDepth) > 0.15f || Rand.Value < 0.0125f)
                {
                    this.map.mapDrawer.MapMeshDirty(c, RRYMapMeshFlagDefOf.Goo, true, false);
                    this.map.mapDrawer.MapMeshDirty(c, RRYMapMeshFlagDefOf.Goo, true, false);
                }
                else if (newDepth == 0f)
                {
                    this.map.mapDrawer.MapMeshDirty(c, RRYMapMeshFlagDefOf.Goo, true, false);
                }
                if (XenomorphHiveUtility.GetGooCategory(oldDepth) != XenomorphHiveUtility.GetGooCategory(newDepth))
                {
                    this.map.pathGrid.RecalculatePerceivedPathCostAt(c);
                }
            }
        }

        public float GetDepth(IntVec3 c)
        {
            if (!c.InBounds(this.map))
            {
                return 0f;
            }
            return this.depthGrid[this.map.cellIndices.CellToIndex(c)];
        }

        public GooCategory GetCategory(IntVec3 c)
        {
            return XenomorphHiveUtility.GetGooCategory(this.GetDepth(c));
        }

        public override void ExposeData()
        {
            MapExposeUtility.ExposeUshort(this.map, (IntVec3 c) => MapComponent_GooGrid.GooFloatToShort(this.GetDepth(c)), delegate (IntVec3 c, ushort val)
            {
                this.depthGrid[this.map.cellIndices.CellToIndex(c)] = MapComponent_GooGrid.GoohortToFloat(val);
            }, "depthGrid");
            base.ExposeData();
        }
        
        public MapComponent_GooGrid HiveGrid;

        private float[] depthGrid;

        private double totalDepth;

        public const float MaxDepth = 1f;

    }
}
