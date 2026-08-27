using RRYautja.ExtensionMethods;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    // Token: 0x020000E4 RID: 228
    public class JobGiver_Xenomorph_GetFood : ThinkNode_JobGiver
    {
        // Token: 0x060004FB RID: 1275 RVA: 0x00032140 File Offset: 0x00030540
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Xenomorph_GetFood jobGiver_XenoGetFood = (JobGiver_Xenomorph_GetFood)base.DeepCopy(resolve);
            jobGiver_XenoGetFood.minCategory = this.minCategory;
            jobGiver_XenoGetFood.forceScanWholeMap = this.forceScanWholeMap;
            return jobGiver_XenoGetFood;
        }

        // Token: 0x060004FC RID: 1276 RVA: 0x00032174 File Offset: 0x00030574
        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null)
            {
                return 0f;
            }
            if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
            {
                return 0f;
            }
            if (food.CurCategory < this.minCategory)
            {
                return 0f;
            }
            if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
            {
                return 9.5f;
            }
            return 0f;
        }

        // Token: 0x060004FD RID: 1277 RVA: 0x000321F8 File Offset: 0x000305F8
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null) return null;
            Need_Food food = pawn.needs.food;
            LifeStageDef stage = pawn.ageTracker.CurLifeStage;
            
            if (stage == pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def)
            {
                return null;
            }
            
            bool flag;
            if (pawn.AnimalOrWildMan())
            {
                flag = true;
            }
            else
            {
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
                flag = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
            }
            bool flag2 = pawn.needs.food.CurCategory == HungerCategory.Starving;
            bool desperate = flag2;
            Thing thing;
            ThingDef thingDef;
            bool canRefillDispenser = false;
            bool canUseInventory = false;
            bool allowCorpse = true;
            bool flag3 = false;
            if (pawn.GetLord() != null && pawn.GetLord() is Lord L)
            {
                if (L.CurLordToil is LordToil_DefendAndExpandHiveLike Hivelord)
                {
                    if (Hivelord.Data.assignedHiveLikes.TryGetValue(pawn) != null)
                    {
                        if (!Hivelord.Data.assignedHiveLikes.TryGetValue(pawn).active)
                        {
                            return null;
                        }
                    }
                }
            }
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out thingDef, canRefillDispenser, canUseInventory, true, allowCorpse, true, pawn.IsWildMan(), flag3))
            {
                return null;
            }
            if (thing.GetType() != typeof(Corpse))
            {
                return null;
            }
            Pawn pawn2 = thing as Pawn;
            if (pawn2 != null && pawn.CanSee(pawn2) && pawn.isXenomorph())
            {
                return new Job(JobDefOf.PredatorHunt, pawn2)
                {
                    killIncappedTarget = true
                };
            }
            float nutrition = FoodUtility.GetNutrition(pawn, thing, thingDef);
            return new Job(XenomorphDefOf.RRY_Neomorph_Ingest, thing)
            {
                count = FoodUtility.WillIngestStackCountOf(pawn, thingDef, nutrition)
            };
        }
        // Token: 0x040002BD RID: 701
        private HungerCategory minCategory = HungerCategory.Starving;

        // Token: 0x040002BE RID: 702
        public bool forceScanWholeMap = false;
    }
}
