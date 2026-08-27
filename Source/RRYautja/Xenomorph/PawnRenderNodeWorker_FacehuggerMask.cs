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
    /// Patches PawnRenderTree.Draw and PawnRenderNode.TryGetAnimationOffset
    /// to offset the facehugger mask down to face level.
    /// </summary>
    [StaticConstructorOnStartup]
    static class FacehuggerMaskOffsetPatch
    {
        public static Pawn lastMaskPawn = null;

        static FacehuggerMaskOffsetPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.maskoffset");

                // Patch PawnRenderTree.Draw — this is the main draw method
                var drawMethod = AccessTools.Method(typeof(PawnRenderTree), "Draw");
                if (drawMethod != null)
                {
                    harmony.Patch(drawMethod, prefix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(DrawPrefix)), postfix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(DrawPostfix)));
                    AvPDebug.LogOnce("MaskOffset", "[AVP Xenomorphs] Patched PawnRenderTree.Draw for mask offset");
                }

                // Patch PawnRenderNode.TryGetAnimationOffset
                var offsetMethod = AccessTools.Method(typeof(PawnRenderNode), "TryGetAnimationOffset");
                if (offsetMethod != null)
                {
                    harmony.Patch(offsetMethod, postfix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(TryGetAnimationOffsetPostfix)));
                    AvPDebug.LogOnce("MaskOffset2", "[AVP Xenomorphs] Patched PawnRenderNode.TryGetAnimationOffset for mask offset");
                }

                // Patch Pawn.DrawAt to draw mask overlay manually at face position
                var drawAtMethod = AccessTools.Method(typeof(Pawn), "DrawAt");
                if (drawAtMethod != null)
                {
                    harmony.Patch(drawAtMethod, postfix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(DrawAtPostfix)));
                    AvPDebug.LogOnce("MaskOffset3", "[AVP Xenomorphs] Patched Pawn.DrawAt for manual mask overlay");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init mask offset patch: " + e.Message);
            }
        }

        public static void DrawPrefix(PawnRenderTree __instance, ref PawnDrawParms parms)
        {
            // Track which pawn is being drawn
            lastMaskPawn = parms.pawn;
        }

        public static void DrawPostfix(PawnRenderTree __instance, PawnDrawParms parms)
        {
            lastMaskPawn = null;
        }

        public static void TryGetAnimationOffsetPostfix(PawnRenderNode __instance, ref bool __result, ref Vector3 offset)
        {
            // Ultra-fast check — most render nodes are not apparel
            if (!(__instance is PawnRenderNode_Apparel apparelNode)) return;
            var apparel = apparelNode.apparel;
            if (apparel == null) return;
            var def = apparel.def;
            if (def == null || def.defName == null) return;
            if (def.defName != "RRY_FacehuggerMask" && def.defName != "RRY_RoyalFacehuggerMask") return;
            // Move the mask down toward the face
            offset.z -= 0.35f;
            offset.y -= 0.1f;
            __result = true;
        }

        /// <summary>
        /// Manual mask overlay — draws the mask texture directly at the face position.
        /// This is a fallback if the render tree offset doesn't work.
        /// </summary>
        private static Graphic_Multi maskGraphic;
        private static Graphic_Multi royalMaskGraphic;
        private static bool maskInitialized = false;

        public static void DrawAtPostfix(Pawn __instance, Vector3 drawLoc, bool flip)
        {
            // Only for humanlike pawns with facehugger infection
            if (__instance == null || !__instance.Spawned || __instance.Dead) return;
            if (!__instance.RaceProps.Humanlike) return;
            if (!__instance.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)) return;

            // Initialize graphics
            if (!maskInitialized)
            {
                try
                {
                    maskGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Xenomorph/Xenomorph_FaceHugger_Mask", ShaderDatabase.Cutout, Vector2.one, Color.white);
                    royalMaskGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Xenomorph/Xenomorph_FaceHuggerRoyal_Mask", ShaderDatabase.Cutout, Vector2.one, Color.white);
                    maskInitialized = true;
                }
                catch
                {
                    return;
                }
            }

            // Determine which mask to use
            var facehuggerHediff = __instance.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection);
            bool royal = false;
            if (facehuggerHediff != null)
            {
                var comp = facehuggerHediff.TryGetComp<HediffComp_XenoFacehugger>();
                royal = comp?.RoyaleHugger ?? false;
            }
            Graphic_Multi graphic = royal ? royalMaskGraphic : maskGraphic;
            if (graphic == null) return;

            // Draw mask at face position — lower than head apparel position
            Vector3 facePos = drawLoc;
            facePos.z += 0.05f; // Slightly above body
            facePos.y += 0.15f; // Face height on the body
            // Scale based on body size
            float bodySize = __instance.RaceProps.baseBodySize;
            Vector3 drawSize = new Vector3(bodySize, 1f, bodySize);

            Material mat = graphic.MatAt(__instance.Rotation);
            if (mat == null) return;

            GenDraw.DrawMeshNowOrLater(MeshPool.plane10, Matrix4x4.TRS(facePos, Quaternion.identity, drawSize), mat, false);
        }
    }
}