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
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
    public static class AvP_Pawn_HealthTracker_CheckForStateChange_Patch
    {
        [HarmonyPrefix]
        public static bool CheckForStateChange_Xenomorph_Prefix(Pawn_HealthTracker __instance, DamageInfo? dinfo, Hediff hediff)
        {
            if (dinfo!=null)
            {
                Pawn instigator = null;
                if (dinfo.Value.Instigator!=null && dinfo.Value.Instigator.GetType() == typeof(Pawn))
                {
                    instigator = (Pawn)dinfo.Value.Instigator;
                }
                if (instigator!=null)
                {
                    if (instigator.isXenomorph())
                    {

						FieldInfo fieldInfo = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");
						MethodBase methodBase = AccessTools.Method(typeof(Pawn_HealthTracker), "MakeDowned", null, null);
						MethodBase methodBase2 = AccessTools.Method(typeof(Pawn_HealthTracker), "MakeUnDowned", null, null);
						Traverse traverse = Traverse.Create(__instance);
						Pawn pawn = (Pawn)fieldInfo.GetValue(__instance);
						bool flag = pawn != null && dinfo != null && dinfo != null && hediff != null && !ThingUtility.DestroyedOrNull(pawn);
						bool result;
						if (flag)
						{
							bool flag2 = !__instance.Dead;
							if (flag2)
							{
								bool value = traverse.Method("ShouldBeDead", Array.Empty<object>()).GetValue<bool>();
								if (value)
								{
									bool flag3 = !pawn.Destroyed;
									if (flag3)
									{
										pawn.Kill(dinfo, hediff);
									}
									return false;
								}
								bool flag4 = !__instance.Downed;
								if (flag4)
								{
									bool value2 = traverse.Method("ShouldBeDowned", Array.Empty<object>()).GetValue<bool>();
									if (value2)
									{
										/*
										bool flag5 = !__instance.forceIncap && dinfo != null && dinfo.Value.Def.ExternalViolenceFor(pawn) && !WildManUtility.IsWildMan(pawn) && (pawn.Faction == null || !pawn.Faction.IsPlayer) && (pawn.HostFaction == null || !pawn.HostFaction.IsPlayer);
										if (flag5)
										{
											bool animal = pawn.RaceProps.Animal;
											float num;
											if (animal && pawn.isPotentialHost())
											{
												num = 0f;
											}
										}
										*/
										__instance.forceDowned = false;
										methodBase.Invoke(__instance, new object[]
										{
											dinfo,
											hediff
										});
										return false;
									}
									bool flag7 = !__instance.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
									if (flag7)
									{
										bool flag8 = pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.jobs != null && pawn.CurJob != null;
										if (flag8)
										{
											pawn.jobs.EndCurrentJob((JobCondition)5, true, true);
										}
										bool flag9 = pawn.equipment != null && pawn.equipment.Primary != null;
										if (flag9)
										{
											bool destroyGearOnDrop = pawn.kindDef.destroyGearOnDrop;
											if (destroyGearOnDrop)
											{
												pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
											}
											else
											{
												bool inContainerEnclosed = pawn.InContainerEnclosed;
												if (inContainerEnclosed)
												{
													pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.holdingOwner);
												}
												else
												{
													bool spawnedOrAnyParentSpawned = pawn.SpawnedOrAnyParentSpawned;
													if (spawnedOrAnyParentSpawned)
													{
														ThingWithComps thingWithComps;
														pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out thingWithComps, pawn.PositionHeld, true);
													}
													else
													{
														pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
													}
												}
											}
										}
									}
								}
								else
								{
									bool flag10 = !traverse.Method("ShouldBeDowned", Array.Empty<object>()).GetValue<bool>();
									if (flag10)
									{
										methodBase2.Invoke(__instance, null);
										return false;
									}
								}
							}
							result = false;
						}
						else
						{
							result = true;
						}
						return result;
                    }
                }
            }
            return true;
        }
    }

   
}