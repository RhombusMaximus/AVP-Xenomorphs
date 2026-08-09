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

    [HarmonyPatch(typeof(PawnGraphicSet), "HeadMatAt")]
    public static class AvP_PawnGraphicSet_HeadMatAt_Invis_Patch
	{
        [HarmonyPostfix]
        public static void HeadMatAt(PawnGraphicSet __instance, Rot4 facing, RotDrawMode bodyCondition, bool stump, ref Material __result)
		{
			Pawn pawn = __instance.pawn;
			PawnGraphicSet graphics = new PawnGraphicSet_Invisible(pawn)
			{
				nakedGraphic = new Graphic_Invisible(),
				rottingGraphic = null,
				packGraphic = null,
				headGraphic = new Graphic_Invisible(),
				desiccatedHeadGraphic = null,
				skullGraphic = null,
				headStumpGraphic = null,
				desiccatedHeadStumpGraphic = null,
				hairGraphic = null
			};
			if (pawn.IsInvisible() && pawn.RaceProps.Humanlike)
			{
				if (pawn.CarriedBy!=null)
				{
					if (pawn.CarriedBy.isXenomorph())
					{
						__result.SetTexture(graphics.headGraphic.MatSingle.name, graphics.headGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
						return;
					}
				}
			}
		}


	}
   
}