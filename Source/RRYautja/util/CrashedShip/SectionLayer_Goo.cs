using System;
using UnityEngine;
using Verse;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    // Token: 0x02000CA1 RID: 3233
    internal class SectionLayer_Goo : SectionLayer
    {
        public SectionLayer_Goo(Section section) : base(section)
        {
            this.relevantChangeTypes = RRYMapMeshFlagDefOf.Goo;
            this.GooMaterial.mainTexture = this.GooTex;
        }
        
        public override bool Visible
        {
            get
            {
                return DebugViewSettings.drawSnow;
            }
        }
        
        private bool Filled(int index)
        {
            Building building = base.Map.edificeGrid[index];
            return building != null && building.def.Fillage == FillCategory.Full;
        }
        /*
        public override void DrawLayer()
        {
            if (!this.Visible)
            {
                return;
            }
            int count = this.subMeshes.Count;
            for (int i = 0; i < count; i++)
            {
                LayerSubMesh layerSubMesh = this.subMeshes[i];
                Vector3 s = new Vector3(.28f, 1f, .28f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                if (layerSubMesh.finalized && !layerSubMesh.disabled)
                {
                    Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zero, Quaternion.identity, layerSubMesh.material, 0);
                }
            }
        }
        */
    
        public override void Regenerate()
        {
            LayerSubMesh subMesh = base.GetSubMesh(MatBases.Snow);
            if (subMesh.mesh.vertexCount == 0)
            {
                SectionLayerGeometryMaker_Solid.MakeBaseGeometry(this.section, subMesh, AltitudeLayer.Terrain);
            }
            subMesh.Clear(MeshParts.Colors);
            MapComponent_GooGrid _AvPHiveCreep = base.Map.GetComponent<MapComponent_GooGrid>();
            //    Log.Message(string.Format(" 6 {0}", _AvPHiveCreep.DepthGridDirect_Unsafe));
            float[] depthGridDirect_Unsafe = _AvPHiveCreep.DepthGridDirect_Unsafe;
            CellRect cellRect = this.section.CellRect;
            int num = base.Map.Size.z - 1;
            int num2 = base.Map.Size.x - 1;
            bool flag = false;
            CellIndices cellIndices = base.Map.cellIndices;
            for (int i = cellRect.minX; i <= cellRect.maxX; i++)
            {
                for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                {
                    float num3 = depthGridDirect_Unsafe[cellIndices.CellToIndex(i, j)];
                    int num4 = cellIndices.CellToIndex(i, j - 1);
                    float num5 = (j <= 0) ? num3 : depthGridDirect_Unsafe[num4];
                    num4 = cellIndices.CellToIndex(i - 1, j - 1);
                    float num6 = (j <= 0 || i <= 0) ? num3 : depthGridDirect_Unsafe[num4];
                    num4 = cellIndices.CellToIndex(i - 1, j);
                    float num7 = (i <= 0) ? num3 : depthGridDirect_Unsafe[num4];
                    num4 = cellIndices.CellToIndex(i - 1, j + 1);
                    float num8 = (j >= num || i <= 0) ? num3 : depthGridDirect_Unsafe[num4];
                    num4 = cellIndices.CellToIndex(i, j + 1);
                    float num9 = (j >= num) ? num3 : depthGridDirect_Unsafe[num4];
                    num4 = cellIndices.CellToIndex(i + 1, j + 1);
                    float num10 = (j >= num || i >= num2) ? num3 : depthGridDirect_Unsafe[num4];
                    num4 = cellIndices.CellToIndex(i + 1, j);
                    float num11 = (i >= num2) ? num3 : depthGridDirect_Unsafe[num4];
                    num4 = cellIndices.CellToIndex(i + 1, j - 1);
                    float num12 = (j <= 0 || i >= num2) ? num3 : depthGridDirect_Unsafe[num4];
                    this.vertDepth[0] = (num5 + num6 + num7 + num3) / 4f;
                    this.vertDepth[1] = (num7 + num3) / 2f;
                    this.vertDepth[2] = (num7 + num8 + num9 + num3) / 4f;
                    this.vertDepth[3] = (num9 + num3) / 2f;
                    this.vertDepth[4] = (num9 + num10 + num11 + num3) / 4f;
                    this.vertDepth[5] = (num11 + num3) / 2f;
                    this.vertDepth[6] = (num11 + num12 + num5 + num3) / 4f;
                    this.vertDepth[7] = (num5 + num3) / 2f;
                    this.vertDepth[8] = num3;
                    for (int k = 0; k < 9; k++)
                    {
                        if (this.vertDepth[k] > 0.01f)
                        {
                            ;
                            flag = true;
                        }
                        subMesh.colors.Add(SectionLayer_Goo.GooDepthColor(this.vertDepth[k]));
                    }
                }
            }
            if (flag)
            {
                subMesh.disabled = false;
                subMesh.FinalizeMesh(MeshParts.Colors);
            }
            else
            {
                subMesh.disabled = true;
            }
        }
        
        private static Color32 GooDepthColor(float snowDepth)
        {
            return Color32.Lerp(SectionLayer_Goo.ColorClear, SectionLayer_Goo.ColorFull, snowDepth);
        }
        
        private float[] vertDepth = new float[9];

        public Material GooMaterial = new Material(MatBases.Snow);
        
        public Texture2D GooTex = ContentFinder<Texture2D>.Get("Other/HiveMat", true);
        
        private static readonly Color32 ColorClear = new Color32(75, 75, 75, 0);
        
        private static readonly Color32 ColorFull = new Color32(40, 25, 90, 75);
    }
}
