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
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor", null)]
    public class AvP_Pawn_RelationsTracker_SecondaryRomanceChanceFactor_Patch
    {
        [HarmonyPostfix]
        public static void SecondaryRomanceChanceFactor(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn pawn = (Pawn)AvP_Pawn_RelationsTracker_SecondaryRomanceChanceFactor_Patch.pawn.GetValue(__instance);
            bool flag = pawn != null && otherPawn != null;
            if (flag)
            {
                bool alien = !Equals(otherPawn.def, pawn.def);
                if (alien)
                {
                    float num = 0.5f;
                    __result *= num;
                }
            }
        }

        public static FieldInfo pawn = typeof(Pawn_RelationsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

    }

}