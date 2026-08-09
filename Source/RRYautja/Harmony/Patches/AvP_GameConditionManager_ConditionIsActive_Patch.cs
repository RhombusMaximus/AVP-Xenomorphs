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
    [HarmonyPatch(typeof(GameConditionManager), "ConditionIsActive")]
    internal static class AvP_GameConditionManager_ConditionIsActive_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(GameConditionManager __instance, ref GameConditionDef def, ref bool __result)
        {
            if (def == GameConditionDefOf.Eclipse)
            {
                //    Log.Message(string.Format("GameConditionManager_ConditionIsActive_Patch SolarFlare: {0}", __result));
                __result = __result || __instance.ConditionIsActive(XenomorphDefOf.RRY_Xenomorph_PowerCut);
#if DEBUG
            //    Log.Message(string.Format("GameConditionManager_ConditionIsActive_Patch Xenomorph_PowerCut: {0}", __result));
#endif
            }
        }
    }
}