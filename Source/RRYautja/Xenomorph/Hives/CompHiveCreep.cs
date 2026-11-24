using RRYautja;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
    // Token: 0x02000259 RID: 601
    public class CompProperties_HiveCreep : CompProperties
    {
        // Token: 0x06000AC7 RID: 2759 RVA: 0x0005629B File Offset: 0x0005469B
        public CompProperties_HiveCreep()
        {
            this.compClass = typeof(CompHiveCreep);
        }

        // Token: 0x040004CB RID: 1227
        public int expandInterval = 500;

        // Token: 0x040004CC RID: 1228
        public float addAmount = 0.12f;

        // Token: 0x040004CD RID: 1229
        public float maxRadius = 10f;
    }

    // Token: 0x02000762 RID: 1890
    public class CompHiveCreep : ThingComp
    {
        // Token: 0x17000675 RID: 1653
        // (get) Token: 0x060029BE RID: 10686 RVA: 0x0013C3DA File Offset: 0x0013A7DA
        public CompProperties_HiveCreep Props
        {
            get
            {
                return (CompProperties_HiveCreep)this.props;
            }
        }

        // Token: 0x060029BF RID: 10687 RVA: 0x0013C3E7 File Offset: 0x0013A7E7
        public override void PostExposeData()
        {
            Scribe_Values.Look<float>(ref this.hiveRadius, "hiveRadius", 0f, false);
        }

        // Token: 0x060029C0 RID: 10688 RVA: 0x0013C3FF File Offset: 0x0013A7FF
        public override void CompTick()
        {
            if (!this.parent.Spawned)
            {
                return;
            }
            if (this.parent.IsHashIntervalTick(this.Props.expandInterval))
            {
                this.TryExpandHive();
            }
        }

        public float maxRadius
        {
            get
            {
                if (Props.maxRadius != 0f && Props.maxRadius < GenRadial.MaxRadialPatternRadius)
                {
                    return Props.maxRadius;
                }
                else
                {
                    return GenRadial.MaxRadialPatternRadius-1;
                }
            }
            set
            {

            }
        }

        // Token: 0x060029C1 RID: 10689 RVA: 0x0013C434 File Offset: 0x0013A834
        private void TryExpandHive()
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            if (this.snowNoise == null)
            {
                this.snowNoise = new Perlin(5.074999999701976776, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium); //new Perlin(0.054999999701976776, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
            }
            if (this.hiveRadius < 8f)
            {
                this.hiveRadius += 1.3f;
            }
            else if (this.hiveRadius < 17f)
            {
                this.hiveRadius += 0.7f;
            }
            else if (this.hiveRadius < 30f)
            {
                this.hiveRadius += 0.4f;
            }
            else
            {
                this.hiveRadius += 0.1f;
            }
            // Gradual growth capped at maxRadius. (The original code assigned
            // hiveRadius = maxRadius unconditionally, which nullified the growth
            // curve above and made every creep expansion a full-radius sweep.)
            this.hiveRadius = Mathf.Min(this.hiveRadius, this.maxRadius);
            CellRect occupiedRect = this.parent.OccupiedRect();
            CompHiveCreep.reachableCells.Clear();
        //    Log.Message(string.Format("1"));
            this.parent.Map.floodFiller.FloodFill(this.parent.Position, (IntVec3 x) => (float)x.DistanceToSquared(this.parent.Position) <= this.hiveRadius * this.hiveRadius && (occupiedRect.Contains(x) || !x.Filled(this.parent.Map) && x.InBounds(this.parent.Map) && x.Roofed(this.parent.Map)), delegate (IntVec3 x)
            {
                CompHiveCreep.reachableCells.Add(x);
            }, int.MaxValue, false, null);
        //    Log.Message(string.Format("2")); errors here V
            int num = GenRadial.NumCellsInRadius(this.hiveRadius);
        //    Log.Message(string.Format("3")); errors here ^
            for (int i = 0; i < num; i++)
            {
            //    Log.Message(string.Format("3 {0} a", i), true);
                if ((this.parent.Position + GenRadial.RadialPattern[i]).InBounds(this.parent.Map))
                {
                    IntVec3 intVec = this.parent.Position + GenRadial.RadialPattern[i];
                    if (intVec.InBounds(this.parent.Map))
                    {
                    //    if (i % 50 == 0) Log.Message(string.Format("3 {0} b", i), true);
                        if (CompHiveCreep.reachableCells.Contains(intVec))
                        {
                            List<Thing> list = intVec.GetThingList(parent.Map);
                            if (!list.NullOrEmpty())
                            {
                                // Destroy a share of the filth without the FindAll list
                                // allocation (this loop runs for hundreds of cells per
                                // expansion, per hive, every expandInterval ticks).
                                for (int k = 0; k < list.Count; k++)
                                {
                                    Thing item = list[k];
                                    if (item.def.IsFilth && Rand.ChanceSeeded(0.25f, AvPConstants.AvPSeed))
                                    {
                                        item.Destroy();
                                    }
                                }
                            }
                            //    Log.Message(string.Format("3 {0} c", i));
                            float num2 = 1f;
                            num2 += 1f;
                            num2 *= 0.5f;
                            if (num2 < 0.1f)
                            {
                                num2 = 0.1f;
                            }

                        if (this.parent.Map.GetComponent<MapComponent_HiveGrid>().GetDepth(intVec) <= num2)
                        {
                            //    Log.Message(string.Format("3 {0} d", i));
                            float lengthHorizontal = (intVec - this.parent.Position).LengthHorizontal;
                            float num3 = 1f - lengthHorizontal / this.hiveRadius;
                            this.parent.Map.GetComponent<MapComponent_HiveGrid>().AddDepth(intVec, num3 * this.Props.addAmount * num2);
                            //    Log.Message(string.Format("3 {0} e", i));
                        }
                        }
                    }
                }
            }
            sw.Stop();
            if (sw.ElapsedMilliseconds > 50)
            {
                Verse.Log.Message($"[AVP Xenomorphs] [Perf] hive creep expansion at {this.parent.Position} took {sw.ElapsedMilliseconds} ms (radius {this.hiveRadius:0.#}, hive-area lag investigation)");
            }
        }

        // Token: 0x04001726 RID: 5926
        private float hiveRadius;

        // Token: 0x04001727 RID: 5927
        private ModuleBase snowNoise;

        // Token: 0x04001728 RID: 5928
        private const float MaxOutdoorTemp = 10f;

        // Token: 0x04001729 RID: 5929
        private static HashSet<IntVec3> reachableCells = new HashSet<IntVec3>();
    }
}
