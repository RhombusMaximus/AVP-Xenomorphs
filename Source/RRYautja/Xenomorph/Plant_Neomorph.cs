using System;
using System.Collections.Generic;
using RimWorld;
using RRYautja;
using RRYautja.ExtensionMethods;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RRYautja
{
    // Token: 0x02000B78 RID: 2936
    public class Plant_Neomorph : Plant
    {
        public float radius = 6f;

        public override bool IngestibleNow => false;

        public override bool HarvestableNow => false;

        public override void TickLong()
        {
            base.TickLong();
            bool selected = Find.Selector.SingleSelectedThing == this;
            if (!this.IsBurning()&& !this.Destroyed && this.Map!=null)
            {
                Thing thing = GenClosest.ClosestThingReachable(this.Position, this.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), radius, x => x.isPotentialHost(), null, 0, -1, false, RegionType.Set_Passable, false);
                if (thing != null && this.Growth > 0.95f && !thing.Destroyed && !((Pawn)thing).Dead)
                {
                    List<Thing> thingList = GridsUtility.GetThingList(thing.Position, this.Map);
                    Thing thing2;


                    if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Fungus))
                    {
                        thing2 = ThingMaker.MakeThing(XenomorphDefOf.RRY_Neomorph_Spores_Hidden);
                    }
                    else
                    {
                        thing2 = ThingMaker.MakeThing(XenomorphDefOf.RRY_Neomorph_Spores);
                    }

                    float Chance = ((0.5f * Growth) * ((Pawn)thing).BodySize) / DistanceBetween(this.Position, thing.Position);
                    //    if (selected) Log.Message(string.Format("Chance: {0}", Chance));
                    
                    if (Rand.Chance(Chance) && !thingList.Exists(x => x.def == XenomorphDefOf.RRY_Neomorph_Spores) && this.CanSee(thing))
                    {
                        if (thing.Faction == Faction.OfPlayer)
                        {
                            string text = TranslatorFormattedStringExtensions.Translate("Xeno_Neospores_Trigger", thing.LabelShortCap, this.Label);
                        //    Log.Message(text);
                            MoteMaker.ThrowText(this.Position.ToVector3(), this.Map, text, 5f);
                        }
                        GenSpawn.Spawn(thing2, thing.Position, this.Map);
                    }
                }
                else
                {
                    HarmRandomPlantInRadius(radius/2);
                }
            }
        }

        // Token: 0x0600295A RID: 10586 RVA: 0x00139AB0 File Offset: 0x00137EB0
        private void HarmRandomPlantInRadius(float radius)
        {
            IntVec3 c = this.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
            if (!c.InBounds(this.Map))
            {
                return;
            }
            Plant plant = c.GetPlant(this.Map);
            bool flag = c.GetThingList(this.Map).Any(x => x.def == XenomorphDefOf.RRY_Plant_Neomorph_Fungus_Hidden || x.def == XenomorphDefOf.RRY_Plant_Neomorph_Fungus);
            if (plant != null && !flag)
            {
                //   Log.Message(string.Format("this.CanSee(plant): {0}", this.CanSee(plant)));
                if (Rand.Value < this.LeaflessPlantKillChance && this.CanSee(plant))
                {
                    Thing thing2;

                    if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Fungus))
                    {
                        thing2 = ThingMaker.MakeThing(XenomorphDefOf.RRY_Plant_Neomorph_Fungus_Hidden);
                    }
                    else
                    {
                        thing2 = ThingMaker.MakeThing(XenomorphDefOf.RRY_Plant_Neomorph_Fungus);
                    }
                    IntVec3 vec3 = plant.Position;
                    GenSpawn.Spawn(thing2, vec3, plant.Map, WipeMode.Vanish);
                    //    plant.Destroy();
                    //    GenSpawn.Spawn(ThingMaker.MakeThing(this.def), vec3, this.Map);
                }
            }
        }
        
        public new float GrowthRateFactor_Temperature
        {
            get
            {
                float num;
                if (!GenTemperature.TryGetTemperatureForCell(base.Position, base.Map, out num))
                {
                    return 1f;
                }
                /*
                if (num < 10f)
                {
                    return Mathf.InverseLerp(0f, 10f, num);
                }
                */
                if (num > 42f)
                {
                    return Mathf.InverseLerp(58f, 42f, num);
                }
                return 1f;
            }
        }

        public new bool LeaflessNow
        {
            get
            {
                return false;
            }
        }

        public override void MakeLeafless(LeaflessCause cause)
        {
            return;
        }

        public override ushort PathFindCostFor(Pawn p)
        {
            if (!XenomorphUtil.IsXenomorph(p))
            {
                if (p.Faction == Faction.OfPlayer)
                {
                    if (!PlayerKnowledgeDatabase.IsComplete(XenomorphConceptDefOf.RRY_Concept_Fungus) && p.Spawned && p.IsColonist)
                    {
                        return base.PathFindCostFor(p);
                    }
                    return 800;
                }
            }
            return base.PathFindCostFor(p);
        }

        public float DistanceBetween(IntVec3 a, IntVec3 b)
        {
            double distance = GetDistance(a.x, a.z, b.x, b.z);
            return (float)distance;
        }

        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }
        
        private float LeaflessPlantKillChance = 0.09f;
    }
}
