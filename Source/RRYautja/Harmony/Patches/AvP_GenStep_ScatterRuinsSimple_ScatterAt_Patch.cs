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
using System.Reflection.Emit;

namespace RRYautja
{

    /*
    [HarmonyPatch(typeof(GenStep_ScatterRuinsSimple), "ScatterAt")]
    public static class AvP_GenStep_ScatterRuinsSimple_ScatterAt_Patch
    {
        [HarmonyPostfix]
        public static void ScatterAt_Postfix(GenStep_ScatterRuinsSimple __instance, IntVec3 c, Map map)
        {
            map.HiveGrid().PotentialHiveLoclist.Add(new PotentialXenomorphHiveLocation(c));
        }
    }
    
    */

    [HarmonyPatch(typeof(GenStep_ScatterRuinsSimple), "ScatterAt")]
    static class AvP_GenStep_ScatterRuinsSimple_ScatterAt_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            foreach (var instruction in instructionsList)
            {
                yield return instruction;
                if (instruction.operand as MethodInfo == typeof(CellRect).GetMethod("get_CenterCell"))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(AvP_GenStep_ScatterRuinsSimple_ScatterAt_Patch).GetMethod("CenterCellValue"));
                }

            }
        }
        public static IntVec3 CenterCellValue(IntVec3 pos)
        {
            Map map = MapGenerator.mapBeingGenerated;
            MapComponent_HiveGrid hiveGrid = map.HiveGrid();
            hiveGrid.PotentialHiveLoclist.Add(new PotentialXenomorphHiveLocation(pos));
        //    Log.Message(string.Format("Ruin spawned: {0}, adding to Maps Potential Hive locations, Total: {1}", pos, hiveGrid.PotentialHiveLoclist.Count));
            
            return pos;
        }
    }
    
}