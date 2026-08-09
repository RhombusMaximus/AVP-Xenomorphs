using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RRYautja
{
    public class CompProperties_Neomorph : CompProperties
    {
        public CompProperties_Neomorph()
        {
            this.compClass = typeof(Comp_Neomorph);
        }

    }

    public class Comp_Neomorph : ThingComp
    { 
        public CompProperties_Neomorph Props
        {
            get
            {
                return (CompProperties_Neomorph)this.props;
            }
        }

        public Pawn pawn
        {
            get
            {
                return base.parent as Pawn;
            }
        }

        public bool Hidden = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksSinceHeal, "ticksSinceHeal");
            Scribe_Values.Look<bool>(ref this.Hidden, "Hidden");
        }

        public override void CompTickRare()
        {
            base.CompTickRare();


        }

        public int healIntervalTicks = 50;
        public override void CompTick()
        {
            /*
            if (parent.Faction == null)
            {
                parent.SetFaction(Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph));
            }
            */
            base.CompTick();
            this.ticksSinceHeal++;
            bool flag = this.ticksSinceHeal > this.healIntervalTicks;
            if (flag)
            {
                bool flag2 = pawn.health.hediffSet.HasNaturallyHealingInjury();
                if (flag2)
                {
                    this.ticksSinceHeal = 0;
                    float num = 10f;
                    num = num * pawn.HealthScale * 0.01f;
                    Need_Food food = pawn.needs.food;
                    if (food.CurLevel > (num / 100))
                    {
                        Hediff_Injury hediff_Injury = GenCollection.RandomElement<Hediff_Injury>
                            (from x in pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
                             where HediffUtility.CanHealNaturally(x)
                             select x);
                        doClot(pawn);
                        if (hediff_Injury!=null)
                        {
                            hediff_Injury.Heal(num);
                            food.CurLevel -= (num / 100);
                            string text = string.Format("{0} healed {1} for {2} and food reduced to {3}", pawn.LabelCap, hediff_Injury.Label, num, food.CurLevel);
                            bool selected = Find.Selector.SingleSelectedThing == pawn;
                            if (selected && Prefs.DevMode)
                            {
                            //    Log.Message(text);
                            }
                        }
                    }
                }
            }
        }

        public static void doClot(Pawn pawn, BodyPartRecord part = null)
        {
            var i = 5;
            foreach (var hediff in pawn.health.hediffSet.hediffs.Where(x => x.Bleeding).OrderByDescending(x => x.BleedRate))
            {
                hediff.Tended(Math.Min(Rand.Value + Rand.Value + Rand.Value, 1f));
                i--;

                if (i <= 0) return;
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {

            Pawn other = dinfo.Instigator as Pawn;
            Pawn pawn = base.parent as Pawn;
            
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

        }

        // Token: 0x0600017C RID: 380 RVA: 0x0000DB3C File Offset: 0x0000BF3C
        public bool TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false)
        {
            bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            bool flag2 = !eater.IsTeetotaler();
            Thing thing = null;
            ThingDef thingDef = null;
            ThingDef foodDef2 = thingDef;
            bool allowPlant = getter == eater;
            Thing thing2 = FoodUtility.BestFoodSourceOnMap(getter, eater, desperate, out foodDef2, FoodPreferability.Undefined, allowPlant, flag2, allowCorpse, true, canRefillDispenser, allowForbidden, allowSociallyImproper, allowHarvest, forceScanWholeMap);
            if (thing == null && thing2 == null)
            {
                if (canUseInventory && flag)
                {
                    FoodPreferability minFoodPref = FoodPreferability.DesperateOnly;
                    bool allowDrug = flag2;
                    thing = FoodUtility.BestFoodInInventory(getter, eater, minFoodPref, FoodPreferability.MealLavish, 0f, allowDrug);
                    if (thing != null)
                    {
                        foodSource = thing;
                        foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
                        return true;
                    }
                }
                if (thing2 == null && getter == eater && (getter.RaceProps.predator || (getter.IsWildMan() && !getter.IsPrisoner)))
                {
                    Pawn pawn = BestPawnToHuntForPredator(getter, forceScanWholeMap);
                    if (pawn != null)
                    {
                        foodSource = pawn;
                        foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
                        return true;
                    }
                }
                foodSource = null;
                foodDef = null;
                return false;
            }
            if (thing == null && thing2 != null)
            {
                foodSource = thing2;
                foodDef = thingDef;
                return true;
            }
            ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(thing, false);
            if (thing2 == null)
            {
                foodSource = thing;
                foodDef = finalIngestibleDef;
                return true;
            }
            float num = FoodUtility.FoodOptimality(eater, thing2, thingDef, (float)(getter.Position - thing2.Position).LengthManhattan, false);
            float num2 = FoodUtility.FoodOptimality(eater, thing, finalIngestibleDef, 0f, false);
            num2 -= 32f;
            if (num > num2)
            {
                foodSource = thing2;
                foodDef = thingDef;
                return true;
            }
            foodSource = thing;
            foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
            return true;
        }
        // Token: 0x06000180 RID: 384 RVA: 0x0000E414 File Offset: 0x0000C814
        private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
        {
            if (getter.RaceProps.Humanlike)
            {
                return -1;
            }
            if (forceScanWholeMap)
            {
                return -1;
            }
            if (getter.Faction == Faction.OfPlayer)
            {
                return 100;
            }
            return 30;
        }

        // Token: 0x06000186 RID: 390 RVA: 0x0000E940 File Offset: 0x0000CD40
        public Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap)
        {
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
            {
                return null;
            }
            bool flag = false;
            float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
            if (summaryHealthPercent < 0.5f)
            {
                flag = true;
            }
            tmpPredatorCandidates.Clear();
            int maxRegionsToScan = GetMaxRegionsToScan(predator, forceScanWholeMap);
            if (maxRegionsToScan < 0)
            {
                tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
            }
            else
            {
                TraverseParms traverseParms = TraverseParms.For(predator, Danger.Deadly, TraverseMode.ByPawn, false);
                RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, true), delegate (Region x)
                {
                    List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                    for (int j = 0; j < list.Count; j++)
                    {
                        tmpPredatorCandidates.Add((Pawn)list[j]);
                    }
                    return false;
                }, 999999, RegionType.Set_Passable);
            }
            Pawn pawn = null;
            float num = 0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            for (int i = 0; i < tmpPredatorCandidates.Count; i++)
            {
                Pawn pawn2 = tmpPredatorCandidates[i];
                if (predator.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
                {
                    if (predator != pawn2)
                    {
                        if (!flag || pawn2.Downed)
                        {
                            if (IsAcceptablePreyFor(predator, pawn2))
                            {
                                if (predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                                {
                                    if (!pawn2.IsForbidden(predator))
                                    {
                                        if (!tutorialMode || pawn2.Faction != Faction.OfPlayer)
                                        {
                                            float preyScoreFor = GetPreyScoreFor(predator, pawn2);
                                            if (preyScoreFor > num || pawn == null)
                                            {
                                                num = preyScoreFor;
                                                pawn = pawn2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            tmpPredatorCandidates.Clear();
            bool selected = Find.Selector.SingleSelectedThing == predator && Prefs.DevMode;
            if (selected)
            {
            //    Log.Message(string.Format("{0} hunting: {1}, @: {2}", predator.LabelShortCap, pawn.LabelShortCap, pawn.Position));
            }
            return pawn;
        }

        // Token: 0x06000187 RID: 391 RVA: 0x0000EB14 File Offset: 0x0000CF14
        public static bool IsAcceptablePreyFor(Pawn predator, Pawn prey)
        {
            if (!prey.RaceProps.canBePredatorPrey)
            {
                return false;
            }
            if (!prey.RaceProps.IsFlesh)
            {
                return false;
            }
            /*
            if (!Find.Storyteller.difficulty.predatorsHuntHumanlikes && prey.RaceProps.Humanlike)
            {
                return false;
            }
            */
            if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
            {
                return false;
            }
            if (!prey.Downed)
            {
                if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
                {
                    return false;
                }
                float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.ageTracker.CurLifeStage.bodySizeFactor;
                float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor;
                if (num >= num2)
                {
                    return false;
                }
            }
            return (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (predator.Faction == null || prey.HostFaction == null || predator.HostileTo(prey)) && (predator.Faction != Faction.OfPlayer || prey.Faction != Faction.OfPlayer) && (!predator.RaceProps.herdAnimal || predator.def != prey.def);
        }

        // Token: 0x06000188 RID: 392 RVA: 0x0000ECA4 File Offset: 0x0000D0A4
        public static float GetPreyScoreFor(Pawn predator, Pawn prey)
        {
            float num = prey.kindDef.combatPower / predator.kindDef.combatPower;
            float num2 = prey.health.summaryHealth.SummaryHealthPercent;
            float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor / predator.ageTracker.CurLifeStage.bodySizeFactor;
            float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
            if (prey.Downed)
            {
                num2 = Mathf.Min(num2, 0.2f);
            }
            float num3 = -lengthHorizontal - 56f * num2 * num2 * num * bodySizeFactor;
            float num4 = -56f * num2 * num2 * num * bodySizeFactor;
            if (prey.isHost())
            {
                if (prey.isXenoHost())
                {
                    num3 -= 25f;
                }
                if (prey.isNeoHost())
                {
                    num3 -= 35f;
                }
            }
            if (prey.isXenomorph())
            {
                num3 -= 250f;
            }
            if (prey.isNeomorph())
            {
                num3 -= 35f;
            }
            if (prey.isPotentialHost())
            {
                num3 -= 20f;
            }
            if (prey.RaceProps.Humanlike)
            {
                num3 -= 35f;
            }
            bool selected = Find.Selector.SelectedObjects.Contains(predator) && Prefs.DevMode;
            if (selected)
            {
            //    Log.Message(string.Format("{0} found: {1} @: {2}\nPreyScore: {3}, BFPreyScore: {4}, isXenoHost: {5}, isNeoHost: {6}, isXenomorph: {7}, isNeomorph: {8}, isPotentialHost: {9}, Humanlike: {10}", predator.LabelShortCap, prey.LabelShortCap, prey.Position, num3, num4, prey.isXenoHost(), prey.isNeoHost(), prey.isXenomorph(), prey.isNeomorph(), prey.isPotentialHost(), prey.RaceProps.Humanlike));
            }
            return num3;
        }

        private static List<Pawn> tmpPredatorCandidates = new List<Pawn>();
        public PawnKindDef host;
        public int ticksSinceHeal;
    }
}
