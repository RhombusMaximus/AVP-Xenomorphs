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
    [HarmonyPatch(typeof(SnowUtility), "AddSnowRadial")]
    public static class AvP_SnowUtility_AddSnowRadial_Patch
    {
        [HarmonyPostfix]
        public static void AddSnowRadialPostfix(IntVec3 center, Map map, float radius, float depth)
        {
            //    Log.Message(string.Format("AddSnowRadial center: {0}, radius: {1}, depth: {2}", center, radius, depth));
            MapComponent_HiveGrid _HiveGrid = map.GetComponent<MapComponent_HiveGrid>();
            if (_HiveGrid != null)
            {
                //    Log.Message(string.Format("AddSnowRadial _HiveGrid != null center: {0}, radius: {1}, depth: {2}", center, radius, depth));
                XenomorphHiveUtility.AddHiveRadial(center, map, radius, depth);
            }
            //    Log.Message(string.Format("AddSnowRadial center: {0}, radius: {1}, depth: {2}", center, radius, depth));
            MapComponent_GooGrid _GooGrid = map.GetComponent<MapComponent_GooGrid>();
            if (_GooGrid != null)
            {
                //    Log.Message(string.Format("AddSnowRadial _HiveGrid != null center: {0}, radius: {1}, depth: {2}", center, radius, depth));
                XenomorphHiveUtility.AddGooRadial(center, map, radius, depth);
            }
        }
    }
}