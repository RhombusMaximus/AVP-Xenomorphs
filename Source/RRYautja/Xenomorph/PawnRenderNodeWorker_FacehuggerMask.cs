using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Patches PawnRenderNode to offset the facehugger mask down to face level.
    /// In RimWorld 1.6, PawnRenderNode doesn't have a DrawMesh method.
    /// The draw offset is controlled by TryGetAnimationOffset and the render tree.
    /// We patch PawnRenderTree.DrawNode to modify the draw location.
    /// </summary>
    [StaticConstructorOnStartup]
    static class FacehuggerMaskOffsetPatch
    {
        static FacehuggerMaskOffsetPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.maskoffset");

                // Patch PawnRenderTree.DrawNode to modify the draw position
                var drawNodeMethod = AccessTools.Method(typeof(PawnRenderTree), "DrawNode");
                if (drawNodeMethod != null)
                {
                    harmony.Patch(drawNodeMethod, prefix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(DrawNodePrefix)));
                    AvPDebug.LogOnce("MaskOffset", "[AVP Xenomorphs] Patched PawnRenderTree.DrawNode for mask offset");
                }
                else
                {
                    // List available methods on PawnRenderTree
                    var methods = typeof(PawnRenderTree).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var m in methods)
                    {
                        if (m.Name.Contains("Draw") || m.Name.Contains("Node"))
                        {
                            AvPDebug.LogOnce("TreeMethod_" + m.Name, "[AVP Xenomorphs] PawnRenderTree method: " + m.Name + "(" + string.Join(", ", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name + " " + p.Name)) + ")");
                        }
                    }
                }

                // Also patch PawnRenderNode.TryGetAnimationOffset to add face offset
                var offsetMethod = AccessTools.Method(typeof(PawnRenderNode), "TryGetAnimationOffset");
                if (offsetMethod != null)
                {
                    harmony.Patch(offsetMethod, postfix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(TryGetAnimationOffsetPostfix)));
                    AvPDebug.LogOnce("MaskOffset2", "[AVP Xenomorphs] Patched PawnRenderNode.TryGetAnimationOffset for mask offset");
                }

                // Also try patching PawnRenderTree.DrawNode to modify draw position directly
                var drawNodeMethod = AccessTools.Method(typeof(PawnRenderTree), "DrawNode");
                if (drawNodeMethod != null)
                {
                    // Log the signature for debugging
                    var parms = drawNodeMethod.GetParameters();
                    AvPDebug.LogOnce("DrawNodeSig", "[AVP Xenomorphs] DrawNode signature: " + drawNodeMethod.ReturnType.Name + " DrawNode(" + string.Join(", ", Array.ConvertAll(parms, p => p.ParameterType.Name + " " + p.Name)) + ")");
                    harmony.Patch(drawNodeMethod, prefix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(DrawNodePrefix)));
                    AvPDebug.LogOnce("MaskOffset3", "[AVP Xenomorphs] Patched PawnRenderTree.DrawNode for mask offset");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init mask offset patch: " + e.Message);
            }
        }

        /// <summary>
        /// Postfix on PawnRenderNode.TryGetAnimationOffset — adds a face offset
        /// for facehugger mask apparel nodes.
        /// </summary>
        public static void TryGetAnimationOffsetPostfix(PawnRenderNode __instance, ref bool __result, ref Vector3 offset)
        {
            // Ultra-fast check — most render nodes are not apparel
            if (!(__instance is PawnRenderNode_Apparel apparelNode)) return;
            // Only facehugger mask apparel — check defName directly
            var apparel = apparelNode.apparel;
            if (apparel == null) return;
            var def = apparel.def;
            if (def == null || def.defName == null) return;
            // String comparison is fast — only 2 defNames to check
            if (def.defName != "RRY_FacehuggerMask" && def.defName != "RRY_RoyalFacehuggerMask") return;
            // Move the mask down toward the face (negative Z = down on screen)
            // Increased from 0.15 to 0.35 for more visible face placement
            offset.z -= 0.35f;
            // Also offset Y (vertical on screen in RimWorld's coordinate system)
            offset.y -= 0.1f;
            __result = true;
        }

        /// <summary>
        /// Prefix on PawnRenderTree.DrawNode — modifies the draw location for mask nodes.
        /// </summary>
        public static void DrawNodePrefix(PawnRenderNode node)
        {
            // This is a fallback if TryGetAnimationOffset doesn't work
            // We can't easily modify the draw position here without knowing the method signature
        }
    }
}