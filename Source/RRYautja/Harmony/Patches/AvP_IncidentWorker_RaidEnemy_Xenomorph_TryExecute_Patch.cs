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
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class AvP_IncidentWorker_RaidEnemy_Xenomorph_TryExecute_Patch
    {
        [HarmonyPrefix]
        public static bool PreExecute(ref IncidentParms parms)
        {
            if (parms.target is Map && (parms.target as Map).IsPlayerHome)
            {
                if (parms.faction != null && (parms.faction.def == XenomorphDefOf.RRY_Xenomorph))
                {
                    if ((parms.target is Map map))
                    {
                        parms.generateFightersOnly = true;
                        int @int = Rand.Int;
                        bool extTunnels = !map.GetComponent<MapComponent_HiveGrid>().HiveChildlist.NullOrEmpty();
                        if (Rand.ChanceSeeded(0.05f, AvPConstants.AvPSeed))
                        {
                            parms.forced = true;
                            parms.points = Mathf.Max(parms.points * new FloatRange(1f, 1.6f).RandomInRange, parms.faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
                            
                        }
                        if (parms.raidStrategy != XenomorphDefOf.RRY_PowerCut)
                        {
                            if ((parms.target as Map).skyManager.CurSkyGlow <= 0.5f)
                            {
                                parms.points *= 2;
                                if (Rand.Chance(0.05f))
                                {
                                    parms.forced = true;
                                    parms.points = Mathf.Max(parms.points * new FloatRange(1f, 1.6f).RandomInRange, parms.faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
                                }
                            }
                            if (extTunnels && Rand.ChanceSeeded(0.10f + (map.GetComponent<MapComponent_HiveGrid>().HiveChildlist.Count / 100f), AvPConstants.AvPSeed))
                            {
                                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                                parms.raidArrivalMode = XenomorphDefOf.RRY_RandomEnterFromTunnel;
                            }
                            else
                            {
                                if (parms.raidArrivalMode != XenomorphDefOf.RRY_RandomEnterFromTunnel && parms.raidArrivalMode != XenomorphDefOf.RRY_DropThroughRoofNearPower)
                                {
                                    parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}