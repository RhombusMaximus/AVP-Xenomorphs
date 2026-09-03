using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RRYautja.ExtensionMethods
{
    [StaticConstructorOnStartup]
    public static class XenomorphExtensions
    {
        public static Lord SwitchToLord(this Pawn p, Lord lord)
        {
            if (p.GetLord() != null && p.GetLord() is Lord l)
            {
                // Properly remove from old lord
                if (l.ownedPawns.Contains(p))
                {
                    l.ownedPawns.Remove(p);
                }
                // Clear the pawn's lord reference
                p.mindState.duty = null;
                if (l.ownedPawns.Count == 0)
                {
                    l.lordManager.RemoveLord(l);
                }
            }
            // Only add if not already in this lord
            if (!lord.ownedPawns.Contains(p))
            {
                lord.AddPawn(p);
            }
            return lord;
        }

        public static Lord CreateNewLord(this Pawn pawn)
        {
            IntVec3 c;
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(XenomorphDefOf.RRY_Xenomorph_Hive_Slime), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 9999f, null, null, 0, -1, false, RegionType.Set_Passable, false);
            if (thing != null)
            {
                c = RCellFinder.RandomWanderDestFor(pawn, thing.Position, 5f, null, Danger.Some);
            }
            else
            {
                if (InfestationLikeCellFinder.TryFindCell(out c, out IntVec3 lc, pawn.Map, false))
                {
                    if (Prefs.DevMode && Find.Selector.SelectedObjects.Contains(pawn))
                    {
                        ThingDef td = XenomorphDefOf.RRY_Xenomorph_Hive_Slime;
                        GenSpawn.Spawn(td, c, pawn.Map);
                        Find.LetterStack.ReceiveLetter(string.Format("Lord Created"), string.Format("@: {0} ", c), LetterDefOf.NegativeEvent, c.GetFirstThing(pawn.Map, td), null, null);
                    }
                }
                if (pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, true))
                {
                    //    c = RCellFinder.RandomWanderDestFor(pawn, c, 3f, null, Danger.Some);
                }
                else
                {
                    c = RCellFinder.RandomWanderDestFor(pawn, thing.Position, 3f, null, Danger.Some);
                }
            }
            if (pawn.GetLord() != null && pawn.GetLord() is Lord l)
            {
                if (l.ownedPawns.Count > 0)
                {
                    l.ownedPawns.Remove(pawn);
                }
                if (l.ownedPawns.Count == 0)
                {
                    l.lordManager.RemoveLord(l);
                }
            }
            Lord lord = LordMaker.MakeNewLord(pawn.Faction, new LordJob_DefendAndExpandHiveLike(false), pawn.Map, null);
            lord.AddPawn(pawn);
            return lord;
        }

        public static Lord CreateNewLord(this Pawn pawn, IntVec3 loc, LordJob lordJob)
        {
            IntVec3 c = loc;
            if (pawn.GetLord() != null && pawn.GetLord() is Lord l)
            {
                if (l.ownedPawns.Count > 0)
                {
                    l.ownedPawns.Remove(pawn);
                }
                if (l.ownedPawns.Count == 0)
                {
                    l.lordManager.RemoveLord(l);
                }
            }
            Lord lord = LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, null);
            lord.AddPawn(pawn);
            return lord;
        }
    }
}
