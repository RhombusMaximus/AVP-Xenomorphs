using HunterMarkingSystem.ExtensionMethods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace HunterMarkingSystem
{
    // Token: 0x02000791 RID: 1937
    public class CompUseEffect_MarkSelf : CompUseEffect
    {
        public override void DoEffect(Pawn user)
        {
            //    base.DoEffect(user);
            if (user.Markable(out Comp_Markable Markable))
            {
                BodyPartRecord part = Markable.partRecord;
                if (user.Marked(out Hediff marked, out Hediff blooded))
                {
                //    Log.Message(string.Format("Removing hediff {0}", marked));
                    user.health.RemoveHediff(marked);
                }
                if (blooded!=null)
                {
                    user.health.RemoveHediff(blooded);
                //    Log.Message(string.Format("Removing hediff {0}", marked));
                }
                corpse = (Corpse)this.parent;
                marked = HediffMaker.MakeHediff(Markable.markDataKillNew.MarkDef, user, part);// user.health.hediffSet.GetFirstHediffOfDef(markedDef);
                HediffComp_HunterMark marked_Yautja = marked.TryGetComp<HediffComp_HunterMark>();
                Markable.Mark = corpse.InnerPawn;
                user.health.AddHediff(marked, part);
                ThingDef thingDef = null;
                foreach (var item in corpse.InnerPawn.health.hediffSet.GetNotMissingParts())
                {
                    TrophyPartDefExtension trophyDef = null;
                    if (item.def.HasModExtension<TrophyPartDefExtension>())
                    {
                        trophyDef = item.def.GetModExtension<TrophyPartDefExtension>();
                    }
                    if (trophyDef != null)
                    {
                        float basechance = trophyDef.TrophyChance;
                        ThingDef trophy = trophyDef.TrophyDef ?? item.def.spawnThingOnRemoved;
                        if (trophyDef.PartHealthModifier)
                        {
                            basechance = basechance * corpse.InnerPawn.health.hediffSet.GetPartHealth(item);
                        }
                        if (trophy != null)
                        {
                            if (Rand.Chance(basechance))
                            {
                                partRecord = item;
                                thingDef = trophy;
                                corpse.InnerPawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, corpse.InnerPawn, this.partRecord));
                                GenSpawn.Spawn(ThingMaker.MakeThing(thingDef), user.Position, user.Map);

                            }
                        }
                    }
                }
                if (user.story.Adulthood == null || user.story.Adulthood.identifier == null || user.story.Adulthood.identifier == "RRY_Yautja_YoungBlood")
                {
                    if (marked.def == HMSDefOf.HMS_Hediff_BloodedMXenomorph)
                    {
                        AlienRace.BackstoryDef backstoryDef = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_Blooded");
                        user.story.Adulthood = backstoryDef.backstory;
                    }
                }
                Markable.markDataKill = Markable.markDataKillNew;
                if (this.parent.IsForbidden(user))
                {
                    this.parent.SetForbidden(false);
                }
                
                if (user.Markable(out Comp_Markable markable))
                {
                    markable.markcorpse = null;
                    markable.Mark = null;
                }
            }
        }

        BodyPartRecord partRecord;
        public Corpse corpse;
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (p.Markable(out Comp_Markable _Markable))
            {

                Hediff hediff = null;
                if (p.Marked(out hediff))
                {

                }
                if (hediff == null)
                {
                    return "Doesnt need marking";
                }
                ThingDef def = _Markable.markDataKillNew.raceDef;
                if (def == null)
                {
                    return "def pawn missing";
                }
                Corpse Corpse = _Markable.Markcorpse;
                if (Corpse == null)
                {
                    return "_Markable.Markcorpse is NULL";
                }
                if (this.parent is Corpse corpse)
                {
                    this.corpse = corpse;
                    /*
                //    Log.Message(string.Format("this.parent is Corpse corpse"));
                //    Log.Message(string.Format("corpse.InnerPawn.kindDef.race: {0}, def: {1}", corpse.InnerPawn.kindDef.race, def));
                    */
                    if (corpse.InnerPawn.kindDef.race == def || corpse == Corpse)
                    {
                        return true;
                    }
                    else
                    {
                        return "Wrong race";
                    }
                }
                else
                {
                    return "not a corpse";
                }
                /*
                if (YautjaBloodedUtility.bloodmatch(marked, (Corpse)this.parent))
                {
                    return true;
                }
                */
            }
            else
            {
                return "Unmarkable pawn";
            }
        }
    }

}
