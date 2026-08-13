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
    // Protects Cocooned Pawns from wound infections
    [HarmonyPatch(typeof(HediffComp_Infecter), "CheckMakeInfection")]
    public static class AvP_HediffComp_Infecter_CheckMakeInfection_Patch
    {
        [HarmonyPrefix]
        public static bool preCheckMakeInfection(HediffComp_Infecter __instance)
        {
            if (__instance.Pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Cocooned) || (__instance.Pawn.InBed() && __instance.Pawn.CurrentBed() is Building_XenomorphCocoon))
            {
                return false;
            }
            return true;
        }
    }

}