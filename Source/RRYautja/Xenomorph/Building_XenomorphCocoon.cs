using RRYautja;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	// Token: 0x020006C1 RID: 1729
	public class Building_XenomorphCocoon : Building_Bed
	{
		private bool PlayerCanSeeOwners
		{
			get
			{
				return false;
			}
		}

        public bool Occupied
        {
            get
            {
                return !this.AnyUnoccupiedSleepingSlot;//this.CurOccupants.Count() > 0;
            }
        }

        private static Graphic invisibleGraphic;

        public override Graphic Graphic
        {
            get
            {
                if (Occupied)
                {
                    Pawn occupant = GetCurOccupant(0);
                    if (occupant != null && occupant.RaceProps != null && occupant.RaceProps.Humanlike)
                    {
                        return base.Graphic;
                    }
                }
                // Use a cached transparent texture instead of Graphic_Invisible (which NREs in atlas)
                if (invisibleGraphic == null)
                {
                    invisibleGraphic = GraphicDatabase.Get<Graphic_Single>("DummyTexture", ShaderDatabase.Cutout);
                }
                return invisibleGraphic;
            }
        }
        public Pawn LastOccupant;
        public int ticksSinceHeal;
        public int healIntervalTicks = 60;
        public int ticksSinceOccupied;
        public int occupiedIntervalTicks = 100;

        // Token: 0x060024D6 RID: 9430 RVA: 0x001183A8 File Offset: 0x001167A8
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			Region validRegionAt_NoRebuild = map.regionGrid.GetValidRegionAt_NoRebuild(base.Position);
			this.Medical = true;
        }

        protected override void Tick()
        {
            base.Tick();
            if (Occupied)
            {
                if (CurOccupants.Count()>0)
                {
                    foreach (Pawn p in CurOccupants)
                    {
                        if (p.RaceProps.Humanlike)
                        {
                            this.def.building.bed_showSleeperBody = false;
                        }
                        if (!p.health.hediffSet.HasHediff(XenomorphDefOf.RRY_Hediff_Cocooned))
                        {
                            p.health.AddHediff(XenomorphDefOf.RRY_Hediff_Cocooned);
                        }
                        if (p.Dead)
                        {
                            this.Destroy();
                        }
                    }
                }
            }
            else
            {
                List<IntVec3> celllist = this.CellsAdjacent8WayAndInside().ToList();
                if (!celllist.NullOrEmpty())
                {
                    foreach (var cell in celllist)
                    {
                        if (cell.GetFirstPawn(this.Map) != null && cell.GetFirstPawn(this.Map) is Pawn p)
                        {
                            if (p.Downed && !p.Dead && !p.InBed() && !(p.kindDef.race.defName.Contains("RRY_Xenomorph_")))
                            {
                                p.jobs.Notify_TuckedIntoBed(this);
                                p.mindState.Notify_TuckedIntoBed();
                            }
                        }
                    }
                }
                else if (!Occupied)
                {
                    this.def.building.bed_showSleeperBody = true;
                }
                this.Destroy();
            }
        }
        
        public new Pawn GetCurOccupant(int slotIndex)
        {
            if (!base.Spawned)
            {
                return null;
            }
            IntVec3 sleepingSlotPos = this.GetSleepingSlotPos(slotIndex);
            List<Thing> list = base.Map.thingGrid.ThingsListAt(sleepingSlotPos);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Pawn pawn)
                {
                    if (pawn.CurJob != null)
                    {
                        if (pawn.GetPosture() == PawnPosture.LayingInBed)
                        {
                            return pawn;
                        }
                    }
                }
            }
            return null;
        }
        
        public new int GetCurOccupantSlotIndex(Pawn curOccupant)
        {
            for (int i = 0; i < this.SleepingSlotsCount; i++)
            {
                if (this.GetCurOccupant(i) == curOccupant)
                {
                    return i;
                }
            }
            return 0;
        }

        public new Pawn GetCurOccupantAt(IntVec3 pos)
        {
            for (int i = 0; i < this.SleepingSlotsCount; i++)
            {
                if (this.GetSleepingSlotPos(i) == pos)
                {
                    return this.GetCurOccupant(i);
                }
            }
            return null;
        }

        public new IntVec3 GetSleepingSlotPos(int index)
        {
            return XenomorphCocoonUtility.GetSleepingSlotPos(index, base.Position, base.Rotation, this.def.size);
        }
        
        public override void ExposeData()
		{
			base.ExposeData();
            /*
			Scribe_Values.Look<bool>(ref this.forPrisonersInt, "forPrisoners", false, false);
			Scribe_Values.Look<bool>(ref this.medicalInt, "medical", false, false);
			Scribe_Values.Look<bool>(ref this.alreadySetDefaultMed, "alreadySetDefaultMed", false, false);
            */
		}
        // Token: 0x060024DE RID: 9438 RVA: 0x001189DC File Offset: 0x00116DDC
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
            {
                yield return o;
            }
            if (base.AllComps != null)
            {
                for (int i = 0; i < this.AllComps.Count; i++)
                {
                    foreach (FloatMenuOption o2 in this.AllComps[i].CompFloatMenuOptions(myPawn))
                    {
                        yield return o2;
                    }
                }
            }
            yield break;
        }

        // Token: 0x060024D9 RID: 9433 RVA: 0x001184A0 File Offset: 0x001168A0
        public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
		}
        
		// Token: 0x060024DB RID: 9435 RVA: 0x001184F8 File Offset: 0x001168F8
		public override IEnumerable<Gizmo> GetGizmos()
        {
            /*
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (this.def.building.bed_humanlike && base.Faction == Faction.OfPlayer)
			{
				Command_Toggle pris = new Command_Toggle();
				pris.defaultLabel = "CommandBedSetForPrisonersLabel".Translate();
				pris.defaultDesc = "CommandBedSetForPrisonersDesc".Translate();
				pris.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners", true);
				pris.isActive = new Func<bool>(this.get_ForPrisoners);
				pris.toggleAction = delegate()
				{
					this.ToggleForPrisonersByInterface();
				};
				if (!Building_Bed.RoomCanBePrisonCell(this.GetRoom(RegionType.Set_Passable)) && !this.ForPrisoners)
				{
					pris.Disable("CommandBedSetForPrisonersFailOutdoors".Translate());
				}
				pris.hotKey = KeyBindingDefOf.Misc3;
				pris.turnOffSound = null;
				pris.turnOnSound = null;
				yield return pris;
				yield return new Command_Toggle
				{
					defaultLabel = "CommandBedSetAsMedicalLabel".Translate(),
					defaultDesc = "CommandBedSetAsMedicalDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/AsMedical", true),
					isActive = new Func<bool>(this.get_Medical),
					toggleAction = delegate()
					{
						this.Medical = !this.Medical;
					},
					hotKey = KeyBindingDefOf.Misc2
				};
				if (!this.ForPrisoners && !this.Medical)
				{
					yield return new Command_Action
					{
						defaultLabel = "CommandBedSetOwnerLabel".Translate(),
						icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner", true),
						defaultDesc = "CommandBedSetOwnerDesc".Translate(),
						action = delegate()
						{
							Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this));
						},
						hotKey = KeyBindingDefOf.Misc3
					};
				}
			}
            */
            yield break;
		}

        private IntVec3 nextValidPlacementSpot;
        public IntVec3 NextValidPlacementSpot
        {
            get
            {
                bool flag = this.nextValidPlacementSpot == default(IntVec3) || this.nextValidPlacementSpot == IntVec3.Invalid;
                if (flag)
                {
                    HashSet<Building_XenomorphCocoon> hashSet = new HashSet<Building_XenomorphCocoon>();
                    IEnumerable<IntVec3> enumerable = GenAdj.CellsAdjacent8Way(new TargetInfo(base.PositionHeld, base.MapHeld, false));
                    foreach (IntVec3 intVec in enumerable)
                    {
                        bool flag2 = GenGrid.Walkable(intVec, base.MapHeld);
                        if (flag2)
                        {
                            Building_XenomorphCocoon item;
                            bool flag3 = (item = (GridsUtility.GetThingList(intVec, base.Map).FirstOrDefault((Thing x) => x is Building_XenomorphCocoon) as Building_XenomorphCocoon)) != null;
                            if (flag3)
                            {
                                hashSet.Add(item);
                            }
                            else
                            {
                                bool flag4 = GridsUtility.GetThingList(intVec, base.Map).FirstOrDefault((Thing x) => x is Building_XenomorphCocoon) == null;
                                if (flag4)
                                {
                                    bool accepted = GenConstruct.CanPlaceBlueprintAt(this.def, intVec, Rot4.North, base.Map, false, null).Accepted;
                                    if (accepted)
                                    {
                                        this.nextValidPlacementSpot = intVec;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    foreach (Building_XenomorphCocoon building_Cocoon in hashSet)
                    {
                        bool flag5 = !building_Cocoon.resolvingCurrently;
                        if (flag5)
                        {
                            building_Cocoon.ResolvedNeighborPos();
                        }
                    }
                }
                return this.nextValidPlacementSpot;
            }
        }
        private bool resolvingCurrently = false;
        private Pawn spinner;
        public Pawn Spinner
        {
            get
            {
                return this.spinner;
            }
            set
            {
                this.spinner = value;
            }
        }

        public IntVec3 ResolvedNeighborPos()
        {
            this.resolvingCurrently = true;
            IntVec3 intVec = this.NextValidPlacementSpot;
            bool flag = !GenGrid.Walkable(intVec, base.MapHeld);
            if (flag)
            {
                this.nextValidPlacementSpot = default(IntVec3);
                intVec = this.NextValidPlacementSpot;
                for (int i = 0; i < 9; i++)
                {
                    bool flag2 = GridsUtility.GetThingList(intVec, base.Map).FirstOrDefault((Thing x) => x is Building_XenomorphCocoon) != null;
                    if (!flag2)
                    {
                        break;
                    }
                    this.nextValidPlacementSpot = default(IntVec3);
                    intVec = this.NextValidPlacementSpot;
                    bool flag3 = !GenConstruct.CanPlaceBlueprintAt(this.def, intVec, Rot4.North, base.Map, false, null).Accepted;
                    if (!flag3)
                    {
                        break;
                    }
                    this.nextValidPlacementSpot = default(IntVec3);
                    intVec = this.NextValidPlacementSpot;
                }
            }
            this.resolvingCurrently = false;
            return intVec;
        }
        
        public override string GetInspectString()
        {
            return "";
        }
        
        public override void DrawGUIOverlay()
        {
            return;
        }
        
		private void RemoveAllOwners()
		{
			for (int i = this.OwnersForReading.Count - 1; i >= 0; i--)
			{
				this.OwnersForReading[i].ownership.UnclaimBed();
			}
		}
    }
}
