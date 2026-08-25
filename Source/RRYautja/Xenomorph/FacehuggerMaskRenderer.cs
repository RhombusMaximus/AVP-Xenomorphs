using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using RRYautja.ExtensionMethods;
using RRYautja.settings;

namespace RRYautja
{
    /// <summary>
    /// Draws facehugger mask overlay on pawns that have the FaceHuggerInfection hediff.
    /// Uses GenDraw.DrawMeshNowOrLater to render in the map rendering pipeline.
    /// </summary>
    [StaticConstructorOnStartup]
    static class FacehuggerMaskRenderer
    {
        private static Graphic facehuggerMaskGraphic;
        private static Graphic royalFacehuggerMaskGraphic;
        private static bool initialized = false;

        static FacehuggerMaskRenderer()
        {
            try
            {
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

                if (facehuggerMaskGraphic != null)
                    AvPDebug.LogOnce("MaskInit", "[AVP Xenomorphs] Facehugger mask graphic loaded: " + facehuggerMaskGraphic.path);
                else
                    AvPDebug.Error("Facehugger mask graphic failed to load!");

                // Patch Pawn.DrawAt — this is what we know works (log messages appeared before)
                var drawMethod = AccessTools.Method(typeof(Pawn), "DrawAt");
                if (drawMethod != null)
                {
                    var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.facehuggermask");
                    harmony.Patch(drawMethod, postfix: new HarmonyMethod(typeof(FacehuggerMaskRenderer), nameof(DrawAtPostfix)));
                    AvPDebug.LogOnce("Patch", "[AVP Xenomorphs] Patched Pawn.DrawAt for facehugger mask");
                    initialized = true;
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] Pawn.DrawAt not found, facehugger mask will not render");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init facehugger mask renderer: " + e.Message);
            }
        }

        public static void DrawAtPostfix(Pawn __instance, Vector3 drawLoc, bool flip)
        {
            if (!initialized) return;
            if (__instance == null || !__instance.Spawned || __instance.Dead) return;
            TryDrawMask(__instance, drawLoc);
        }

        public static void TryDrawMask(Pawn pawn, Vector3 drawLoc)
        {
            if (pawn == null || !pawn.Spawned || pawn.Dead) return;
            // Only draw manually for non-humanlike pawns (humanlike use apparel)
            if (pawn.RaceProps.Humanlike) return;

            if (!pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)) return;

            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection);
            if (hediff == null) return;

            var facehuggerComp = hediff.TryGetComp<HediffComp_XenoFacehugger>();
            if (facehuggerComp == null) return;

            AvPDebug.Log("Mask", "Drawing mask for " + pawn.LabelShort + " (royal=" + facehuggerComp.RoyaleHugger + ")");

            Graphic maskGraphic = facehuggerComp.RoyaleHugger ? royalFacehuggerMaskGraphic : facehuggerMaskGraphic;
            if (maskGraphic == null) return;

            // Use GenDraw.DrawMeshNowOrLater — renders in the map camera pipeline
            Vector3 pos = drawLoc;
            pos.y += 0.03f; // Slightly above pawn body

            float scale = pawn.RaceProps.Humanlike ? 1.0f : Mathf.Lerp(1.2f, 1.55f, pawn.BodySize);
            Vector3 drawSize = new Vector3(scale, 1f, scale);

            Material mat = maskGraphic.MatAt(pawn.Rotation);
            if (mat == null) return;

            // Use GenDraw.DrawMeshNowOrLater with drawNow=true to render immediately
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, drawSize);
            GenDraw.DrawMeshNowOrLater(MeshPool.plane10, matrix, mat, true);
        }
    }
}