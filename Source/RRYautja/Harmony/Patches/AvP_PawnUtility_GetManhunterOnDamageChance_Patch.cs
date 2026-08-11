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

    // Stops wild animals attacked by xeno/neomorphs triggering manhunter 
    [HarmonyPatch(typeof(RimWorld.PawnUtility), "GetManhunterOnDamageChance", new Type[] { typeof(Pawn), typeof(Thing) }), StaticConstructorOnStartup]
    public static class AvP_PawnUtility_GetManhunterOnDamageChance_Patch
    {
        [HarmonyPostfix]
        public static void GetManhunterOnDamageChancePostfix(Pawn pawn, Thing instigator, ref float __result)
        {
            if (instigator != null)
            {
                if (instigator.def.thingClass == typeof(Pawn))
                {
                    __result = XenomorphUtil.IsXenomorphPawn(((Pawn)instigator)) ? 0.0f : __result;
                    //     Log.Message(string.Format("__result: {0}", __result));
                }
            }
        }
    }

}
#endif
