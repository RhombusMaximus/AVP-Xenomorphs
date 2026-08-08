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
	// InvisibilityMatPool GetInvisibleMat
	// TODO(1.5+): PawnRenderer.OverrideMaterialIfNeeded may have changed signature or been renamed in RimWorld 1.5+.
	// The method signature and parameter list (Material original, Pawn pawn) must be verified against the 1.5+/1.6 API.
	// If the method was renamed or its parameters changed, this patch will fail to apply at runtime.
	// Verify against the RimWorld 1.5+ source/decompiled assembly and update the HarmonyPatch attribute and method signature accordingly.
#if false // TODO(1.5+): Re-enable after verifying OverrideMaterialIfNeeded method name and signature in RimWorld 1.5+
    [HarmonyPatch(typeof(PawnRenderer), "OverrideMaterialIfNeeded")]
    public static class AvP_PawnRenderer_OverrideMaterialIfNeeded_Xenomorph_Patch
    {
        [HarmonyPostfix]
        public static void OverrideMaterialIfNeeded(PawnRenderer __instance, Material original, Pawn pawn,ref Material __result)
		{
			PawnGraphicSet graphics = new PawnGraphicSet_Invisible(pawn)
			{
				nakedGraphic = new Graphic_Invisible(),
				rottingGraphic = null,
				packGraphic = null,
				headGraphic = null,
				desiccatedHeadGraphic = null,
				skullGraphic = null,
				headStumpGraphic = null,
				desiccatedHeadStumpGraphic = null,
				hairGraphic = null
			};
			if (pawn.IsInvisible())
			{
				if (pawn.CarriedBy!=null)
				{
					if (pawn.CarriedBy.isXenomorph())
					{
						__result.SetTexture(graphics.nakedGraphic.MatSingle.name, graphics.nakedGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
						return;
					}
				}
				else
				if (pawn.Faction == Faction.OfPlayer)
				{
					__result.SetTexture(AvP_PawnRenderer_OverrideMaterialIfNeeded_Xenomorph_Patch.NoiseTex, TexGame.RippleTex);
					__result.color = AvP_PawnRenderer_OverrideMaterialIfNeeded_Xenomorph_Patch.xenomorphColor;
				}
				else
				{
					if (pawn.isXenomorph())
					{
						__result.SetTexture(graphics.nakedGraphic.MatSingle.name, graphics.nakedGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
					}
					/*
					if (pawn.isCloaked())
					{
						__result.SetTexture(graphics.nakedGraphic.MatSingle.name, graphics.nakedGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
					}
					*/
				}
			}
		}

		// Token: 0x04001120 RID: 4384
		private static Color xenomorphColor = new Color(0.25f, 0.25f, 0.25f, 0.0001f);
		private static int NoiseTex = Shader.PropertyToID("_NoiseTex");


	}
#endif // TODO(1.5+): Re-enable after verifying OverrideMaterialIfNeeded
   
}