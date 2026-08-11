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
    // ScenarioLister.ScenariosInCategory
    // Hides Yautja Scenarios when Faction is disabled
    [HarmonyPatch(typeof(ScenarioLister), "ScenariosInCategory")]
    public static class AvP_Page_ScenarioLister_ScenariosInCategory_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<Scenario> ScenariosInCategoryPrefix(IEnumerable<Scenario> scenario, ScenarioCategory cat)
        {
            if (cat == ScenarioCategory.FromDef)
            {
                IEnumerable<ScenarioDef> scenarios = null;
                scenarios = DefDatabase<ScenarioDef>.AllDefs;
                foreach (ScenarioDef scenDef in DefDatabase<ScenarioDef>.AllDefs)
                {
                    //    Log.Message(string.Format("Found {0}", scenarios.Count()));
                    if (!scenDef.defName.Contains("Yautja"))
                    {
                        yield return scenDef.scenario;
                    }
                }
            }
            else if (cat == ScenarioCategory.CustomLocal)
            {
                IEnumerable<Scenario> scenarios = ScenarioFiles.AllScenariosLocal;
                foreach (Scenario scen in scenarios)
                {
                    //    Log.Message(string.Format("Found {0}", scenarios.Count()));
                    if (!scen.name.Contains("Yautja"))
                    {
                        yield return scen;
                    }
                }
            }
            else if (cat == ScenarioCategory.SteamWorkshop)
            {
                IEnumerable<Scenario> scenarios = ScenarioFiles.AllScenariosWorkshop;
                foreach (Scenario scen2 in scenarios)
                {
                    //    Log.Message(string.Format("Found {0}", scenarios.Count()));
                    if (!scen2.name.Contains("Yautja"))
                    {
                        yield return scen2;
                    }
                }
            }
            yield break;

        }
    }
}