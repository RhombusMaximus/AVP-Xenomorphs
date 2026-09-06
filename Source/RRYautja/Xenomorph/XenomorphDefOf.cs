using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x02000956 RID: 2390
	[DefOf]
    public static class XenomorphDefOf
    {
        // Token: 0x06003781 RID: 14209 RVA: 0x001A8393 File Offset: 0x001A6793
        static XenomorphDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(XenomorphDefOf));
        }
        // Xenomorph DamageDefs 
        public static DamageDef RRY_AcidDamage;
        public static DamageDef RRY_AcidBurn;

        // Xenomorph DamageDefs Special
        public static DamageDef RRY_EmergingChestbursterDD;

        // Xenomorph HefiffDefs 
        public static HediffDef RRY_Hediff_Xenomorph_Hidden;
        public static HediffDef RRY_Hediff_Cocooned;
        public static HediffDef HypothermicSlowdown;
        public static HediffDef RRY_Hediff_Anesthetic;
        public static HediffDef RRY_Hediff_Xenomorph_ProtoBodypart;
        public static HediffDef RRY_Hediff_Xenomorph_RestoredBodypart;

        // Xenomorph HefiffDefs HypothermicSlowdown
        public static HediffDef RRY_FaceHuggerInfection;
        public static HediffDef RRY_XenomorphImpregnation;
        public static HediffDef RRY_HiddenXenomorphImpregnation;

        // Neomorph HefiffDefs
        public static HediffDef RRY_NeomorphImpregnation;
        public static HediffDef RRY_HiddenNeomorphImpregnation;

        // Surgical RecipeDefs
        public static RecipeDef RRY_FaceHuggerRemoval;
        public static RecipeDef RRY_XenomorphImpregnationRemoval;
        public static RecipeDef RRY_NeomorphImpregnationRemoval;

        // Xenomorph PawnKindDefs
        public static PawnKindDef RRY_Xenomorph_FaceHugger;
        public static PawnKindDef RRY_Xenomorph_RoyaleHugger;
        public static PawnKindDef RRY_Xenomorph_Queen;
        public static PawnKindDef RRY_Xenomorph_Thrumbomorph;
        public static PawnKindDef RRY_Xenomorph_Warrior;
        public static PawnKindDef RRY_Xenomorph_Drone;
        public static PawnKindDef RRY_Xenomorph_Runner;
        public static PawnKindDef RRY_Xenomorph_Predalien;
        public static PawnKindDef RRY_Xenomorph_Praetorian;
        public static PawnKindDef RRY_Xenomorph_Spitter;
        /*
        public static PawnKindDef RRY_Xenomorph_Preatorian;
        public static PawnKindDef RRY_Xenomorph_Boiler;
        public static PawnKindDef RRY_Xenomorph_Crusher;
        public static PawnKindDef RRY_Xenomorph_King;
        */

        // Neomorph PawnKindDefs
        public static PawnKindDef RRY_Xenomorph_Neomorph;

        // Xenomorph FactionDefs
        public static FactionDef RRY_Xenomorph;

        // Xenomorph PawnsArrivalModeDefs 
        public static PawnsArrivalModeDef RRY_RandomDropThroughRoof;
        public static PawnsArrivalModeDef RRY_DropThroughRoofNearPower;
        public static PawnsArrivalModeDef RRY_RandomEnterFromTunnel;

        // Xenomorph RaidStrategyDefs 
        public static RaidStrategyDef RRY_PowerCut;
        public static RaidStrategyDef RRY_XenomorphImmediateAttack;

        // Xenomorph JobDefs 
        public static JobDef RRY_Job_Xenomorph_EnterHiveTunnel;
        public static JobDef RRY_Job_Xenomorph_LayEgg;
        public static JobDef RRY_Job_Xenomorph_Kidnap;
        public static JobDef RRY_Job_Xenomorph_PredalienImpregnate;
        public static JobDef RRY_Job_Xenomorph_MaintainHive;
        public static JobDef RRY_Job_Xenomorph_Construct_Hive_Slime;
        public static JobDef RRY_Job_Xenomorph_Construct_Hive_Node;
        public static JobDef RRY_Job_Xenomorph_Construct_Hive_Wall;
        public static JobDef RRY_Job_Xenomorph_Construct_Hive_Roof;
        public static JobDef RRY_Job_DestroyCocoon; 
        public static JobDef RRY_Neomorph_Ingest;
        public static JobDef RRY_Xenomorph_Ingest; 

        // Xenomorph DutyDefs    
        public static DutyDef RRY_Xenomorph_DefendHiveLoc;
        public static DutyDef RRY_Xenomorph_AssaultColony;
        public static DutyDef RRY_Xenomorph_AssaultColony_CutPower;
        public static DutyDef RRY_Xenomorph_DefendAndExpandHive;
        public static DutyDef RRY_Xenomorph_DefendHiveAggressively;
        public static DutyDef RRY_Xenomorph_Kidnap; 


        // Xenomorph ThingDefs  
        public static ThingDef RRY_Xenomorph_Cocoon_Humanoid;
        public static ThingDef RRY_Xenomorph_Cocoon_Animal;
        public static ThingDef RRY_EggXenomorphFertilized;
        public static ThingDef RRY_Xenomorph_TailSpike;
        public static ThingDef RRY_Xenomorph_HeadShell;
        public static ThingDef RRY_Leather_Xenomorph; 

        public static ThingDef RRY_Neomorph_Spores;
        public static ThingDef RRY_Neomorph_Spores_Hidden;
        
        public static ThingDef RRY_Plant_Neomorph_Fungus;
        public static ThingDef RRY_Plant_Neomorph_Fungus_Hidden;

        public static ThingDef RRY_XenomorphCryptosleepCasket;

        public static ThingDef RRY_XenomorphCrashedShipPart;
        public static ThingDef TunnelHiveLikeSpawner;
        public static ThingDef RRY_Xenomorph_Hive; 
        public static ThingDef RRY_Xenomorph_Hive_Child;
        public static ThingDef RRY_Xenomorph_Hive_Wall;
        public static ThingDef RRY_Xenomorph_Hive_Slime;

        public static ThingDef RRY_Filth_Slime;

        // Xenomorph BloodDefs 
        public static ThingDef RRY_FilthBloodXenomorph_Active;
        public static ThingDef RRY_FilthBloodXenomorph;

        // Xenomorph LifeStageDefs 
        public static LifeStageDef RRY_XFacehuggerFullyFormed;
        public static LifeStageDef RRY_XenomorphChestBursterBaby;
        public static LifeStageDef RRY_XenomorphChestBursterBig;
        public static LifeStageDef RRY_XenomorphFormedBaby;
        public static LifeStageDef RRY_XenomorphFormedMid;
        public static LifeStageDef RRY_XenomorphFullyFormed;

        // Neomorph LifeStageDefs 
        public static LifeStageDef RRY_NeomorphBloodBursterBaby;
        public static LifeStageDef RRY_NeomorphBloodBursterBig;
        public static LifeStageDef RRY_NeomorphFormedBaby;
        public static LifeStageDef RRY_NeomorphFormedMid;
        public static LifeStageDef RRY_NeomorphFullyFormed;

        // Xenomorph IncidentDefs RRY_XenomorphInfestation
        public static IncidentDef RRY_Neomorph_FungusSprout;
        public static IncidentDef RRY_Neomorph_FungusSprout_Hidden;
        public static IncidentDef RRY_XenomorphInfestation;
        public static IncidentDef RRY_XenomorphCrashedShipPartCrash;
    //    public static IncidentDef RRY_RaidEnemy_PowerCut_Xenomorph;
        public static IncidentDef RRY_PowerCut_Xenomorph;

        // Xenomorph BodyPartDefs
        public static BodyPartDef RRY_Xeno_TailSpike;
        public static BodyPartDef RRY_Xeno_Shell;

        // Xenomorph AbilityDefs
        public static AbilityDef RRY_Ability_SpitAcid;

        // Xenomorph GameCOnditionDefs
        public static GameConditionDef RRY_Xenomorph_PowerCut;

        // Xenomorph LifeStageDefs RRY_Xeno_Shell 
        //   public static LifeStageDef RRY_EggXenomorphFertilized;
/*
#if DEBUG
            if (Prefs.DevMode)
            {

            }
#endif
*/
    }

    [DefOf]
    public static class XenomorphRacesDefOf
    {
        // Token: 0x06003781 RID: 14209 RVA: 0x001A8393 File Offset: 0x001A6793
        static XenomorphRacesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(XenomorphRacesDefOf));
        }
        // Xenomorph ThingDefs Races
        public static ThingDef RRY_Xenomorph_FaceHugger;
        public static ThingDef RRY_Xenomorph_Queen;
        public static ThingDef RRY_Xenomorph_Warrior;
        public static ThingDef RRY_Xenomorph_Drone;
        public static ThingDef RRY_Xenomorph_Runner;
        public static ThingDef RRY_Xenomorph_Predalien;
        public static ThingDef RRY_Xenomorph_Praetorian;
        public static ThingDef RRY_Xenomorph_Thrumbomorph;

        // Neomorph ThingDefs Races
        public static ThingDef RRY_Xenomorph_Neomorph;

        // Xenomorph FleshTypeDefs
        public static FleshTypeDef RRY_Xenomorph;
        public static FleshTypeDef RRY_Neomorph;
    }
    [DefOf]
    public static class XenomorphConceptDefOf
    {
        // Token: 0x06003781 RID: 14209 RVA: 0x001A8393 File Offset: 0x001A6793
        static XenomorphConceptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(XenomorphRacesDefOf));
        }
        // Xenomorph ConceptDefs  
        public static ConceptDef RRY_Concept_Cocoons;
        public static ConceptDef RRY_Concept_Facehuggers;
        public static ConceptDef RRY_Concept_Royalehuggers;
        public static ConceptDef RRY_Concept_Embryo;
        public static ConceptDef RRY_Concept_Chestbursters;
        public static ConceptDef RRY_Concept_Runners;
        public static ConceptDef RRY_Concept_Drones;
        public static ConceptDef RRY_Concept_Warriors;
        public static ConceptDef RRY_Concept_Predaliens;
        public static ConceptDef RRY_Concept_Queens;
        public static ConceptDef RRY_Concept_Neomorphs;

        public static ConceptDef RRY_Concept_HiveLike;
        public static ConceptDef RRY_Concept_ShipPart;
        public static ConceptDef RRY_Concept_Eggs;
        public static ConceptDef RRY_Concept_Fungus;
    }
}
