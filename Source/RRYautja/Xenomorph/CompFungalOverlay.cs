using System;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x0200024D RID: 589
    public class CompProperties_FungalOverlay : CompProperties
    {
        // Token: 0x06000AAF RID: 2735 RVA: 0x00055935 File Offset: 0x00053D35
        public CompProperties_FungalOverlay()
        {
            this.compClass = typeof(CompFungalOverlay);
        }

        public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
        {
            Graphic graphic = GhostUtility.GhostGraphicFor(CompFireOverlay.FireGraphic, thingDef, ghostCol);
            graphic.DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef, 0f);
        }
        
        // Token: 0x040004A9 RID: 1193
        public float fireSize = 1f;

        // Token: 0x040004AA RID: 1194
        public Vector3 offset;
    }

    // Token: 0x0200073A RID: 1850
    [StaticConstructorOnStartup]
    public class CompFungalOverlay : ThingComp
    {
        // Token: 0x17000623 RID: 1571
        // (get) Token: 0x060028B9 RID: 10425 RVA: 0x00135B3C File Offset: 0x00133F3C
        public CompProperties_FireOverlay Props
        {
            get
            {
                return (CompProperties_FireOverlay)this.props;
            }
        }

        // Token: 0x060028BA RID: 10426 RVA: 0x00135B4C File Offset: 0x00133F4C
        public override void PostDynamicDrawPhase()
        {
            base.PostDynamicDrawPhase();
            Vector3 drawPos = this.parent.DrawPos;
            drawPos.y += 0.046875f;
            CompFungalOverlay.FireGraphic.Draw(drawPos, Rot4.North, this.parent, 0f);
        }

        // Token: 0x060028BB RID: 10427 RVA: 0x00135BB5 File Offset: 0x00133FB5
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }
        
        // Token: 0x040016B6 RID: 5814
        public static readonly Graphic FireGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Projectile/Flamer", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
    }
}
