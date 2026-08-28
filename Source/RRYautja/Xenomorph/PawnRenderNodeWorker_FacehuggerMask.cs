using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Custom render node worker for facehugger mask.
    /// Shows royal facehugger mask texture when the hediff comp has royaleHugger=true.
    /// Otherwise shows the normal facehugger mask texture (from texPath in XML).
    /// </summary>
    public class PawnRenderNodeWorker_FacehuggerMask : PawnRenderNodeWorker_Hediff
    {
        private static Graphic royalGraphic;

        private static Graphic RoyalGraphic
        {
            get
            {
                if (royalGraphic == null)
                {
                    royalGraphic = GraphicDatabase.Get<Graphic_Multi>(
                        "Things/Pawn/Xenomorph/Xenomorph_FaceHuggerRoyal_Mask",
                        ShaderDatabase.CutoutComplex,
                        new Vector2(1.15f, 1.15f),
                        Color.white);
                }
                return royalGraphic;
            }
        }

        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            try
            {
                // node.hediff gives direct access to the Hediff object
                if (node.hediff != null)
                {
                    var comp = node.hediff.TryGetComp<HediffComp_XenoFacehugger>();
                    if (comp != null && comp.RoyaleHugger)
                    {
                        return RoyalGraphic;
                    }
                }
            }
            catch { }
            return base.GetGraphic(node, parms);
        }
    }
}