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

    // ObserveSurroundingThings
    [HarmonyPatch(typeof(PawnObserver), "ObserveSurroundingThings")]
    public static class AvP_PawnObserver_ObserveSurroundingThings_Patch
    {
        [HarmonyPostfix]
        public static void ObserveSurroundingThingsPostfix(PawnObserver __instance)
        {
            Traverse traverse = Traverse.Create(__instance);
            Pawn pawn = (Pawn)AvP_PawnObserver_ObserveSurroundingThings_Patch.pawn.GetValue(__instance);
        //    Log.Message(string.Format("ObserveSurroundingThingsPostfix {0}", pawn.LabelShortCap));
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight) || pawn.isBloodMarked() || pawn.def == YautjaDefOf.RRY_Alien_Yautja)
            {
            //    Log.Message("Blind");
                return;
            }
            float num = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
        //    Log.Message(string.Format("Sight: {0}", num));
            Map map = pawn.Map;
            //    List<Thing> thingList = intVec.GetThingList(map).FindAll(x => x.def.thingClass == typeof(Pawn) && (Pawn)x != pawn);
            List<Pawn> thingList = map.mapPawns.AllPawns.Where(x => x != pawn && x.isXenomorph() && GenSight.LineOfSight(x.Position, pawn.Position, map, true, null, 0, 0) && pawn.Position.DistanceTo(x.Position) <= (20 * num)).ToList();

            if (!thingList.NullOrEmpty())
            {
                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn observed = (Pawn)thingList[i];
                    if (observed==null)
                    {
                        return;
                    }
                    if (!observed.isXenomorph(out Comp_Xenomorph thoughtGiver))
                    {
                        return;
                    }
                //    Log.Message(string.Format("{0} observed {1}", pawn.LabelShortCap, observed.LabelShortCap));
                    if (thoughtGiver != null)
                    {
                        Thought_Memory thought_Memory = thoughtGiver.GiveObservedThought(Traverse.Create(__instance).Field("pawn").GetValue<Pawn>());
                        if (thought_Memory != null)
                        {
                            //Log.Message(string.Format("{0} TryGainMemory {1}", pawn.LabelShortCap, thought_Memory.LabelCap));
                            pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, observed);
                        }
                    //    else Log.Message("thought_Memory == null");
                    }
                //    else Log.Message(string.Format("thoughtGiver {0} == null", thingList[i].LabelShortCap));
                }
            }
        //    else Log.Message("thingList.NullOrEmpty");
        }
        public static FieldInfo pawn = typeof(PawnObserver).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
    }

}