#if false
﻿using RimWorld;
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
    // Pauses NeedsTracker on Cocooned Pawns
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "NeedsTrackerTick", null)]
    public static class AvP_Pawn_NeedsTracker_Patch
    {
        public static bool Prefix(Pawn_NeedsTracker __instance)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn pawn = (Pawn)AvP_Pawn_NeedsTracker_Patch.pawn.GetValue(__instance);
            bool flag = pawn != null;
            if (flag)
            {
                bool flag2 = (pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Cocooned));
                bool flag3 = (pawn.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection) && Find.TickManager.TicksGame % 5 != 0);
                if (flag2 || flag3)
                {
                    return false;
                }
            }
            return true;
        }

        public static FieldInfo pawn = typeof(Pawn_NeedsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
    }
}
#endif
