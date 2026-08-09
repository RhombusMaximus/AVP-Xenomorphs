using System;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000003 RID: 3
    [StaticConstructorOnStartup]
    internal class Gizmo_InjectorStatus : Gizmo
    {
        // Token: 0x06000003 RID: 3 RVA: 0x00002069 File Offset: 0x00000269
        public Gizmo_InjectorStatus()
        {
            this.Order = -100f;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002080 File Offset: 0x00000280
        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002098 File Offset: 0x00000298
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(1523289473, overRect, 0, delegate ()
            {
                Rect rect = GenUI.ContractedBy(GenUI.AtZero(overRect), 6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = 0;
                Widgets.Label(rect2, Translator.Translate("HealthShardUses"));
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float num = (float)this.kit.uses / (float)this.kit.kitComp.Props.Uses;
                Widgets.FillableBar(rect3, num, Gizmo_InjectorStatus.FullBarTex, Gizmo_InjectorStatus.EmptyBarTex, false);
                Text.Font = (GameFont)1;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, this.kit.uses.ToString("F0") + " / " + this.kit.kitComp.Props.Uses.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(0);
        }

        // Token: 0x04000001 RID: 1
        public Cloakgen kit;

        // Token: 0x04000002 RID: 2
        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.0f, 0.1f));

        // Token: 0x04000003 RID: 3
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

        // Token: 0x04000004 RID: 4
        private static readonly Texture2D TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated", true);

        // Token: 0x04000005 RID: 5
        private const float ArrowScale = 0.5f;
    }
}
