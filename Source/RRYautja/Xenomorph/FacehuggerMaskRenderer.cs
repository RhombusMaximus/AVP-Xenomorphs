using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    /// <summary>
    /// Draws facehugger mask overlay on pawns that have the FaceHuggerInfection hediff.
    /// In RimWorld 1.1 this was handled by HediffComp_DrawImplant.DrawWornExtras.
    /// In 1.6 the rendering system changed to PawnRenderTree, so we patch
    /// Pawn_DrawTracker.DrawGenes instead and draw the mask manually.
    /// </summary>
    [StaticConstructorOnStartup]
    static class FacehuggerMaskRenderer
    {
        private static Graphic facehuggerMaskGraphic;
        private static Graphic royalFacehuggerMaskGraphic;

        static FacehuggerMaskRenderer()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.facehuggermask");

                // Patch PawnRenderer.RenderPawnInternal to draw mask after pawn renders
                var renderMethod = AccessTools.Method("RimWorld.PawnRenderer:RenderPawnInternal");
                if (renderMethod != null)
                {
                    harmony.Patch(renderMethod, postfix: new HarmonyMethod(typeof(FacehuggerMaskRenderer), nameof(RenderPawnInternalPostfix)));
                    Log.Message("[AVP Xenomorphs] Patched PawnRenderer.RenderPawnInternal for facehugger mask");
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] PawnRenderer.RenderPawnInternal not found");
                }

                // Load mask graphics
                facehuggerMaskGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    "Things/Pawn/Xenomorph/Xenomorph_FaceHugger_Mask",
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);

                royalFacehuggerMaskGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    "Things/Pawn/Xenomorph/Xenomorph_FaceHuggerRoyal_Mask",
                    ShaderDatabase.Cutout,
                    Vector2.one,
                    Color.white);
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init facehugger mask renderer: " + e.Message);
            }
        }

        public static void RenderPawnInternalPostfix(PawnRenderer __instance)
        {
            // Get the pawn from the renderer
            Pawn pawn = __instance.GetType().GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(__instance) as Pawn;
            if (pawn == null) return;
            TryDrawMask(pawn);
        }

        /// <summary>
        /// Draw the facehugger mask on a pawn if they have the infection.
        /// Call this from a MapComponent or patched draw method.
        /// </summary>
        public static void TryDrawMask(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Dead) return;

            // Check if pawn has facehugger infection
            if (!pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)) return;

            // Get the facehugger comp
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection);
            if (hediff == null) return;

            var facehuggerComp = hediff.TryGetComp<HediffComp_XenoFacehugger>();
            if (facehuggerComp == null) return;

            // Select the right mask
            Graphic maskGraphic = facehuggerComp.RoyaleHugger ? royalFacehuggerMaskGraphic : facehuggerMaskGraphic;
            if (maskGraphic == null) return;

            // Draw the mask on the pawn's head
            Vector3 drawPos = pawn.Drawer.DrawPos;
            drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.VisEffects);

            // Scale based on body size
            float num = Mathf.Lerp(1.2f, 1.55f, pawn.BodySize);

            // For non-humanlike, just draw at the pawn position
            if (!pawn.RaceProps.Humanlike)
            {
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, maskGraphic.MatAt(Rot4.South), 0);
            }
            else
            {
                // For humanlike, draw at head position
                drawPos += new Vector3(0f, 0f, 0.15f); // Offset slightly up toward head
                Vector3 s = new Vector3(0.9f, 1f, 0.9f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, maskGraphic.MatAt(pawn.Rotation), 0);
            }
        }
    }
}