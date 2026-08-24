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
    /// </summary>
    [StaticConstructorOnStartup]
    static class FacehuggerMaskRenderer
    {
        private static Graphic facehuggerMaskGraphic;
        private static Graphic royalFacehuggerMaskGraphic;
        private static bool initialized = false;
        private static int lastDebugLogTick = -1;
        private static int debugLogCount = 0;

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

            if (!pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)) return;

            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection);
            if (hediff == null) return;

            var facehuggerComp = hediff.TryGetComp<HediffComp_XenoFacehugger>();
            if (facehuggerComp == null) return;

            // Debug logging — rate limited via AvPDebug
            AvPDebug.Log("Mask", "Drawing mask for " + pawn.LabelShort + " (royal=" + facehuggerComp.RoyaleHugger + ")");

            Graphic maskGraphic = facehuggerComp.RoyaleHugger ? royalFacehuggerMaskGraphic : facehuggerMaskGraphic;
            if (maskGraphic == null) return;

            Vector3 pos = drawLoc;
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.VisEffects);

            float num = Mathf.Lerp(1.2f, 1.55f, pawn.BodySize);

            if (!pawn.RaceProps.Humanlike)
            {
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(pos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, maskGraphic.MatAt(Rot4.South), 0);
            }
            else
            {
                pos += new Vector3(0f, 0f, 0.15f);
                Vector3 s = new Vector3(0.9f, 1f, 0.9f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(pos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, maskGraphic.MatAt(pawn.Rotation), 0);
            }
        }
    }
}