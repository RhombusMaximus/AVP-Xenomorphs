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
    //helicopter's mass will not appear in trade window
    [HarmonyPatch(typeof(Dialog_Trade), "SetupPlayerCaravanVariables", new Type[] { })]
    public static class HarmonyTest_trade
    {
        public static void Postfix(Dialog_Trade __instance)
        {
            Traverse tv = Traverse.Create(__instance);
            List<Thing> contents = tv.Field("playerCaravanAllPawnsAndItems").GetValue<List<Thing>>();
            List<Thing> newResult = new List<Thing>();
            if (contents == null || contents.Count <= 0) return;

            for (int i = 0; i < contents.Count; i++)
            {

                if (contents[i].def != USCMDefOf.RRY_USCM_DropshipUD4L)
                {
                    newResult.Add(contents[i]);
                }
            }
            tv.Field("playerCaravanAllPawnsAndItems").SetValue(newResult);
        }

    }

    //helicopter incoming, Edge Code thanks to SmashPhil and Neceros of SRTS-Expanded!
    [HarmonyPatch(typeof(DropPodUtility), "MakeDropPodAt", new Type[] { typeof(IntVec3), typeof(Map), typeof(ActiveTransporterInfo), typeof(Faction) })]
    public static class HarmonyTest
    {
        public static bool Prefix(IntVec3 c, Map map, ActiveTransporterInfo info)
        {
            Thing dropship = null;
            CompUSCMDropship cargo = null;
        //    CompTransporter comp2 = null;
            for (int index = 0; index < info.innerContainer.Count; index++)
            {
                if (info.innerContainer[index].TryGetComp<CompUSCMDropship>() != null)
                {
                    dropship = info.innerContainer[index];
                    ActiveTransporter activeDropPod = (ActiveTransporter)ThingMaker.MakeThing(USCMDefOf.RRY_USCM_ActiveDropshipUD4L, null);

                    activeDropPod.Contents = info;
                    EnsureInBounds(ref c, info.innerContainer[index].def, map);
                    info.innerContainer.Remove(dropship);
                    cargo = dropship.TryGetComp<CompUSCMDropship>();
                    cargo.Transporter.innerContainer = info.innerContainer;
                    SkyfallerMaker.SpawnSkyfaller(USCMDefOf.RRY_USCM_DropshipUD4LIncoming, dropship, c, map);
                    return false;
                }
            }

            return true;
        }

        private static void EnsureInBounds(ref IntVec3 c, ThingDef dropship, Map map)
        {

            int x = (int)9;
            int y = (int)9;
            int offset = x > y ? x : y;

            if (c.x < offset)
            {
                c.x = offset;
            }
            else if (c.x >= (map.Size.x - offset))
            {
                c.x = (map.Size.x - offset);
            }
            if (c.z < offset)
            {
                c.z = offset;
            }
            else if (c.z > (map.Size.z - offset))
            {
                c.z = (map.Size.z - offset);
            }
        }
    }


    [HarmonyPatch(typeof(Dialog_LoadTransporters), "AddPawnsToTransferables", new Type[] { })]
    public static class HarmonyTest_C
    {
        public static bool Prefix(Dialog_LoadTransporters __instance)
        {
            Traverse tv = Traverse.Create(__instance);
            List<CompTransporter> lp = tv.Field("transporters").GetValue<List<CompTransporter>>();
            foreach (CompTransporter lpc in lp)
            {
                if (lpc.parent.TryGetComp<CompUSCMDropship>() != null)
                {
                    Map map = tv.Field("map").GetValue<Map>();
                    List<Pawn> list = CaravanFormingUtility.AllSendablePawns(map, true, true, true, true);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Type typ = __instance.GetType();
                        MethodInfo minfo = typ.GetMethod("AddToTransferables", BindingFlags.NonPublic | BindingFlags.Instance);
                        minfo.Invoke(__instance, new object[] { list[i] });
                        // __instance.AddToTransferables(list[i]);
                    }
                    return false;
                }
            }
            return true;


        }

    }

    //helicopter arrive jingzhun
    [HarmonyPatch(typeof(TransportersArrivalAction_LandInSpecificCell), "Arrived", new Type[]  { typeof(List<ActiveTransporterInfo>), typeof(int) })]
    public static class HarmonyTest_AJ
    {
        public static bool Prefix(TransportersArrivalAction_LandInSpecificCell __instance, List<ActiveTransporterInfo> pods, int tile)
        {
        //    Log.Message(string.Format("pods: {0}", pods.Count));
            foreach (ActiveTransporterInfo info in pods)
            {
                if (info.innerContainer.Contains(USCMDefOf.RRY_USCM_DropshipUD4L))
                {
                //    Log.Message(string.Format("pods: {0}", info.innerContainer.ContentsString));
                    Thing lookTarget = null // TransportPodsArrivalActionUtility.GetLookTarget(pods) // Removed in 1.6;
                    Traverse tv = Traverse.Create(__instance);
                    IntVec3 c = tv.Field("cell").GetValue<IntVec3>();
                    Map map = tv.Field("mapParent").GetValue<MapParent>().Map;
                    // TransportPodsArrivalActionUtility.RemovePawnsFromWorldPawns(pods) // Removed in 1.6;
                    for (int i = 0; i < pods.Count; i++)
                    {
                        DropPodUtility.MakeDropPodAt(c, map, pods[i]);
                    }
                    Messages.Message("USCM_Dropship_MessageArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion, true);
                    return false;
                }
            }
            return true;


        }

    }

    //caravan gizmo
    [HarmonyPatch(typeof(Caravan), "GetGizmos", new Type[] { })]
    public static class HarmonyTest_BB
    {
        public static void Postfix(Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            float masss = 0;
            foreach (Pawn pawn in __instance.pawns.InnerListForReading)
            {

                for (int j = 0; j < pawn.inventory.innerContainer.Count; j++)
                {
                    if (pawn.inventory.innerContainer[j].def != USCMDefOf.RRY_USCM_DropshipUD4L)
                        masss += (pawn.inventory.innerContainer[j].def.BaseMass * pawn.inventory.innerContainer[j].stackCount);
                }
            }

            foreach (Pawn pawn in __instance.pawns.InnerListForReading)
            {
                Pawn_InventoryTracker pinv = pawn.inventory;
                for (int i = 0; i < pinv.innerContainer.Count; i++)
                {
                    if (pinv.innerContainer[i].def == USCMDefOf.RRY_USCM_DropshipUD4L)
                    {
                        Command_Action launch = new Command_Action();
                        launch.defaultLabel = "CommandSendShuttle".Translate();
                        launch.defaultDesc = "CommandSendShuttleDesc".Translate();
                        launch.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);
                        launch.alsoClickIfOtherInGroupClicked = false;
                        launch.action = delegate
                        {
                            float maxmass = pinv.innerContainer[i].TryGetComp<CompTransporter>().Props.massCapacity;
                            if (masss <= maxmass)
                                pinv.innerContainer[i].TryGetComp<CompUSCMDropship>().WorldStartChoosingDestination(__instance);
                            else
                                Messages.Message("TooBigTransportersMassUsage".Translate() + "(" + (maxmass - masss) + "KG)", MessageTypeDefOf.RejectInput, false);
                        };

                        List<Gizmo> newr = __result.ToList();
                        newr.Add(launch);

                        Command_Action addFuel = new Command_Action();
                        addFuel.defaultLabel = "USCM_Dropship_CommandAddFuel".Translate();
                        addFuel.defaultDesc = "USCM_Dropship_CommandAddFuelDesc".Translate();
                        addFuel.icon = ContentFinder<Texture2D>.Get("Things/Item/Resource/Chemfuel", true);
                        addFuel.alsoClickIfOtherInGroupClicked = false;
                        addFuel.action = delegate
                        {
                            bool hasAddFuel = false;
                            int fcont = 0;
                            CompRefuelable comprf = pinv.innerContainer[i].TryGetComp<CompRefuelable>();
                            List<Thing> list = CaravanInventoryUtility.AllInventoryItems(__instance);
                            //pinv.innerContainer.Count
                            for (int j = 0; j < list.Count; j++)
                            {

                                if (list[j].def == ThingDefOf.Chemfuel)
                                {
                                    fcont = list[j].stackCount;
                                    Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(__instance, list[j]);
                                    float need = comprf.Props.fuelCapacity - comprf.Fuel;

                                    if (need < 1f && need > 0)
                                    {
                                        fcont = 1;
                                    }
                                    if (fcont * 1f >= need)
                                    {
                                        fcont = (int)need;
                                    }



                                    // Log.Warning("f&n is "+fcont+"/"+need);
                                    if (list[j].stackCount * 1f <= fcont)
                                    {
                                        list[j].stackCount -= fcont;
                                        Thing thing = list[j];
                                        ownerOf.inventory.innerContainer.Remove(thing);
                                        thing.Destroy(DestroyMode.Vanish);
                                    }
                                    else
                                    {
                                        if (fcont != 0)
                                            list[j].SplitOff(fcont).Destroy(DestroyMode.Vanish);

                                    }


                                    Type crtype = comprf.GetType();
                                    FieldInfo finfo = crtype.GetField("fuel", BindingFlags.NonPublic | BindingFlags.Instance);
                                    finfo.SetValue(comprf, comprf.Fuel + fcont);
                                    hasAddFuel = true;
                                    break;

                                }
                            }
                            if (hasAddFuel)
                            {
                                Messages.Message("USCM_Dropship_AddFuelDoneMsg".Translate(fcont, comprf.Fuel), MessageTypeDefOf.PositiveEvent, false);
                            }
                            else
                            {
                                Messages.Message("USCM_Dropship_NoFuelMsg".Translate(), MessageTypeDefOf.RejectInput, false);
                            }

                        };

                        newr.Add(addFuel);

                        Gizmo_MapRefuelableFuelStatus fuelStat = new Gizmo_MapRefuelableFuelStatus
                        {
                            nowFuel = pinv.innerContainer[i].TryGetComp<CompRefuelable>().Fuel,
                            maxFuel = pinv.innerContainer[i].TryGetComp<CompRefuelable>().Props.fuelCapacity,
                            compLabel = pinv.innerContainer[i].TryGetComp<CompRefuelable>().Props.FuelGizmoLabel

                        };


                        newr.Add(fuelStat);

                        __result = newr;
                        return;
                    }
                }

            }


        }

    }

}