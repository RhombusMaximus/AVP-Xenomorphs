using System;
using Verse;

namespace RimWorld
{
	[DefOf]
	public static class USCMDefOf
    {
		static USCMDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(USCMDefOf));
		}

        // USCM HefiffDefs
        public static HediffDef RRY_Damaged_Inhibitor;
        public static HediffDef RRY_Defective_Inhibitor;

        // USCM PawnKindDefs 

        // USCM ThingDefs  Races 
        public static ThingDef RRY_Synth;

        // USCM ThingDefs  Equipment
        public static ThingDef RRY_Equipment_HMS;

        // USCM ThingDefs  Weapons
        //    public static ThingDef RRY_Gun_Hunting_Bow;
        //    public static ThingDef RRY_Gun_Compound_Bow;

        // USCM ThingDefs  Projectiles
        //    // Yautja SmartDisk removed

        // USCM ThingDefs  Equipment RRY_USCM_ActiveDropshipUD4L
        public static ThingDef RRY_USCM_DropshipUD4L;
        public static ThingDef RRY_USCM_ActiveDropshipUD4L;
    //    public static ThingDef RRY_USCM_TravelingDropshipUD4L;
        public static ThingDef RRY_USCM_DropshipUD4LIncoming;
        public static ThingDef RRY_USCM_DropshipUD4LLeaving;

        // USCM ThingDefs  Motes
        //    // Yautja SmartDisk Mote removed

        // USCM PawnsArrivalModeDefs 
        //    public static PawnsArrivalModeDef EdgeWalkInGroups;

        // USCM ThoughtDefs
        //    // Yautja HonourableVsBadBlood removed

        // USCM ThoughtDefs Memories
        //    public static ThoughtDef RRY_Thought_ThrillOfTheHunt;

    }
    [DefOf]
    public static class USCMConceptDefOf
    {
        // Token: 0x06003781 RID: 14209 RVA: 0x001A8393 File Offset: 0x001A6793
        static USCMConceptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(USCMConceptDefOf));
        }
        // USCM ConceptDefs
        // RRY_Concept_Gauntlet removed (Yautja-only)
    }
}
