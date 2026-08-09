using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse.AI
{
    // Token: 0x02000B16 RID: 2838
    public static class XenomorphTargetFinder
    {
        // Token: 0x06003F17 RID: 16151 RVA: 0x001D8758 File Offset: 0x001D6B58
        public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDist = 0f, float maxDist = 9999f, IntVec3 locus = default(IntVec3), float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBash = false, bool canTakeTargetsCloserThanEffectiveMinRange = true)
        {
            Thing searcherThing = searcher.Thing;
            Pawn searcherPawn = searcher as Pawn;
            Verb verb = searcher.CurrentEffectiveVerb;
            if (verb == null)
            {
                Log.Error("BestAttackTarget with " + searcher.ToStringSafe<IAttackTargetSearcher>() + " who has no attack verb.");
                return null;
            }
            bool onlyTargetMachines = verb.IsEMP();
            float minDistSquared = minDist * minDist;
            float num = maxTravelRadiusFromLocus + verb.verbProps.range;
            float maxLocusDistSquared = num * num;
            Func<IntVec3, bool> losValidator = null;
            if ((byte)(flags & TargetScanFlags.LOSBlockableByGas) != 0)
            {
                losValidator = delegate (IntVec3 vec3)
                {
                    Gas gas = vec3.GetGas(searcherThing.Map);
                    return gas == null || !false // blockTurretTracking removed in 1.6;
                };
            }
            Predicate<IAttackTarget> innerValidator = delegate (IAttackTarget t)
            {
                Thing thing = t.Thing;
                if (t == searcher)
                {
                    return false;
                }
                if (minDistSquared > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < minDistSquared)
                {
                    return false;
                }
                if (!canTakeTargetsCloserThanEffectiveMinRange)
                {
                    float num2 = verb.verbProps.EffectiveMinRange(thing, searcherThing);
                    if (num2 > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < num2 * num2)
                    {
                        return false;
                    }
                }
                if (maxTravelRadiusFromLocus < 9999f && (float)(thing.Position - locus).LengthHorizontalSquared > maxLocusDistSquared)
                {
                    return false;
                }
                /*
                if (!searcherThing.HostileTo(thing))
                {
                    return false;
                }
                */
                if (validator != null && !validator(thing))
                {
                    return false;
                }
                if (searcherPawn != null)
                {
                    Lord lord = searcherPawn.GetLord();
                    if (lord != null && !lord.LordJob.ValidateAttackTarget(searcherPawn, thing))
                    {
                        return false;
                    }
                }
                if ((byte)(flags & TargetScanFlags.NeedLOSToAll) != 0 && !searcherThing.CanSee(thing, losValidator))
                {
                    if (t is Pawn)
                    {
                        if ((byte)(flags & TargetScanFlags.NeedLOSToPawns) != 0)
                        {
                            return false;
                        }
                    }
                    else if ((byte)(flags & TargetScanFlags.NeedLOSToNonPawns) != 0)
                    {
                        return false;
                    }
                }
                if ((byte)(flags & TargetScanFlags.NeedThreat) != 0 && t.ThreatDisabled(searcher))
                {
                    return false;
                }
                Pawn pawn = t as Pawn;
                if (onlyTargetMachines && pawn != null && pawn.RaceProps.IsFlesh)
                {
                    return false;
                }
                if ((byte)(flags & TargetScanFlags.NeedNonBurning) != 0 && thing.IsBurning())
                {
                    return false;
                }
                if (searcherThing.def.race != null && searcherThing.def.race.intelligence >= Intelligence.Humanlike)
                {
                    CompExplosive compExplosive = thing.TryGetComp<CompExplosive>();
                    if (compExplosive != null && compExplosive.wickStarted)
                    {
                        return false;
                    }
                }
                if (thing.def.size.x == 1 && thing.def.size.z == 1)
                {
                    if (thing.Position.Fogged(thing.Map))
                    {
                        return false;
                    }
                }
                else
                {
                    bool flag2 = false;
                    using (CellRect.Enumerator enumerator = thing.OccupiedRect().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (!enumerator.Current.Fogged(thing.Map))
                            {
                                flag2 = true;
                                break;
                            }
                        }
                    }
                    if (!flag2)
                    {
                        return false;
                    }
                }
                return true;
            };
            if (XenomorphTargetFinder.HasRangedAttack(searcher))
            {
                XenomorphTargetFinder.tmpTargets.Clear();
                XenomorphTargetFinder.tmpTargets.AddRange(searcherThing.Map.attackTargetsCache.GetPotentialTargetsFor(searcher));
                if ((byte)(flags & TargetScanFlags.NeedReachable) != 0)
                {
                    Predicate<IAttackTarget> oldValidator = innerValidator;
                    innerValidator = ((IAttackTarget t) => oldValidator(t) && XenomorphTargetFinder.CanReach(searcherThing, t.Thing, canBash));
                }
                bool flag = false;
                for (int i = 0; i < XenomorphTargetFinder.tmpTargets.Count; i++)
                {
                    IAttackTarget attackTarget = XenomorphTargetFinder.tmpTargets[i];
                    if (attackTarget.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) && innerValidator(attackTarget) && XenomorphTargetFinder.CanShootAtFromCurrentPosition(attackTarget, searcher, verb))
                    {
                        flag = true;
                        break;
                    }
                }
                IAttackTarget result;
                if (flag)
                {
                    XenomorphTargetFinder.tmpTargets.RemoveAll((IAttackTarget x) => !x.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) || !innerValidator(x));
                    result = XenomorphTargetFinder.GetRandomShootingTargetByScore(XenomorphTargetFinder.tmpTargets, searcher, verb);
                }
                else
                {
                    Predicate<Thing> validator2;
                    if ((byte)(flags & TargetScanFlags.NeedReachableIfCantHitFromMyPos) != 0 && (byte)(flags & TargetScanFlags.NeedReachable) == 0)
                    {
                        validator2 = ((Thing t) => innerValidator((IAttackTarget)t) && (XenomorphTargetFinder.CanReach(searcherThing, t, canBash) || XenomorphTargetFinder.CanShootAtFromCurrentPosition((IAttackTarget)t, searcher, verb)));
                    }
                    else
                    {
                        validator2 = ((Thing t) => innerValidator((IAttackTarget)t));
                    }
                    result = (IAttackTarget)GenClosest.ClosestThing_Global(searcherThing.Position, XenomorphTargetFinder.tmpTargets, maxDist, validator2, null);
                }
                XenomorphTargetFinder.tmpTargets.Clear();
                return result;
            }
            if (searcherPawn != null && searcherPawn.mindState.duty != null && searcherPawn.mindState.duty.radius > 0f && !searcherPawn.InMentalState)
            {
                Predicate<IAttackTarget> oldValidator = innerValidator;
                innerValidator = ((IAttackTarget t) => oldValidator(t) && t.Thing.Position.InHorDistOf(searcherPawn.mindState.duty.focus.Cell, searcherPawn.mindState.duty.radius));
            }
            IntVec3 position = searcherThing.Position;
            Map map = searcherThing.Map;
            ThingRequest thingReq = ThingRequest.ForGroup(ThingRequestGroup.AttackTarget);
            PathEndMode peMode = PathEndMode.Touch;
            Pawn searcherPawn2 = searcherPawn;
            Danger maxDanger = Danger.Deadly;
            bool canBash2 = canBash;
            TraverseParms traverseParams = TraverseParms.For(searcherPawn2, maxDanger, TraverseMode.ByPawn, canBash2);
            float maxDist2 = maxDist;
            Predicate<Thing> validator3 = (Thing x) => innerValidator((IAttackTarget)x);
            int searchRegionsMax = (maxDist <= 800f) ? 40 : -1;
            IAttackTarget attackTarget2 = (IAttackTarget)GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, maxDist2, validator3, null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
            if (attackTarget2 != null && PawnUtility.ShouldCollideWithPawns(searcherPawn))
            {
                IAttackTarget attackTarget3 = XenomorphTargetFinder.FindBestReachableMeleeTarget(innerValidator, searcherPawn, maxDist, canBash);
                if (attackTarget3 != null)
                {
                    float lengthHorizontal = (searcherPawn.Position - attackTarget2.Thing.Position).LengthHorizontal;
                    float lengthHorizontal2 = (searcherPawn.Position - attackTarget3.Thing.Position).LengthHorizontal;
                    if (Mathf.Abs(lengthHorizontal - lengthHorizontal2) < 50f)
                    {
                        attackTarget2 = attackTarget3;
                    }
                }
            }
            return attackTarget2;
        }

        // Token: 0x06003F18 RID: 16152 RVA: 0x001D8BE4 File Offset: 0x001D6FE4
        private static bool CanReach(Thing searcher, Thing target, bool canBash)
        {
            Pawn pawn = searcher as Pawn;
            if (pawn != null)
            {
                if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Some, canBash, false))
                {
                    return false;
                }
            }
            else
            {
                TraverseMode mode = (!canBash) ? TraverseMode.NoPassClosedDoors : TraverseMode.PassDoors;
                if (!searcher.Map.reachability.CanReach(searcher.Position, target, PathEndMode.Touch, TraverseParms.For(mode, Danger.Deadly, false)))
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x06003F19 RID: 16153 RVA: 0x001D8C58 File Offset: 0x001D7058
        private static IAttackTarget FindBestReachableMeleeTarget(Predicate<IAttackTarget> validator, Pawn searcherPawn, float maxTargDist, bool canBash)
        {
            maxTargDist = Mathf.Min(maxTargDist, 30f);
            IAttackTarget reachableTarget = null;
            Func<IntVec3, IAttackTarget> bestTargetOnCell = delegate (IntVec3 x)
            {
                List<Thing> thingList = x.GetThingList(searcherPawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    IAttackTarget attackTarget = thing as IAttackTarget;
                    if (attackTarget != null)
                    {
                        if (validator(attackTarget))
                        {
                            if (ReachabilityImmediate.CanReachImmediate(x, thing, searcherPawn.Map, PathEndMode.Touch, searcherPawn))
                            {
                                if (searcherPawn.CanReachImmediate(thing, PathEndMode.Touch) || searcherPawn.Map.attackTargetReservationManager.CanReserve(searcherPawn, attackTarget))
                                {
                                    return attackTarget;
                                }
                            }
                        }
                    }
                }
                return null;
            };
            searcherPawn.Map.floodFiller.FloodFill(searcherPawn.Position, delegate (IntVec3 x)
            {
                if (!x.Walkable(searcherPawn.Map))
                {
                    return false;
                }
                if ((float)x.DistanceToSquared(searcherPawn.Position) > maxTargDist * maxTargDist)
                {
                    return false;
                }
                if (!canBash)
                {
                    Building_Door building_Door = x.GetEdifice(searcherPawn.Map) as Building_Door;
                    if (building_Door != null && !building_Door.CanPhysicallyPass(searcherPawn))
                    {
                        return false;
                    }
                }
                return !PawnUtility.AnyPawnBlockingPathAt(x, searcherPawn, true, false, false);
            }, delegate (IntVec3 x)
            {
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 intVec = x + GenAdj.AdjacentCells[i];
                    if (intVec.InBounds(searcherPawn.Map))
                    {
                        IAttackTarget attackTarget = bestTargetOnCell(intVec);
                        if (attackTarget != null)
                        {
                            reachableTarget = attackTarget;
                            break;
                        }
                    }
                }
                return reachableTarget != null;
            }, int.MaxValue, false, null);
            return reachableTarget;
        }

        // Token: 0x06003F1A RID: 16154 RVA: 0x001D8CFC File Offset: 0x001D70FC
        private static bool HasRangedAttack(IAttackTargetSearcher t)
        {
            Verb currentEffectiveVerb = t.CurrentEffectiveVerb;
            return currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack;
        }

        // Token: 0x06003F1B RID: 16155 RVA: 0x001D8D27 File Offset: 0x001D7127
        private static bool CanShootAtFromCurrentPosition(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            return verb != null && verb.CanHitTargetFrom(searcher.Thing.Position, target.Thing);
        }

        // Token: 0x06003F1C RID: 16156 RVA: 0x001D8D50 File Offset: 0x001D7150
        private static IAttackTarget GetRandomShootingTargetByScore(List<IAttackTarget> targets, IAttackTargetSearcher searcher, Verb verb)
        {
            Pair<IAttackTarget, float> pair;
            if (XenomorphTargetFinder.GetAvailableShootingTargetsByScore(targets, searcher, verb).TryRandomElementByWeight((Pair<IAttackTarget, float> x) => x.Second, out pair))
            {
                return pair.First;
            }
            return null;
        }

        // Token: 0x06003F1D RID: 16157 RVA: 0x001D8D98 File Offset: 0x001D7198
        private static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(List<IAttackTarget> rawTargets, IAttackTargetSearcher searcher, Verb verb)
        {
            XenomorphTargetFinder.availableShootingTargets.Clear();
            if (rawTargets.Count == 0)
            {
                return XenomorphTargetFinder.availableShootingTargets;
            }
            XenomorphTargetFinder.tmpTargetScores.Clear();
            XenomorphTargetFinder.tmpCanShootAtTarget.Clear();
            float num = 0f;
            IAttackTarget attackTarget = null;
            for (int i = 0; i < rawTargets.Count; i++)
            {
                XenomorphTargetFinder.tmpTargetScores.Add(float.MinValue);
                XenomorphTargetFinder.tmpCanShootAtTarget.Add(false);
                if (rawTargets[i] != searcher)
                {
                    bool flag = XenomorphTargetFinder.CanShootAtFromCurrentPosition(rawTargets[i], searcher, verb);
                    XenomorphTargetFinder.tmpCanShootAtTarget[i] = flag;
                    if (flag)
                    {
                        float shootingTargetScore = XenomorphTargetFinder.GetShootingTargetScore(rawTargets[i], searcher, verb);
                        XenomorphTargetFinder.tmpTargetScores[i] = shootingTargetScore;
                        if (attackTarget == null || shootingTargetScore > num)
                        {
                            attackTarget = rawTargets[i];
                            num = shootingTargetScore;
                        }
                    }
                }
            }
            if (num < 1f)
            {
                if (attackTarget != null)
                {
                    XenomorphTargetFinder.availableShootingTargets.Add(new Pair<IAttackTarget, float>(attackTarget, 1f));
                }
            }
            else
            {
                float num2 = num - 30f;
                for (int j = 0; j < rawTargets.Count; j++)
                {
                    if (rawTargets[j] != searcher)
                    {
                        if (XenomorphTargetFinder.tmpCanShootAtTarget[j])
                        {
                            float num3 = XenomorphTargetFinder.tmpTargetScores[j];
                            if (num3 >= num2)
                            {
                                float second = Mathf.InverseLerp(num - 30f, num, num3);
                                XenomorphTargetFinder.availableShootingTargets.Add(new Pair<IAttackTarget, float>(rawTargets[j], second));
                            }
                        }
                    }
                }
            }
            return XenomorphTargetFinder.availableShootingTargets;
        }

        // Token: 0x06003F1E RID: 16158 RVA: 0x001D8F3C File Offset: 0x001D733C
        private static float GetShootingTargetScore(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            float num = 60f;
            num -= Mathf.Min((target.Thing.Position - searcher.Thing.Position).LengthHorizontal, 40f);
            if (target.TargetCurrentlyAimingAt == searcher.Thing)
            {
                num += 10f;
            }
            if (searcher.LastAttackedTarget == target.Thing && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
            {
                num += 40f;
            }
            num -= CoverUtility.CalculateOverallBlockChance(target.Thing.Position, searcher.Thing.Position, searcher.Thing.Map) * 10f;
            Pawn pawn = target as Pawn;
            if (pawn != null && pawn.RaceProps.Animal && pawn.Faction != null && !pawn.IsFighting())
            {
                num -= 50f;
            }
            num += XenomorphTargetFinder.FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, verb);
            return num + XenomorphTargetFinder.FriendlyFireConeTargetScoreOffset(target, searcher, verb);
        }

        // Token: 0x06003F1F RID: 16159 RVA: 0x001D9068 File Offset: 0x001D7468
        private static float FriendlyFireBlastRadiusTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            if (verb.verbProps.ai_AvoidFriendlyFireRadius <= 0f)
            {
                return 0f;
            }
            Map map = target.Thing.Map;
            IntVec3 position = target.Thing.Position;
            int num = GenRadial.NumCellsInRadius(verb.verbProps.ai_AvoidFriendlyFireRadius);
            float num2 = 0f;
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map))
                {
                    bool flag = true;
                    List<Thing> thingList = intVec.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j] is IAttackTarget && thingList[j] != target)
                        {
                            if (flag)
                            {
                                if (!GenSight.LineOfSight(position, intVec, map, true, null, 0, 0))
                                {
                                    break;
                                }
                                flag = false;
                            }
                            float num3;
                            if (thingList[j] == searcher)
                            {
                                num3 = 40f;
                            }
                            else if (thingList[j] is Pawn)
                            {
                                num3 = ((!thingList[j].def.race.Animal) ? 18f : 7f);
                            }
                            else
                            {
                                num3 = 10f;
                            }
                            if (searcher.Thing.HostileTo(thingList[j]))
                            {
                                num2 += num3 * 0.6f;
                            }
                            else
                            {
                                num2 -= num3;
                            }
                        }
                    }
                }
            }
            return num2;
        }

        // Token: 0x06003F20 RID: 16160 RVA: 0x001D9208 File Offset: 0x001D7608
        private static float FriendlyFireConeTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            Pawn pawn = searcher.Thing as Pawn;
            if (pawn == null)
            {
                return 0f;
            }
            if (pawn.RaceProps.intelligence < Intelligence.ToolUser)
            {
                return 0f;
            }
            if (pawn.RaceProps.IsMechanoid)
            {
                return 0f;
            }
            Verb_Shoot verb_Shoot = verb as Verb_Shoot;
            if (verb_Shoot == null)
            {
                return 0f;
            }
            ThingDef defaultProjectile = verb_Shoot.verbProps.defaultProjectile;
            if (defaultProjectile == null)
            {
                return 0f;
            }
            if (defaultProjectile.projectile.flyOverhead)
            {
                return 0f;
            }
            Map map = pawn.Map;
            ShotReport report = ShotReport.HitReportFor(pawn, verb, (Thing)target);
            float a = VerbUtility.CalculateAdjustedForcedMiss(verb.verbProps.ForcedMissRadius, report.ShootLine.Dest - report.ShootLine.Source);
            float radius = Mathf.Max(a, 1.5f);
            IntVec3 dest2 = report.ShootLine.Dest;
            IEnumerable<IntVec3> source = from dest in GenRadial.RadialCellsAround(dest2, radius, true)
                                          where dest.InBounds(map)
                                          select dest;
            IEnumerable<ShootLine> source2 = from dest in source
                                             select new ShootLine(report.ShootLine.Source, dest);
            IEnumerable<IntVec3> source3 = source2.SelectMany((ShootLine line) => line.Points().Concat(line.Dest).TakeWhile((IntVec3 pos) => pos.CanBeSeenOverFast(map)));
            IEnumerable<IntVec3> enumerable = source3.Distinct<IntVec3>();
            float num = 0f;
            foreach (IntVec3 c in enumerable)
            {
                float num2 = VerbUtility.InterceptChanceFactorFromDistance(report.ShootLine.Source.ToVector3Shifted(), c);
                if (num2 > 0f)
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Thing thing = thingList[i];
                        if (thing is IAttackTarget && thing != target)
                        {
                            float num3;
                            if (thing == searcher)
                            {
                                num3 = 40f;
                            }
                            else if (thing is Pawn)
                            {
                                num3 = ((!thing.def.race.Animal) ? 18f : 7f);
                            }
                            else
                            {
                                num3 = 10f;
                            }
                            num3 *= num2;
                            if (searcher.Thing.HostileTo(thing))
                            {
                                num3 *= 0.6f;
                            }
                            else
                            {
                                num3 *= -1f;
                            }
                            num += num3;
                        }
                    }
                }
            }
            return num;
        }

        // Token: 0x06003F21 RID: 16161 RVA: 0x001D94E8 File Offset: 0x001D78E8
        public static IAttackTarget BestShootTargetFromCurrentPosition(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDistance = 0f, float maxDistance = 9999f)
        {
            Verb currentEffectiveVerb = searcher.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null)
            {
                Log.Error("BestShootTargetFromCurrentPosition with " + searcher.ToStringSafe<IAttackTargetSearcher>() + " who has no attack verb.");
                return null;
            }
            return XenomorphTargetFinder.BestAttackTarget(searcher, flags, validator, Mathf.Max(minDistance, currentEffectiveVerb.verbProps.minRange), Mathf.Min(maxDistance, currentEffectiveVerb.verbProps.range), default(IntVec3), float.MaxValue, false, false);
        }

        // Token: 0x06003F22 RID: 16162 RVA: 0x001D955C File Offset: 0x001D795C
        public static bool CanSeeTarget(this Thing seer, Thing target, Func<IntVec3, bool> validator = null)
        {
            ShootLeanUtility.CalcShootableCellsOf(XenomorphTargetFinder.tempDestList, target, searcherThing.Position);
            for (int i = 0; i < XenomorphTargetFinder.tempDestList.Count; i++)
            {
                if (GenSight.LineOfSight(seer.Position, XenomorphTargetFinder.tempDestList[i], seer.Map, true, validator, 0, 0))
                {
                    return true;
                }
            }
            ShootLeanUtility.LeanShootingSourcesFromTo(seer.Position, target.Position, seer.Map, XenomorphTargetFinder.tempSourceList);
            for (int j = 0; j < XenomorphTargetFinder.tempSourceList.Count; j++)
            {
                for (int k = 0; k < XenomorphTargetFinder.tempDestList.Count; k++)
                {
                    if (GenSight.LineOfSight(XenomorphTargetFinder.tempSourceList[j], XenomorphTargetFinder.tempDestList[k], seer.Map, true, validator, 0, 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Token: 0x06003F23 RID: 16163 RVA: 0x001D9638 File Offset: 0x001D7A38
        public static void DebugDrawAttackTargetScores_Update()
        {
            IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
            if (attackTargetSearcher == null)
            {
                return;
            }
            if (attackTargetSearcher.Thing.Map != Find.CurrentMap)
            {
                return;
            }
            Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null)
            {
                return;
            }
            XenomorphTargetFinder.tmpTargets.Clear();
            List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
            for (int i = 0; i < list.Count; i++)
            {
                XenomorphTargetFinder.tmpTargets.Add((IAttackTarget)list[i]);
            }
            List<Pair<IAttackTarget, float>> availableShootingTargetsByScore = XenomorphTargetFinder.GetAvailableShootingTargetsByScore(XenomorphTargetFinder.tmpTargets, attackTargetSearcher, currentEffectiveVerb);
            for (int j = 0; j < availableShootingTargetsByScore.Count; j++)
            {
                GenDraw.DrawLineBetween(attackTargetSearcher.Thing.DrawPos, availableShootingTargetsByScore[j].First.Thing.DrawPos);
            }
        }

        // Token: 0x06003F24 RID: 16164 RVA: 0x001D9728 File Offset: 0x001D7B28
        public static void DebugDrawAttackTargetScores_OnGUI()
        {
            IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
            if (attackTargetSearcher == null)
            {
                return;
            }
            if (attackTargetSearcher.Thing.Map != Find.CurrentMap)
            {
                return;
            }
            Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null)
            {
                return;
            }
            List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (thing != attackTargetSearcher)
                {
                    string text;
                    Color red;
                    if (!XenomorphTargetFinder.CanShootAtFromCurrentPosition((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb))
                    {
                        text = "out of range";
                        red = Color.red;
                    }
                    else
                    {
                        text = XenomorphTargetFinder.GetShootingTargetScore((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb).ToString("F0");
                        red = new Color(0.25f, 1f, 0.25f);
                    }
                    Vector2 screenPos = thing.DrawPos.MapToUIPosition();
                    GenMapUI.DrawThingLabel(screenPos, text, red);
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        // Token: 0x0400280B RID: 10251
        private const float FriendlyFireScoreOffsetPerHumanlikeOrMechanoid = 18f;

        // Token: 0x0400280C RID: 10252
        private const float FriendlyFireScoreOffsetPerAnimal = 7f;

        // Token: 0x0400280D RID: 10253
        private const float FriendlyFireScoreOffsetPerNonPawn = 10f;

        // Token: 0x0400280E RID: 10254
        private const float FriendlyFireScoreOffsetSelf = 40f;

        // Token: 0x0400280F RID: 10255
        private static List<IAttackTarget> tmpTargets = new List<IAttackTarget>();

        // Token: 0x04002810 RID: 10256
        private static List<Pair<IAttackTarget, float>> availableShootingTargets = new List<Pair<IAttackTarget, float>>();

        // Token: 0x04002811 RID: 10257
        private static List<float> tmpTargetScores = new List<float>();

        // Token: 0x04002812 RID: 10258
        private static List<bool> tmpCanShootAtTarget = new List<bool>();

        // Token: 0x04002813 RID: 10259
        private static List<IntVec3> tempDestList = new List<IntVec3>();

        // Token: 0x04002814 RID: 10260
        private static List<IntVec3> tempSourceList = new List<IntVec3>();
    }
}
