using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Swaps the body graphic of Drone pawns to one of the available drone
    /// texture variants (base, Dark, Gold, Light, Red) based on the
    /// droneTextureVariant field set on Comp_Xenomorph at spawn time.
    ///
    /// Drones use renderTree>Animal, so their body graphic is resolved by
    /// PawnRenderNode_AnimalPart.GraphicFor (not PawnRenderNode_Body).
    /// This patch intercepts that method and replaces the graphic for adult
    /// Drones only. Chestburster life stages use the normal life stage graphics
    /// (ChestBursterBloody / ChestBurster) because the variant override is
    /// gated on the pawn being at the final life stage.
    ///
    /// The variant is rolled in XenomorphFactionAssigner.GeneratePawnPostfix
    /// and persisted via Scribe in Comp_Xenomorph.PostExposeData.
    /// </summary>
    [StaticConstructorOnStartup]
    static class AvP_DroneVariant_Patch
    {
        // Texture paths for each variant index.
        // 0 = base (no override needed — the PawnKindDef already points at Xenomorph_Drone)
        // 1..4 = New_Dark, New_Gold, New_Light, New_Red
        private static readonly string[] VariantPaths =
        {
            null, // 0: base, no override
            "Things/Pawn/Xenomorph/Xenomorph_Drone_New_Dark",
            "Things/Pawn/Xenomorph/Xenomorph_Drone_New_Gold",
            "Things/Pawn/Xenomorph/Xenomorph_Drone_New_Light",
            "Things/Pawn/Xenomorph/Xenomorph_Drone_New_Red"
        };

        // Cached Graphics keyed by variant index — Graphic_Multi loads all 4 facings.
        private static readonly Graphic_Multi[] variantGraphics = new Graphic_Multi[5];

        static AvP_DroneVariant_Patch()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.dronevariant");
            // Drones are animals (renderTree>Animal), so their body graphic is
            // produced by PawnRenderNode_AnimalPart.GraphicFor, not
            // PawnRenderNode_Body.GraphicFor (which is for humanlikes).
            var target = AccessTools.Method(typeof(PawnRenderNode_AnimalPart), "GraphicFor");
            var postfix = AccessTools.Method(typeof(AvP_DroneVariant_Patch), nameof(GraphicFor_Postfix));
            if (target != null && postfix != null)
            {
                harmony.Patch(target, postfix: new HarmonyMethod(postfix));
                AvPDebug.LogOnce("Patch", "[AVP Xenomorphs] Patched PawnRenderNode_AnimalPart.GraphicFor for drone texture variants");
            }
            else
            {
                Log.Warning("[AVP Xenomorphs] Failed to patch PawnRenderNode_AnimalPart.GraphicFor (method not found)");
            }
        }

        private static Graphic_Multi GetVariantGraphic(int variant)
        {
            if (variant < 0 || variant >= VariantPaths.Length || VariantPaths[variant] == null)
            {
                return null;
            }
            if (variantGraphics[variant] == null)
            {
                variantGraphics[variant] = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(
                    VariantPaths[variant],
                    ShaderDatabase.CutoutComplex,
                    Vector2.one,
                    Color.white);
            }
            return variantGraphics[variant];
        }

        /// <summary>
        /// Postfix on PawnRenderNode_AnimalPart.GraphicFor(Pawn).
        /// Replaces the returned body graphic for adult Drones whose
        /// droneTextureVariant > 0. Chestburster life stages are unaffected
        /// because we check that the pawn is at its final life stage.
        /// </summary>
        static void GraphicFor_Postfix(PawnRenderNode_AnimalPart __instance, ref Graphic __result, Pawn pawn)
        {
            if (__result == null || pawn == null) return;

            // Only Drones get variant textures.
            if (pawn.kindDef != XenomorphDefOf.RRY_Xenomorph_Drone) return;

            Comp_Xenomorph comp = pawn.TryGetComp<Comp_Xenomorph>();
            if (comp == null) return;

            int variant = comp.droneTextureVariant;
            if (variant <= 0) return; // 0 = base texture, no override

            // Only override the graphic for adult (final life stage) Drones.
            // Chestburster stages (0-1) must keep their own textures.
            if (pawn.ageTracker.CurLifeStage != XenomorphDefOf.RRY_XenomorphFullyFormed) return;

            Graphic_Multi replacement = GetVariantGraphic(variant);
            if (replacement != null)
            {
                __result = replacement;
            }
        }
    }
}