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
    
    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeUndowned")]
    public static class AvP_Pawn_HealthTracker_MakeUndowned_Xenomorph_Patch
    {
        public static FieldInfo pawn = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        [HarmonyPostfix]
        public static void Post_MakeUndowned(Pawn_HealthTracker __instance)
        {
            if (__instance!=null)
            {
                Traverse traverse = Traverse.Create(__instance);
                Pawn pawn = (Pawn)AvP_Pawn_HealthTracker_MakeUndowned_Xenomorph_Patch.pawn.GetValue(__instance);
                if (pawn!=null && !pawn.Dead && pawn.Map!=null)
                {
                    if (pawn.isXenomorph(out Comp_Xenomorph xenomorph))
                    {
                        if (pawn.ageTracker.CurLifeStage == XenomorphDefOf.RRY_XenomorphFullyFormed)
                        {
                            if (pawn.GetLord() == null)
                            {
                                xenomorph.delay = 30;
                            }
                        }
                    }
                }
            }
        }
    }
    
}