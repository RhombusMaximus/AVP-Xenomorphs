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
    // Disables Stuffable Projectiles Firing while wearing vanillia Shield Belts
    [HarmonyPatch(typeof(ShieldBelt), "AllowVerbCast")]
    internal static class AvP_ShieldBelt_AllowVerbCast_YautjaWeapons_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(IntVec3 root, Map map, LocalTargetInfo targ, Verb verb, ref bool __result)
        {
            bool flag = verb is Verb_Launch_Stuffable_Projectile;
            __result = __result && !flag;
            return;
        }
    }

}