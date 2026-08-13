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
    
    [HarmonyPatch(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex")]
    public static class AvP_Pawn_AgeTracker_RecalculateLifeStageIndex_Xenomorph_Patch
    {
        public static FieldInfo pawn = typeof(Pawn_AgeTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        [HarmonyPostfix]
        public static void Post_RecalculateLifeStageIndex(Pawn_AgeTracker __instance)
        {
            if (__instance!=null)
            {
                Traverse traverse = Traverse.Create(__instance);
                Pawn pawn = (Pawn)AvP_Pawn_AgeTracker_RecalculateLifeStageIndex_Xenomorph_Patch.pawn.GetValue(__instance);
                if (pawn!=null && !pawn.Dead)
                {
                    if (pawn.isXenomorph(out Comp_Xenomorph xenomorph))
                    {
                        if (__instance.CurLifeStage == XenomorphDefOf.RRY_XenomorphFullyFormed)
                        {
                            if (pawn.GetLord()==null)
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