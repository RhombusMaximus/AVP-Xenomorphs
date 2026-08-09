using RimWorld;
using RRYautja.ExtensionMethods;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static RRYautja.XenomorphHiveUtility;
using System.Linq;

namespace RRYautja
{
    public class PotentialXenomorphHiveLocation : IExposable
    {
        public PotentialXenomorphHiveLocation()
        {
            this.X = -1;
            this.Z = -1;
        }
        public PotentialXenomorphHiveLocation(IntVec3 value)
        {
            this.X = value.x;
            this.Z = value.z;
        }
        public IntVec3 HiveLoc
        {
            get
            {
                if (X == -1 || Z == -1)
                {
                    return IntVec3.Invalid;
                }
                return new IntVec3(X, 0, Z);
            }
            set
            {
                X = value.x;
                Z = value.z;
            }
        }
        public int X = -1;
        public int Z = -1;

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.X, "HiveLocX");
            Scribe_Values.Look(ref this.Z, "HiveLocZ");
        }
    }
    // Token: 0x02000067 RID: 103
    public class MapComponent_HiveGrid : MapComponent, IThingHolder
    {
        // Token: 0x06000217 RID: 535 RVA: 0x0000B430 File Offset: 0x00009630
        public MapComponent_HiveGrid(Map map) : base(map)
        {
            // this.map = map; // readonly field in 1.6
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            this.depthGrid = new float[map.cellIndices.NumGridCells];
            this.potentialHosts = new List<Pawn>();
            this.nonpotentialHosts = new List<Pawn>();
            this.Queenlist = new List<Pawn>();
            this.Dronelist = new List<Pawn>();
            this.Warriorlist = new List<Pawn>();
            this.Runnerlist = new List<Pawn>();
            this.Predalienlist = new List<Pawn>();
            this.Thrumbomorphlist = new List<Pawn>();
            this.HiveGuardlist = new List<Pawn>();
            this.HiveWorkerlist = new List<Pawn>();
            this.Hivelist = new List<Thing>();
            this.HiveLoclist = new List<IntVec3>();
            this.PotentialHiveLoclist = new List<PotentialXenomorphHiveLocation>();
            this.HiveChildlist = new List<Thing>();
            this.HiveChildLoclist = new List<IntVec3>();
        }

        public List<Pawn> XenoList
        {
            get
            {
                List<Pawn> List = new List<Pawn>();
                Queenlist.ForEach(x => List.Add(x));
                Dronelist.ForEach(x => List.Add(x));
                Warriorlist.ForEach(x => List.Add(x));
                Predalienlist.ForEach(x => List.Add(x));
                Thrumbomorphlist.ForEach(x => List.Add(x));
                return List;
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }
        
        public override void MapGenerated()
        {
            base.MapGenerated();
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            //    this.HiveGrid.Regenerate();
        }


        PawnKindDef RoyalKindDef = XenomorphDefOf.RRY_Xenomorph_RoyaleHugger;
        public bool RoyalPresent
        {
            get
            {
                Func<Pawn, bool> validator = delegate (Pawn t)
                {
                    bool RoyalHugger = t.kindDef == RoyalKindDef;
                    bool RoyalHuggerInfection = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_FaceHuggerInfection).TryGetComp<HediffComp_XenoFacehugger>().RoyaleHugger);
                    bool RoyalImpregnation = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_XenomorphImpregnation) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_XenomorphImpregnation).TryGetComp<HediffComp_XenoSpawner>().RoyaleHugger);
                    bool RoyalHiddenImpregnation = (t.health.hediffSet.HasHediff(XenomorphDefOf.RRY_HiddenXenomorphImpregnation) && t.health.hediffSet.GetFirstHediffOfDef(XenomorphDefOf.RRY_HiddenXenomorphImpregnation).TryGetComp<HediffComp_XenoSpawner>().RoyaleHugger);
                    return RoyalHugger || RoyalHuggerInfection || RoyalImpregnation || RoyalHiddenImpregnation;
                };
                return map.mapPawns.AllPawnsSpawned.Any((Func<Pawn, bool>)validator);
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            /*
            if (true)
            {
                bool hiveship = XenomorphUtil.HiveShipPresent(this.map);
                bool hivetunnel = XenomorphUtil.HivelikesPresent(this.map);
                bool hiveslime = XenomorphUtil.HiveSlimePresent(this.map);
                if (!hiveship && !hivetunnel && !hiveslime)
                {
                    for (int i = 0; i < depthGrid.Length; i++)
                    {
                        if (depthGrid[i] > 0f)
                        {
                            HiveUtility.AddHiveRadial(this.map.cellIndices.IndexToCell(i), map, 1, -Rand.RangeSeeded(0.0001f,0.001f, AvPConstants.AvPSeed));
                        }
                    }
                }
            }
            */
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (map.mapPawns.AllPawnsSpawned.Any(x=> x.isXenomorph()))
                {
                    foreach (Pawn p in map.mapPawns.AllPawnsSpawned)
                    {
                        if (p.isPotentialHost())
                        {
                            if (!potentialHosts.Contains(p))
                            {
                                potentialHosts.Add(p);
                            }
                        }
                        else
                        {
                            if (p.isXenomorph())
                            {
                                if (p.def == XenomorphRacesDefOf.RRY_Xenomorph_Runner)
                                {
                                    if (!Runnerlist.Contains(p))
                                    {
                                        Runnerlist.Add(p);
                                    }
                                }
                                if (p.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone)
                                {
                                    if (!Dronelist.Contains(p))
                                    {
                                        Dronelist.Add(p);
                                    }
                                }
                                if (p.def == XenomorphRacesDefOf.RRY_Xenomorph_Warrior)
                                {
                                    if (!Warriorlist.Contains(p))
                                    {
                                        Warriorlist.Add(p);
                                    }
                                }
                                if (p.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone)
                                {
                                    if (!Dronelist.Contains(p))
                                    {
                                        Dronelist.Add(p);
                                    }
                                }
                                if (p.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone)
                                {
                                    if (!Dronelist.Contains(p))
                                    {
                                        Dronelist.Add(p);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            /*
            if (Find.TickManager.TicksGame % 60 == 0)
            {
            //    Log.Message(string.Format("MapComponentTick update lists"));
                potentialHosts = map.ViableHosts();
                nonpotentialHosts = map.InviableHosts();
                Queenlist = map.mapPawns.AllPawnsSpawned.FindAll(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen);
                Dronelist = map.mapPawns.AllPawnsSpawned.FindAll(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone);
                Warriorlist = map.mapPawns.AllPawnsSpawned.FindAll(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Warrior);
                Runnerlist = map.mapPawns.AllPawnsSpawned.FindAll(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Runner);
                Predalienlist = map.mapPawns.AllPawnsSpawned.FindAll(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Predalien);
                Thrumbomorphlist = map.mapPawns.AllPawnsSpawned.FindAll(x => x.def == XenomorphRacesDefOf.RRY_Xenomorph_Thrumbomorph);
                Hivelist = map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_XenomorphHive);
                HiveChildlist = map.listerThings.ThingsOfDef(XenomorphDefOf.RRY_XenomorphHive_Child);
                foreach (HiveLike item in HiveChildlist)
                {
                    if (!item.GetDirectlyHeldThings().NullOrEmpty())
                    {
                        IList<Thing> l = item.GetDirectlyHeldThings();
                        if (Hivelist != null)
                        {
                            int drone = 0;
                            int warrior = 0;
                            int runner = 0;
                            HiveLike main = (HiveLike)Hivelist.RandomElement();
                            foreach (Pawn i in l)
                            {
                                if (i.def == XenomorphRacesDefOf.RRY_Xenomorph_Runner)
                                {
                                    runner++;
                                    if (runner > 2)
                                    {
                                        main.GetDirectlyHeldThings().TryAddOrTransfer(i);
                                    }
                                }
                                if (i.def == XenomorphRacesDefOf.RRY_Xenomorph_Drone)
                                {
                                    drone++;
                                    if (drone > 2)
                                    {
                                        main.GetDirectlyHeldThings().TryAddOrTransfer(i);
                                    }
                                }
                                if (i.def == XenomorphRacesDefOf.RRY_Xenomorph_Warrior)
                                {
                                    warrior++;
                                    if (warrior > 2)
                                    {
                                        main.GetDirectlyHeldThings().TryAddOrTransfer(i);
                                    }
                                }
                                if (i.def == XenomorphRacesDefOf.RRY_Xenomorph_Predalien)
                                {
                                    main.GetDirectlyHeldThings().TryAddOrTransfer(i);
                                }
                                if (i.def == XenomorphRacesDefOf.RRY_Xenomorph_Queen)
                                {
                                    main.GetDirectlyHeldThings().TryAddOrTransfer(i);
                                }
                                if (i.def == XenomorphRacesDefOf.RRY_Xenomorph_Thrumbomorph)
                                {
                                    main.GetDirectlyHeldThings().TryAddOrTransfer(i);
                                }
                                if (!l.Contains(i))
                                {
                                    if (main.GetDirectlyHeldThings().Contains(i))
                                    {
                                        if (main.Lord != null)
                                        {
                                            i.switchLord(main.Lord);
                                        }
                                        else
                                        {
                                            //    Log.Message(string.Format("{0} lord is null", main, i.LabelShortCap));
                                        }
                                    }
                                    else
                                    {
                                        //   Log.Message(string.Format("{0} doesnt contain {1}", main, i.LabelShortCap));
                                    }
                                }
                                else
                                {
                                    //    Log.Message(string.Format("{0} still contains {1}", item, i.LabelShortCap));
                                }
                            }
                        }
                    }
                }
            }
            */
        }

        internal float[] DepthGridDirect_Unsafe
        {
            get
            {
                return this.depthGrid;
            }
        }

        public float TotalDepth
        {
            get
            {
                return (float)this.totalDepth;
            }
        }


        private static ushort HiveFloatToShort(float depth)
        {
            depth = Mathf.Clamp(depth, 0f, 1f);
            depth *= 65535f;
            return (ushort)Mathf.RoundToInt(depth);
        }

        private static float HiveShortToFloat(ushort depth)
        {
            return (float)depth / 65535f;
        }

        private bool CanHaveHive(int ind)
        {
            Building building = this.map.edificeGrid[ind];

            if (building != null && !MapComponent_HiveGrid.CanCoexistWithHive(building.def))
            {
                return false;
            }
            TerrainDef terrainDef = this.map.terrainGrid.TerrainAt(ind);
            if (terrainDef.HasTag("Water"))
            {
                return false;
            }
            return terrainDef.passability != Traversability.Impassable && this.map.roofGrid.RoofAt(ind)!=null;// terrainDef == null || terrainDef.holdSnow;
        }

        public static bool CanCoexistWithHive(ThingDef def)
        {
            return def.category != ThingCategory.Building || def.Fillage != FillCategory.Full || def == XenomorphDefOf.RRY_Xenomorph_Hive || def == XenomorphDefOf.RRY_Xenomorph_Hive_Child || def == XenomorphDefOf.RRY_Xenomorph_Hive_Wall;
        }

        public void AddDepth(IntVec3 c, float depthToAdd)
        {
            int num = this.map.cellIndices.CellToIndex(c);
            float num2 = this.depthGrid[num];
            if (num2 <= 0f && depthToAdd < 0f)
            {
                return;
            }
            if (num2 >= 0.999f && depthToAdd > 1f)
            {
                return;
            }
            if (!this.CanHaveHive(num))
            {
                this.depthGrid[num] = 0f;
                return;
            }
            float num3 = num2 + depthToAdd;
            num3 = Mathf.Clamp(num3, 0f, 1f);
            float num4 = num3 - num2;
            this.totalDepth += (double)num4;
            if (Mathf.Abs(num4) > 0.0001f)
            {
                this.depthGrid[num] = num3;
                this.CheckVisualOrPathCostChange(c, num2, num3);
            }
        }

        public void SetDepth(IntVec3 c, float newDepth)
        {
            int num = this.map.cellIndices.CellToIndex(c);
            if (!this.CanHaveHive(num))
            {
                this.depthGrid[num] = 0f;
                return;
            }
            newDepth = Mathf.Clamp(newDepth, 0f, 1f);
            float num2 = this.depthGrid[num];
            this.depthGrid[num] = newDepth;
            float num3 = newDepth - num2;
            this.totalDepth += (double)num3;
            this.CheckVisualOrPathCostChange(c, num2, newDepth);
        }

        private void CheckVisualOrPathCostChange(IntVec3 c, float oldDepth, float newDepth)
        {
            if (!Mathf.Approximately(oldDepth, newDepth))
            {
                if (Mathf.Abs(oldDepth - newDepth) > 0.15f || Rand.Value < 0.0125f)
                {
                    this.map.mapDrawer.MapMeshDirty(c, RRYMapMeshFlagDefOf.Hive, true, false);
                    this.map.mapDrawer.MapMeshDirty(c, RRYMapMeshFlagDefOf.Hive, true, false);
                }
                else if (newDepth == 0f)
                {
                    this.map.mapDrawer.MapMeshDirty(c, RRYMapMeshFlagDefOf.Hive, true, false);
                }
                if (XenomorphHiveUtility.GetHiveCategory(oldDepth) != XenomorphHiveUtility.GetHiveCategory(newDepth))
                {
                    this.map.pathing.RecalculatePerceivedPathCostAt(c);
                }
            }
        }

        public float GetDepth(IntVec3 c)
        {
            if (!c.InBounds(this.map))
            {
                return 0f;
            }
            return this.depthGrid[this.map.cellIndices.CellToIndex(c)];
        }

        public HiveCategory GetCategory(IntVec3 c)
        {
            return XenomorphHiveUtility.GetHiveCategory(this.GetDepth(c));
        }

        public override void ExposeData()
        {
            MapExposeUtility.ExposeUshort(this.map, (IntVec3 c) => MapComponent_HiveGrid.HiveFloatToShort(this.GetDepth(c)), delegate (IntVec3 c, ushort val)
            {
                this.depthGrid[this.map.cellIndices.CellToIndex(c)] = MapComponent_HiveGrid.HiveShortToFloat(val);
            }, "depthGrid");


            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.HiveGuardlist.RemoveAll((Pawn x) => x.Destroyed);
                this.HiveWorkerlist.RemoveAll((Pawn x) => x.Destroyed);
                this.potentialHosts.RemoveAll((Pawn x) => x.Destroyed);
                this.nonpotentialHosts.RemoveAll((Pawn x) => x.Destroyed);
                this.Queenlist.RemoveAll((Pawn x) => x.Destroyed);
                this.Dronelist.RemoveAll((Pawn x) => x.Destroyed);
                this.Warriorlist.RemoveAll((Pawn x) => x.Destroyed);
                this.Runnerlist.RemoveAll((Pawn x) => x.Destroyed);
                this.Runnerlist.RemoveAll((Pawn x) => x.Destroyed);
                this.Thrumbomorphlist.RemoveAll((Pawn x) => x.Destroyed);
                this.Hivelist.RemoveAll((Thing x) => x.Destroyed);
                this.HiveChildlist.RemoveAll((Thing x) => x.Destroyed);
            }
            Scribe_Collections.Look<Pawn>(ref this.HiveGuardlist, "HiveGuardlist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.HiveWorkerlist, "HiveWorkerlist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.potentialHosts, "potentialHosts", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.nonpotentialHosts, "nonpotentialHosts", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.Queenlist, "Queenlist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.Dronelist, "Dronelist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.Warriorlist, "Warriorlist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.Runnerlist, "Runnerlist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.Predalienlist, "Predalienlist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.Thrumbomorphlist, "Thrumbomorphlist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<PotentialXenomorphHiveLocation>(ref this.PotentialHiveLoclist, "PotentialHiveLoclist", LookMode.Deep);
            Scribe_Collections.Look<Thing>(ref this.Hivelist, "Hivelist", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Thing>(ref this.HiveChildlist, "HiveChildlist", LookMode.Reference, new object[0]);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.HiveGuardlist.RemoveAll((Pawn x) => x == null);
                this.HiveWorkerlist.RemoveAll((Pawn x) => x == null);
                this.potentialHosts.RemoveAll((Pawn x) => x == null);
                this.nonpotentialHosts.RemoveAll((Pawn x) => x == null);
                this.Queenlist.RemoveAll((Pawn x) => x == null);
                this.Dronelist.RemoveAll((Pawn x) => x == null);
                this.Warriorlist.RemoveAll((Pawn x) => x == null);
                this.Runnerlist.RemoveAll((Pawn x) => x == null);
                this.Predalienlist.RemoveAll((Pawn x) => x == null);
                this.Thrumbomorphlist.RemoveAll((Pawn x) => x == null);
                this.Hivelist.RemoveAll((Thing x) => x == null);
                this.HiveChildlist.RemoveAll((Thing x) => x == null);
            }
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            base.ExposeData();
        }

        // Token: 0x060024F3 RID: 9459 RVA: 0x00116CE3 File Offset: 0x001150E3
        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        // Token: 0x060024F4 RID: 9460 RVA: 0x00116CEB File Offset: 0x001150EB
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public IThingHolder ParentHolder
        {
            get
            {
                return map;
            }
        }
        protected ThingOwner innerContainer;

        public MapComponent_HiveGrid HiveGrid;
        public List<Thing> Hivelist;
        public List<IntVec3> HiveLoclist
        {
            get
            {
                return hiveLoclist;
            }
            set
            {
                hiveLoclist = value;
            }
        }
        public List<IntVec3> hiveLoclist;
        public List<PotentialXenomorphHiveLocation> PotentialHiveLoclist;
        public List<Thing> HiveChildlist;
        public List<IntVec3> HiveChildLoclist;
        public List<Pawn> HiveGuardlist;
        public List<Pawn> HiveWorkerlist;
        public List<Pawn> potentialHosts;
        public List<Pawn> nonpotentialHosts;
        public List<Pawn> Queenlist;
        public List<Pawn> Dronelist;
        public List<Pawn> Warriorlist;
        public List<Pawn> Runnerlist;
        public List<Pawn> Predalienlist;
        public List<Pawn> Thrumbomorphlist;

        private float[] depthGrid;

        private double totalDepth;

        public const float MaxDepth = 1f;

    }
}
