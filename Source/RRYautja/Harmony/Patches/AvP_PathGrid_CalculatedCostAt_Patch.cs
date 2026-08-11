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
    // Pawns avoid acid Xenomorph acid
    [HarmonyPatch(typeof(Verse.AI.PathGrid), "CalculatedCostAt", new Type[] { typeof(IntVec3), typeof(bool), typeof(IntVec3) })]
    public static class AvP_PathGrid_CalculatedCostAt_Patch
    {
        [HarmonyPostfix]
        public static void CalculatedCostAtPostfix(IntVec3 c, bool perceivedStatic, IntVec3 prevCell, ref int __result)
        {
            Map map = Find.CurrentMap;

            if (map != null)
            {
                List<Thing> list = map.thingGrid.ThingsListAt(c);
                for (int j = 0; j < 9; j++)
                {
                    IntVec3 b = GenAdj.AdjacentCellsAndInside[j];
                    IntVec3 c2 = c + b;
                    if (c2.InBounds(map) && perceivedStatic)
                    {
                        Filth_AddAcidDamage acid = null;
                        list = map.thingGrid.ThingsListAtFast(c2);
                        if (list.Any(x => x.def == XenomorphDefOf.RRY_FilthBloodXenomorph_Active))
                        {
                            list = list.FindAll(x => x.def == XenomorphDefOf.RRY_FilthBloodXenomorph_Active);
                            for (int k = 0; k < list.Count; k++)
                            {
                                acid = (list[k] as Filth_AddAcidDamage);
                                if (acid != null)
                                {
                                    if (acid.active)
                                    {
                                        if (__result < 9000)
                                        {
                                            //    Log.Message(string.Format("acid is active: {0} = {1}", acid.active, __result));
                                            if (b.x == 0 && b.z == 0)
                                            {
                                                __result += 1000;
                                                //    Log.Message(string.Format("acid @: {0}, active: {1}, PathCost: {2}", c, acid.active, __result));
                                            }
                                            else
                                            {
                                                __result += 500;
                                                //    Log.Message(string.Format("acid adjacent to: {0} @: {1}, active: {2}, PathCost: {3}", c, c2, acid.active, __result));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //    Log.Message(string.Format("acid @: {0}, active: {1}, PathCost: {2}", c, acid.active, __result));
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
            }

        }
    }

}
#endif
