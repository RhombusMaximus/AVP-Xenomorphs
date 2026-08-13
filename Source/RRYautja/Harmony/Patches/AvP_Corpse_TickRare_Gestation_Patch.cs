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

    [HarmonyPatch(typeof(Corpse), "TickRare")]
    public static class AvP_Corpse_TickRare_Gestation_Patch
    {
        [HarmonyPostfix]
        public static void Post_TickRare(ref Corpse __instance)
        {
            if (__instance!=null)
            {
                if (__instance.InnerPawn!=null)
                {
                    Pawn pawn = __instance.InnerPawn;
                    if (pawn.isHost(out Hediff parasite))
                    {
                        if (parasite!=null && parasite.CurStageIndex>=3)
                        {
                            for (int i = 0; i < 250; i++)
                            {
                                parasite.Tick();
                            }
                            if (parasite.Severity>=1f)
                            {
                                if (parasite.def.defName.Contains("Neomorph"))
                                {
                                    parasite.TryGetComp<HediffComp_NeoSpawner>().Notify_PawnDied(null, null);
                                }
                                else
                                {
                                    parasite.TryGetComp<HediffComp_XenoSpawner>().Notify_PawnDied(null, null);
                                }
                            }
                        }
                    }
                }
            }
        }
        
    }
    
}