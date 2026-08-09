using AlienRace;
using RimWorld;
using RRYautja;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RRYautja
{

    public class CompProperties_Yautja : CompProperties
    {
        public CompProperties_Yautja()
        {
            this.compClass = typeof(Comp_Yautja);
        }
        
        public List<HediffDef> bloodedDefs;
    }

    public class Comp_Yautja : ThingComp
    {
        public CompProperties_Yautja Props
        {
            get
            {
                return (CompProperties_Yautja)this.props;
            }
        }

        AlienRace.BackstoryDef bsDefUnblooded = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_YoungBlood");
        AlienRace.BackstoryDef bsDefBlooded = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_Blooded");
        AlienRace.BackstoryDef bsDefBadbloodA = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_BadBloodA");
        AlienRace.BackstoryDef bsDefBadblooBd = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_BadBloodB");
        
        public HediffDef unbloodedDef = YautjaDefOf.HMS_Hediff_Unblooded;
        public HediffDef unmarkedDef = YautjaDefOf.HMS_Hediff_BloodedUM;
        public HediffDef GenericmarkedDef = YautjaDefOf.HMS_Hediff_BloodedM;

        public BodyPartRecord partRecord
        {
            get
            {
                foreach (var part in ((Pawn)parent).RaceProps.body.AllParts.Where(x => x.def.defName == "Head"))
                {
                    return part;
                }
                return null;
            }
        }

        public Pawn Pawn
        {
            get
            {
                return (Pawn)parent;
            }
        }

        public bool alienRace
        {
            get
            {
                return Pawn.def is ThingDef_AlienRace;
            }
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (Pawn.kindDef.race == YautjaDefOf.RRY_Alien_Yautja&&(Pawn.story.hairDef != YautjaDefOf.RRY_Yaujta_Dreds && Pawn.story.hairDef != YautjaDefOf.RRY_Yaujta_Ponytail && Pawn.story.hairDef != YautjaDefOf.RRY_Yaujta_Bald))
            {
                Pawn.story.hairDef = Rand.Chance(0.5f) ? YautjaDefOf.RRY_Yaujta_Dreds : YautjaDefOf.RRY_Yaujta_Ponytail;
            }
            if (Pawn.kindDef.race != YautjaDefOf.RRY_Alien_Yautja)
            {

            }
        }

        public override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);

        }

        public override void Notify_SignalReceived(Signal signal)
        {
            base.Notify_SignalReceived(signal);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
        }


    }
}
