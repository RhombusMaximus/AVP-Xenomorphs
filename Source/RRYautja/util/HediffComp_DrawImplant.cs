using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RRYautja
{
    public class HediffCompProperties_DrawImplant : HediffCompProperties
    {
        public ImplantDrawerType implantDrawerType;

        public string implantGraphicPath;
    }

    [StaticConstructorOnStartup]
    public class HediffComp_DrawImplant : HediffComp
    {
        public HediffCompProperties_DrawImplant implantDrawProps
        {
            get
            {
                return this.props as HediffCompProperties_DrawImplant;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                Pawn wearer = base.Pawn;
                return wearer.Spawned && !wearer.Dead && wearer.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection);
            }
        }

        public Material ImplantMaterial(Pawn pawn, Rot4 bodyFacing)
        {
            string path;
            HediffComp_XenoFacehugger _XenoFacehugger = parent.TryGetComp<HediffComp_XenoFacehugger>();
            if (_XenoFacehugger!=null)
            {
                
                if (this.implantDrawProps.implantDrawerType == ImplantDrawerType.Head)
                {
                    path = _XenoFacehugger.TexPath;
                }
                else
                {
                    path = _XenoFacehugger.TexPath + "_" + pawn.story.bodyType.ToString();
                }
            }
            else // if (!implantDrawProps.implantGraphicPath.NullOrEmpty())
            {
                if (this.implantDrawProps.implantDrawerType == ImplantDrawerType.Head)
                {
                    path = implantDrawProps.implantGraphicPath;
                }
                else
                {
                    path = implantDrawProps.implantGraphicPath + "_" + pawn.story.bodyType.ToString();
                }
            }
            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, Vector2.one, Color.white).MatAt(bodyFacing);
        }
        
        // Token: 0x0600005E RID: 94 RVA: 0x00004008 File Offset: 0x00002208
        public virtual void DrawWornExtras()
        {
            if (this.ShouldDisplay && !base.Pawn.RaceProps.Humanlike)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, base.Pawn.BodySize);
                Vector3 vector = base.Pawn.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.VisEffects);
                float angle = 0f;// (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }

        }
        private static readonly Material BubbleMat = MaterialPool.MatFrom("UI/FacehuggerInfectionOverlay", ShaderDatabase.Transparent);

    }


    public enum ImplantDrawerType
    {
        Undefined,
        Backpack,
        Head
    }

}
