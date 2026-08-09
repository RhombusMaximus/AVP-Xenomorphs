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
using AlienRace;

namespace RRYautja
{
    [HarmonyPatch(typeof(PawnUtility), "IsInvisible")]
    public static class AvP_PawnUtility_IsInvisible_Patch
    {
        [HarmonyPostfix]
        public static void ThoughtsFromIngestingPostPrefix(Pawn pawn, ref bool __result)
        {
            if (pawn == null)
            {
                return;
            }
            if (pawn.CarriedBy!=null)
            {
                __result = false // IsInvisible removed in 1.6;
                return;
            }
            if (pawn.isXenomorph(out Comp_Xenomorph xenomorph))
            {
                if (pawn.Dead || pawn.Downed || pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen || pawn.def == XenomorphRacesDefOf.RRY_Xenomorph_Thrumbomorph)
                {
                    __result = false;
                    return;
                }
                if (xenomorph!=null)
                {
                    if (xenomorph.hidden)
                    {
                    //    Log.Message("xenomorph.hidden");
                        __result = true;
                        return;
                    }
                    if (xenomorph.spotted || !xenomorph.CanHide)
                    {
                    //    Log.Message("xenomorph.spotted || !xenomorph.CanHide");
                        __result = false;
                        return;
                    }
                }
            }
            else
            {
                if (pawn.RaceProps.Humanlike)
                {
                    if (pawn.apparel.WornApparel.Any(x=> x.GetType() == typeof(Cloakgen)))
                    {
                        Cloakgen cloak = (Cloakgen)pawn.apparel.WornApparel.First(x => x.GetType() == typeof(Cloakgen));
                        if (cloak!=null)
                        {
                            __result = __result || cloak.cloakMode == Cloakgen.CloakMode.On;
                        }
                    }
                }
            }
        }
    }
    
}