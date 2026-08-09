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
    // Xenomorphs should prefer blunt attacks when attempting to gather host pawns
    [HarmonyPatch(typeof(Pawn_MeleeVerbs), "ChooseMeleeVerb")]
    public static class AvP_Pawn_MeleeVerbs_ChooseMeleeVerb_Patch
    {
        [HarmonyPostfix]
        public static void HarmsHealthPostfix(Pawn_MeleeVerbs __instance, Thing target, ref Verb ___curMeleeVerb)
        {
            if (__instance.Pawn.isXenomorph() && __instance.Pawn.def != XenomorphRacesDefOf.RRY_Xenomorph_FaceHugger && target is Pawn pawn)
            {
                if (pawn.isPotentialHost())
                {
                    bool flag = Rand.Chance(0.04f);
                    List<VerbEntry> updatedAvailableVerbsList = __instance.GetUpdatedAvailableVerbsList(flag);
                    //    Log.Message(string.Format("All AvailableVerbs for {0}: {1}", __instance.Pawn.LabelShortCap, updatedAvailableVerbsList.Count));
                    updatedAvailableVerbsList = updatedAvailableVerbsList.FindAll(x => x.verb.maneuver == DefDatabase<ManeuverDef>.GetNamedSilentFail("Smash"));
                    //    Log.Message(string.Format("AvailableVerbs Smash for {0}: {1}", __instance.Pawn.LabelShortCap, updatedAvailableVerbsList.Count));
                    bool flag2 = false;
                    VerbEntry verbEntry;
                    if (updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out verbEntry))
                    {
                        flag2 = true;
                        //    Log.Message(string.Format("{0}'s using {1} against {2}", __instance.Pawn.LabelShortCap, verbEntry.verb.maneuver, pawn.LabelShortCap));
                    }
                    else if (flag)
                    {
                        updatedAvailableVerbsList = __instance.GetUpdatedAvailableVerbsList(false);
                        updatedAvailableVerbsList = updatedAvailableVerbsList.FindAll(x => x.verb.maneuver == DefDatabase<ManeuverDef>.GetNamedSilentFail("Smash"));
                        flag2 = updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out verbEntry);
                        //    Log.Message(string.Format("{0}'s using {1} against {2}", __instance.Pawn.LabelShortCap, verbEntry.verb.maneuver, pawn.LabelShortCap));
                    }
                    if (flag2)
                    {
                        //    verbEntry.verb.tool.capacities.Contains();
                        ___curMeleeVerb = verbEntry.verb;
                    }
                    else
                    {
                        Log.ErrorOnce(string.Concat(new object[]
                        {
                    __instance.Pawn.ToStringSafe<Pawn>(),
                    " has no available melee attack, spawned=",
                    __instance.Pawn.Spawned,
                    " dead=",
                    __instance.Pawn.Dead,
                    " downed=",
                    __instance.Pawn.Downed,
                    " curJob=",
                    __instance.Pawn.CurJob.ToStringSafe<Job>(),
                    " verbList=",
                    updatedAvailableVerbsList.ToStringSafeEnumerable(),
                    " bodyVerbs=",
                    __instance.Pawn.verbTracker.AllVerbs.ToStringSafeEnumerable()
                        }), __instance.Pawn.thingIDNumber ^ 195867354);
                        ___curMeleeVerb = null;
                    }
                }
            }
        }
    }

}