using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Patches PawnRenderNode.DrawMesh to offset the facehugger mask down to face level.
    /// The default head apparel renders at the top of the head (hat position).
    /// This prefix intercepts the draw call and moves the mask down.
    /// </summary>
    [StaticConstructorOnStartup]
    static class FacehuggerMaskOffsetPatch
    {
        static FacehuggerMaskOffsetPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.maskoffset");

                // Find PawnRenderNode.DrawMesh method
                var drawMeshMethod = AccessTools.Method(typeof(PawnRenderNode), "DrawMesh");
                if (drawMeshMethod != null)
                {
                    harmony.Patch(drawMeshMethod, prefix: new HarmonyMethod(typeof(FacehuggerMaskOffsetPatch), nameof(DrawMeshPrefix)));
                    AvPDebug.LogOnce("MaskOffset", "[AVP Xenomorphs] Patched PawnRenderNode.DrawMesh for mask offset");
                }
                else
                {
                    // List available methods for debugging
                    var methods = typeof(PawnRenderNode).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var m in methods)
                    {
                        if (m.Name.Contains("Draw") || m.Name.Contains("Offset") || m.Name.Contains("Mesh"))
                        {
                            AvPDebug.LogOnce("MaskMethod_" + m.Name, "[AVP Xenomorphs] PawnRenderNode method: " + m.Name + "(" + string.Join(", ", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name + " " + p.Name)) + ")");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init mask offset patch: " + e.Message);
            }
        }

        /// <summary>
        /// Prefix on PawnRenderNode.DrawMesh — modifies the draw location
        /// to move the facehugger mask down to face level.
        /// </summary>
        public static void DrawMeshPrefix(PawnRenderNode __instance, ref Vector3 loc, Rot4 facing, Pawn pawn)
        {
            // Check if this render node is for our facehugger mask apparel
            if (__instance is PawnRenderNode_Apparel apparelNode)
            {
                var def = apparelNode.apparel?.def;
                if (def != null && (def.defName == "RRY_FacehuggerMask" || def.defName == "RRY_RoyalFacehuggerMask"))
                {
                    // Move the mask down toward the face (negative Z = down on screen)
                    loc.z -= 0.15f;
                }
            }
        }
    }
}