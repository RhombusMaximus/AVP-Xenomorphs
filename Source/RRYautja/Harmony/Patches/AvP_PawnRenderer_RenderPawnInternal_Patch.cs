using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using RRYautja.settings;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    // Cocoon Draw Pos fix
    // Hediff_Implant Drawer
    // TODO(1.5+): PawnRenderer.RenderPawnInternal was completely overhauled in RimWorld 1.5+.
    // The method may have been renamed, its signature changed, or rendering moved to PawnRenderUtility.
    // The "pawn" private field may have been renamed — Traverse-based access may return null.
    // BaseHeadOffsetAt may have been renamed or moved.
    // All of these must be verified against the RimWorld 1.5+/1.6 decompiled assembly before re-enabling.
#if false // TODO(1.5+): Re-enable after verifying RenderPawnInternal method name, signature, and pawn field in RimWorld 1.5+
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    static class AvP_PawnRenderer_RenderPawnInternal_Patch
    {
        [HarmonyPrefix]
        static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, ref bool renderBody, ref Rot4 bodyFacing, ref Rot4 headFacing, ref RotDrawMode bodyDrawType, ref bool portrait, ref bool headStump, ref bool invisible)
        {
            // TODO(1.5+): The "pawn" private field may have been renamed in PawnRenderer. Handle null gracefully.
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null)
            {
                Log.Warning("AvP_PawnRenderer_RenderPawnInternal_Patch: Failed to get pawn from PawnRenderer via reflection. Field 'pawn' may have been renamed in 1.5+.");
                return;
            }
            //    bool selected = Find.Selector.SelectedObjects.Contains(pawn) && Prefs.DevMode;
            if (invisible)
            {
               return;
            }
            if (!portrait)
            {
                if (pawn.RaceProps.Humanlike && pawn.CurrentBed() != null && pawn.CurrentBed().GetType() == typeof(Building_XenomorphCocoon))
                {
                    //rootLoc.z += 1f;
                    //rootLoc.x += 1f;
                    if (pawn.CurrentBed().Rotation == Rot4.North)
                    {
                        //rootLoc.x += 0.5f;
                        rootLoc.z -= 0.5f;
                    }
                    else if (pawn.CurrentBed().Rotation == Rot4.South)
                    {
                        //rootLoc.x += 0.5f;
                        rootLoc.z += 0.5f;
                    }
                    else if (pawn.CurrentBed().Rotation == Rot4.East)
                    {
                        rootLoc.x -= 0.5f;
                        //rootLoc.z += 0.5f;
                    }
                    else if (pawn.CurrentBed().Rotation == Rot4.West)
                    {
                        rootLoc.x += 0.5f;
                        //rootLoc.z += 0.5f;
                    }
                    else rootLoc = pawn.CurrentBed().DrawPos;
                }
                bool pawnflag = !((pawn.kindDef.race.defName.StartsWith("Android") && pawn.kindDef.race.defName.Contains("Tier")) || pawn.kindDef.race.defName.Contains("ChjDroid") || pawn.kindDef.race.defName.Contains("ChjBattleDroid") || pawn.kindDef.race.defName.Contains("M7Mech"));
                if ((pawn.RaceProps.Humanlike && pawnflag) || pawn.kindDef.race.GetModExtension<OffsetDefExtension>() != null)
                {
                    foreach (var hd in pawn.health.hediffSet.hediffs)
                    {
                        HediffComp_DrawImplant comp = hd.TryGetComp<HediffComp_DrawImplant>();
                        if (comp != null)
                        {
                            DrawImplant(comp, __instance, rootLoc, angle, renderBody, bodyFacing, headFacing, bodyDrawType, portrait, headStump);
                        }
                    }
                    /*
                    */
                } // DrawWornExtras()
                else
                {
                    foreach (var hd in pawn.health.hediffSet.hediffs)
                    {
                        HediffComp_DrawImplant comp = hd.TryGetComp<HediffComp_DrawImplant>();
                        if (comp != null)
                        {
                            comp.DrawWornExtras();
                        }
                    }
                }
            }
        }

        static void DrawImplant(HediffComp_DrawImplant comp, PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {// this.Pawn
            // TODO(1.5+): The "pawn" private field may have been renamed in PawnRenderer. Handle null gracefully.
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null)
            {
                Log.Warning("AvP_PawnRenderer_RenderPawnInternal_Patch.DrawImplant: Failed to get pawn from PawnRenderer via reflection. Field 'pawn' may have been renamed in 1.5+.");
                return;
            }
            // TODO(1.5+): BaseHeadOffsetAt may have been renamed or moved in RimWorld 1.5+. Verify before re-enabling.
            bool selected = Find.Selector.SelectedObjects.Contains(pawn) && Prefs.DevMode;
            Rot4 rot = bodyFacing;
            Vector3 vector3 = pawn.RaceProps.Humanlike ? __instance.BaseHeadOffsetAt(headFacing) : new Vector3();
            Vector3 s = new Vector3(pawn.BodySize * 1.75f, pawn.BodySize * 1.75f, pawn.BodySize * 1.75f);
            GetAltitudeOffset(pawn, comp.parent, rot, out float X, out float Y, out float Z, out float DsX, out float DsZ, out float ang);
            vector3.x += X;
            vector3.y += Y;
            vector3.z += Z;
            angle += ang;
            s.x = DsX;
            s.z = DsZ;
            if (pawn.RaceProps.Humanlike)
            {
                vector3.x += 0.01f;
                vector3.z += -0.35f;
            }

            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 b = quaternion * vector3;
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.02734375f;
                vector.y += 0.0234375f;
            }
            else
            {
                a.y += 0.0234375f;
                vector.y += 0.02734375f;
            }
            /*
            Material material = __instance.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
            if (material != null)
            {
                Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                GenDraw.DrawMeshNowOrLater(mesh2, a + b, quaternion, material, portrait);
            }
            */
            Vector3 loc2 = rootLoc + b;
            loc2.y += 0.03105f;
            bool flag = false;
            // NOTE: The commented-out block referencing hatRenderedFrontOfFace was removed.
            // hatRenderedFrontOfFace was REMOVED from ApparelProperties in RimWorld 1.5+.
            // It has been replaced by renderSkipFlags / drawData in the new rendering system.
            if (!flag && bodyDrawType != RotDrawMode.Dessicated)
            {
#if DEBUG
                if (selected)
                {
                //    Log.Message(string.Format("{0}'s rootLoc: {1}, angle: {2}, renderBody: {3}, bodyFacing: {4}, headFacing: {5}, bodyDrawType: {6}, portrait: {7}", pawn.Label, rootLoc, angle, renderBody, bodyFacing.ToStringHuman(), headFacing.ToStringHuman(), bodyDrawType, portrait));
                }
#endif
                if (!portrait)
                {

                    //    Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    Material mat = comp.ImplantMaterial(pawn, pawn.RaceProps.Humanlike ? headFacing : bodyFacing);
                    //    GenDraw.DrawMeshNowOrLater(headFacing == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, loc2, quaternion, mat, true);
                    Matrix4x4 matrix = default(Matrix4x4);
                    matrix.SetTRS(loc2, quaternion, s);
                    Graphics.DrawMesh((pawn.RaceProps.Humanlike ? headFacing : bodyFacing) == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix, mat, 0);
                    //    Graphics.DrawMesh((pawn.RaceProps.Humanlike ? headFacing : bodyFacing) == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix, mat, 0);
                }
            }

            /*
            Material matSingle = comp.ImplantMaterial(pawn, rot);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(rot == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix, matSingle, 0);
            */
        }

        static void GetAltitudeOffset(Pawn pawn, Hediff hediff, Rot4 rotation, out float OffsetX, out float OffsetY, out float OffsetZ, out float DrawSizeX, out float DrawSizeZ, out float ang)
        {
            OffsetDefExtension myDef = null;
            if (!pawn.def.modExtensions.NullOrEmpty())
            {
                if (pawn.def.HasModExtension<RRYautja.OffsetDefExtension>())
                {
                    List<DefModExtension> list = pawn.kindDef.race.modExtensions.Where((x) => x.GetType() == typeof(RRYautja.OffsetDefExtension)).ToList();
                    if (list.Any((x) => hediff.def.defName.Contains(((RRYautja.OffsetDefExtension)x).hediff.defName)))
                    {
                        myDef = (OffsetDefExtension)list.First((x) => hediff.def.defName.Contains(((OffsetDefExtension)x).hediff.defName)) ?? null;
                    }
                    else
                    {
                        if (ThingDefOf.Human.modExtensions.Any((x) => hediff.def.defName.Contains(((OffsetDefExtension)x).hediff.defName)))
                        {
                            myDef = (OffsetDefExtension)ThingDefOf.Human.modExtensions.First((x) => hediff.def.defName.Contains(((OffsetDefExtension)x).hediff.defName));
                        }
                        else
                        {
                            myDef = new OffsetDefExtension();
                        }
                    }
                }
                
            }
            if (myDef == null)
            {
                if (!ThingDefOf.Human.modExtensions.NullOrEmpty())
                {
                    myDef = new OffsetDefExtension() { hediff = hediff.def };
                }
                else
                if (ThingDefOf.Human.modExtensions.Any((x) => hediff.def.defName.Contains(((OffsetDefExtension)x).hediff.defName)))
                {
                    myDef = (OffsetDefExtension)ThingDefOf.Human.modExtensions.First((x) => hediff.def.defName.Contains(((OffsetDefExtension)x).hediff.defName)) ?? new OffsetDefExtension() { hediff = hediff.def };
                }
                else
                {
                    myDef = new OffsetDefExtension() { hediff = hediff.def };
                }
            }
            else
            {
            //    Log.Message("else");
            //    myDef = new OffsetDefExtension() { hediff = hediff.def };

            }


            if (myDef.hediff != null)
            {
                //    Log.Message(string.Format("{0}'s drawdata for hediff {1} OffsetDefExtension.hediff {2}", pawn.LabelShortCap, hediff.LabelCap, myDef.hediff.label));
            }
            if (pawn.RaceProps.Humanlike)
            {
                if (rotation == Rot4.North)
                {
                    OffsetX = myDef.NorthXOffset;
                    OffsetY = myDef.NorthYOffset;
                    OffsetZ = myDef.NorthZOffset;
                    DrawSizeX = myDef.NorthXDrawSize;
                    DrawSizeZ = myDef.NorthZDrawSize;
                    ang = myDef.NorthAngle;
                }
                else if (rotation == Rot4.West)
                {
                    OffsetX = myDef.WestXOffset;
                    OffsetY = myDef.WestYOffset;
                    OffsetZ = myDef.WestZOffset;
                    DrawSizeX = myDef.WestXDrawSize;
                    DrawSizeZ = myDef.WestZDrawSize;
                    ang = myDef.WestAngle;
                }
                else if (rotation == Rot4.East)
                {
                    OffsetX = myDef.EastXOffset;
                    OffsetY = myDef.EastYOffset;
                    OffsetZ = myDef.EastZOffset;
                    DrawSizeX = myDef.EastXDrawSize;
                    DrawSizeZ = myDef.EastZDrawSize;
                    ang = myDef.EastAngle;
                }
                else if (rotation == Rot4.South)
                {
                    OffsetX = myDef.SouthXOffset;
                    OffsetY = myDef.SouthYOffset;
                    OffsetZ = myDef.SouthZOffset;
                    DrawSizeX = myDef.SouthXDrawSize;
                    DrawSizeZ = myDef.SouthZDrawSize;
                    ang = myDef.SouthAngle;
                }
                else
                {
                    OffsetX = 0f;
                    OffsetY = 0f;
                    OffsetZ = 0f;
                    DrawSizeX = 1f;
                    DrawSizeZ = 1f;
                    ang = 0f;
                }
                if (myDef.ApplyBaseHeadOffset)
                {
                    OffsetX = myDef.SouthXOffset + pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).x;
                    OffsetY = myDef.SouthYOffset + pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).y;
                    OffsetZ = myDef.SouthZOffset + pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).z;
                }
            }
            else
            {
                if (rotation == Rot4.North)
                {
                    OffsetX = myDef.NorthXOffset;
                    OffsetY = myDef.NorthYOffset;
                    OffsetZ = myDef.NorthZOffset;
                    DrawSizeX = myDef.NorthXDrawSize;
                    DrawSizeZ = myDef.NorthZDrawSize;
                    ang = myDef.NorthAngle;
                }
                else if (rotation == Rot4.West)
                {
                    OffsetX = myDef.WestXOffset;
                    OffsetY = myDef.WestYOffset;
                    OffsetZ = myDef.WestZOffset;
                    DrawSizeX = myDef.WestXDrawSize;
                    DrawSizeZ = myDef.WestZDrawSize;
                    ang = myDef.WestAngle;
                }
                else if (rotation == Rot4.East)
                {
                    OffsetX = myDef.EastXOffset;
                    OffsetY = myDef.EastYOffset;
                    OffsetZ = myDef.EastZOffset;
                    DrawSizeX = myDef.EastXDrawSize;
                    DrawSizeZ = myDef.EastZDrawSize;
                    ang = myDef.EastAngle;
                }
                else if (rotation == Rot4.South)
                {
                    OffsetX = myDef.SouthXOffset;
                    OffsetY = myDef.SouthYOffset;
                    OffsetZ = myDef.SouthZOffset;
                    DrawSizeX = myDef.SouthXDrawSize;
                    DrawSizeZ = myDef.SouthZDrawSize;
                    ang = myDef.SouthAngle;
                }
                else
                {
                    OffsetX = 0f;
                    OffsetY = 0f;
                    OffsetZ = 0f;
                    DrawSizeX = 1f;
                    DrawSizeZ = 1f;
                    ang = 0f;
                }
            }
        }
    }
#endif // TODO(1.5+): Re-enable after verifying RenderPawnInternal method name, signature, and pawn field in RimWorld 1.5+

}