using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x020000E4 RID: 228
    public class JobGiver_Neomorph_GetFood : ThinkNode_JobGiver
    {
        // Token: 0x060004FB RID: 1275 RVA: 0x00032140 File Offset: 0x00030540
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Neomorph_GetFood jobGiver_XenoGetFood = (JobGiver_Neomorph_GetFood)base.DeepCopy(resolve);
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
            /*
            if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
            {
                return 0f;
            }
            */
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
            bool selected = Find.Selector.SelectedObjects.Contains(pawn) && Prefs.DevMode;
            Need_Food food = pawn.needs.food;
            LifeStageDef stage = pawn.ageTracker.CurLifeStage;
            bool desperate = food.CurCategory >= HungerCategory.Starving;
            bool canRefillDispenser = false;
            bool canUseInventory = false;
            bool allowCorpse = true;
            bool flag3 = this.forceScanWholeMap;

            if (stage == pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].def)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("adult {0} @ {1}", pawn, pawn.Position));
                if (food.CurCategory == HungerCategory.Fed)
                {
                    return null;
                }
            }
            else
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("young {0} @ {1}", pawn, pawn.Position));
                /*
                if (food.CurCategory == HungerCategory.Fed)
                {
                    return null;
                }
                */
            }
            
            float nutrition;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out Thing thing, out ThingDef thingDef, canRefillDispenser, canUseInventory, true, allowCorpse, true, pawn.IsWildMan(), flag3))
            {
                return null;
            }
            if (thing is Corpse corpse)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} is corpse @ {1}", thing, thing.Position));
                nutrition = FoodUtility.GetNutrition(pawn, thing, thingDef);
                return new Job(XenomorphDefOf.RRY_Neomorph_Ingest, thing)
                {
                    count = FoodUtility.WillIngestStackCountOf(pawn, thingDef, nutrition)
                };
            }
            if (thing is Pawn pawn2)
            {
                if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} is pawn @ {1}", thing, thing.Position));
                if (pawn2 != null)
                {
                    return new Job(JobDefOf.PredatorHunt, pawn2)
                    {
                        killIncappedTarget = true
                    };
                }
            }
            nutrition = FoodUtility.GetNutrition(pawn, thing, thingDef);
            if (pawn.jobs.debugLog) pawn.jobs.DebugLogEvent(string.Format("{0} is thing @ {1}", thing, thing.Position));
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
