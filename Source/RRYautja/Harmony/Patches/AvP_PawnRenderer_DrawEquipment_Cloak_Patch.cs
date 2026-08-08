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
    // TODO(1.5+): PawnRenderer.DrawEquipment may have been moved to PawnRenderUtility.DrawEquipmentAndApparelExtras in RimWorld 1.5+.
    // If the method was moved/renamed, this patch will silently fail to apply (Harmony will log a warning).
    // Verify against the RimWorld 1.5+ source/decompiled assembly and update the HarmonyPatch target accordingly.
    // The Main.PawnRenderer_GetPawn helper uses reflection on the "pawn" private field which may have been renamed.
#if false // TODO(1.5+): Re-enable after verifying DrawEquipment method exists on PawnRenderer in RimWorld 1.5+
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public static class AvP_PawnRenderer_DrawEquipment_Cloak_Patch
    {
        public static bool Prefix(PawnRenderer __instance)
        {
            Pawn pawn = Main.PawnRenderer_GetPawn(__instance);
            if (pawn == null) return true; // TODO(1.5+): pawn field reflection may fail if field was renamed
            bool flag = pawn.health.hediffSet.HasHediff(YautjaDefOf.RRY_Hediff_Cloaked, false);
            if (flag)
            {
                return false;
            }
            return true;
        }
    }
#endif // TODO(1.5+): Re-enable after verifying DrawEquipment method exists on PawnRenderer in RimWorld 1.5+

}