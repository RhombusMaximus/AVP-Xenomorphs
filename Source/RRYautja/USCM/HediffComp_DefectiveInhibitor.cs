using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RRYautja
{
    // Token: 0x02000010 RID: 16
    public class SynthTraitEntry
    {
        public TraitDef def
        {
            get
            {
                return DefDatabase<TraitDef>.GetNamedSilentFail(defName);
            }
        }
        // Token: 0x04000066 RID: 102
        public string defName;

        // Token: 0x04000067 RID: 103
        public int degree = 0;

        // Token: 0x04000068 RID: 104
        public float chance = 100f;

        // Token: 0x04000069 RID: 105
        public float commonalityMale = -1f;

        // Token: 0x0400006A RID: 106
        public float commonalityFemale = -1f;
    }

    public class HediffCompProperties_DefectiveInhibitor : HediffCompProperties
    {
        public HediffCompProperties_DefectiveInhibitor()
        {
            this.compClass = typeof(HediffComp_DefectiveInhibitor);
        }
        public List<SynthTraitEntry> traitsToGive = new List<SynthTraitEntry>();
        public int TraitGainMinIntervalTicks = 15000;
        public int TraitGainMaxIntervalTicks = 60000;
    }

    public class HediffComp_DefectiveInhibitor : HediffComp
    {
        public HediffCompProperties_DefectiveInhibitor Props
        {
            get
            {
                return (HediffCompProperties_DefectiveInhibitor)this.props;
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksSinceTraitGain, "ticksSinceTraitGain", 0, false);
            Scribe_Values.Look<int>(ref this.ticksTillTraitGain, "ticksTillTraitGain", 0, false);
            Scribe_Values.Look<int>(ref this.ticksTillTraitGain, "ticksTillTraitGain", 0, false);
            Scribe_Collections.Look(ref this.OriginalTraits, "OriginalTraits");
        }

        // Token: 0x04000002 RID: 2
        public int addedTraitCount;
        public int originalsRemaining;
        public int ticksSinceTraitGain;
        public int ticksTillTraitGain;
        public List<Trait> OriginalTraits;

        public int MaxTraits
        {
            get
            {
                int traitsmax = 4;
                if (!MoreTraitSlotsUtil.TryGetMaxTraitSlots(out traitsmax))
                {
                    traitsmax = 4;
                }
                return traitsmax;
            }
        }

        public int CurTraitCount
        {
            get
            {
                return Pawn.story.traits.allTraits.Count;
            }
        }

        public int OriTraitCount
        {
            get
            {
                return OriginalTraits.Count;
            }
        }

        public int OriRemaining
        {
            get
            {
                return OriginalTraits.FindAll(x => Pawn.story.traits.HasTrait(x.def)).Count;
            }
        }

        public List<Trait> OriTraitsRemaining
        {
            get
            {
                return OriginalTraits.FindAll(x => Pawn.story.traits.HasTrait(x.def));
            }
        }

        public int AddedTraitCount
        {
            get
            {
                return Pawn.story.traits.allTraits.FindAll(x => Props.traitsToGive.Any((y) => y.def == x.def)).Count;
            }
        }

        public List<Trait> AddedTraits
        {
            get
            {
                return Pawn.story.traits.allTraits.FindAll(x => Props.traitsToGive.Any((y) => y.def == x.def));
            }
        }

        public int AddedSpectrumTraitCount
        {
            get
            {
                return AddedTraits.FindAll(x => x.def.degreeDatas.Count > 1).Count;
            }
        }

        public List<Trait> AddedSpectrumTraits
        {
            get
            {
                return AddedTraits.FindAll(x => x.def.degreeDatas.Count > 1);
            }
        }

        public bool Active
        {
            get
            {
                bool flag1 = OriginalTraits.Any(x => Pawn.story.traits.HasTrait(x.def));
                bool flag2 = AddedSpectrumTraits.Any(x => Props.traitsToGive.Any(y => y.def == x.def && ((y.degree > 0 && y.def.degreeDatas.Any(z => z.degree == x.Degree + 1)) || (y.degree < 0 && y.def.degreeDatas.Any(z => z.degree == x.Degree - 1)))));
                return flag1 || flag2;
            }
        }

        public bool Selected
        {
            get
            {
                return Pawn.Spawned && Find.Selector.SelectedObjects.Contains(Pawn) && Prefs.DevMode && DebugSettings.godMode;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ticksTillTraitGain = Rand.Range(Props.TraitGainMinIntervalTicks, Props.TraitGainMaxIntervalTicks);
            OriginalTraits = Pawn.story.traits.allTraits;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (ticksSinceTraitGain >= ticksTillTraitGain)
            {
                //    if (Selected) Log.Message(string.Format("{0} Trait change on {1} Traits: {2}/{3}\n active: {4}, Originals: {5}/{6}, Added: {7}", this.parent.LabelCap, Pawn.LabelShortCap, CurTraitCount, MaxTraits, Active, OriRemaining, OriTraitCount, AddedTraitCount));
                if (MaxTraits >= CurTraitCount && Active)
                {
                    SynthTraitEntry traitEntry = GenCollection.RandomElementByWeight(Props.traitsToGive, (x => x.chance));
                    TraitDef traitDef = traitEntry.def;
                    //    if (Selected) Log.Message(string.Format("{0} rolled {1}", Pawn, traitDef));
                    Trait replacedtrait = null;
                    if (CurTraitCount == MaxTraits)
                    {
                        replacedtrait = OriRemaining > 0 ? OriTraitsRemaining.RandomElement() : AddedSpectrumTraits.FindAll(x => Props.traitsToGive.Any(y => y.def == x.def && ((y.degree > 0 && y.def.degreeDatas.Any(z => z.degree == x.Degree + 1)) || (y.degree < 0 && y.def.degreeDatas.Any(z => z.degree == x.Degree - 1))))).RandomElement();
                        if (replacedtrait == null && CurTraitCount == MaxTraits)
                        {
                            //    if (Selected) Log.Message(string.Format("{0} failed to find a trait to replace with {1}", Pawn, traitDef));
                            return;
                        }
                        //    if (Selected) Log.Message(string.Format("{0} replacing {1} with {2}", Pawn, replacedtrait, traitDef));
                    }
                    if (Pawn.story.traits.HasTrait(traitDef))// || (replacedtrait!=null && replacedtrait.def.degreeDatas.Count>1))
                    {
                        Trait trait = Pawn.story.traits.GetTrait(traitDef);
                        //   if (Selected) Log.Message(string.Format("{0} already has {1}",Pawn, trait.LabelCap));
                        if (traitDef.degreeDatas.Count > 1)
                        {
                            string spectxt = string.Format("{0} is a spectrum trait at Degree {1}", trait.LabelCap, trait.Degree);
                            int targetDegree = trait.Degree;
                            bool specinc = traitEntry.degree > 0 && traitEntry.def.degreeDatas.Any(x => x.degree == trait.Degree + 1);
                            bool specdec = traitEntry.degree < 0 && traitEntry.def.degreeDatas.Any(x => x.degree == trait.Degree - 1);
                            if (specinc)
                            {
                                targetDegree = +traitEntry.degree;
                                spectxt = spectxt + " " + string.Format("specinc:{0} target Degree {1}", specinc, targetDegree);
                            }
                            else if (specdec)
                            {
                                targetDegree = +traitEntry.degree;
                                spectxt = spectxt + " " + string.Format("specdec:{0} target Degree {1}", specdec, targetDegree);
                            }
                            else
                            {
                                targetDegree = +traitEntry.degree;
                                spectxt = spectxt + " " + string.Format("target Degree {2} is max", specinc, specdec, targetDegree);
                            }
                            //    if (Selected) Log.Message(spectxt);
                            if (trait.Degree == traitEntry.degree && traitEntry.degree != 0)
                            {
                                int degree = traitEntry.degree;
                                if (traitEntry.degree > 0 && traitEntry.def.degreeDatas.Any(x => x.degree == traitEntry.degree + 1))
                                {
                                    replacedtrait = trait;
                                    degree = traitEntry.degree + 1;
                                    trait = new Trait(traitDef, degree);
                                    Pawn.story.traits.allTraits.Remove(replacedtrait);
                                    GainTrait(Pawn, trait);
                                }
                                if (traitEntry.degree < 0 && traitEntry.def.degreeDatas.Any(x => x.degree == traitEntry.degree - 1))
                                {
                                    replacedtrait = trait;
                                    degree = traitEntry.degree - 1;
                                    trait = new Trait(traitDef, degree);
                                    Pawn.story.traits.allTraits.Remove(replacedtrait);
                                    GainTrait(Pawn, trait);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in traitDef.conflictingTraits)
                        {
                            if (Pawn.story.traits.HasTrait(item))
                            {
                                Pawn.story.traits.allTraits.Remove(Pawn.story.traits.GetTrait(item));
                            }
                        }
                        if (CurTraitCount == MaxTraits || (replacedtrait != null && Pawn.story.traits.HasTrait(replacedtrait.def)))
                        {
                            Pawn.story.traits.allTraits.Remove(Pawn.story.traits.GetTrait(replacedtrait.def));
                        }
                        if (CurTraitCount < MaxTraits)
                        {
                            Trait trait = new Trait(traitDef, traitEntry.degree);
                            GainTrait(Pawn, trait);
                        }
                    }
                }
                ticksSinceTraitGain = 0;
                ticksTillTraitGain = Rand.Range(Props.TraitGainMinIntervalTicks, Props.TraitGainMaxIntervalTicks);
            }
            ticksSinceTraitGain++;
        }


        public void GainTrait(Pawn pawn, Trait trait)
        {
            if (pawn.story.traits.HasTrait(trait.def))
            {
                Log.Warning(pawn + " already has trait " + trait.def);
                return;
            }
            pawn.story.traits.allTraits.Add(trait);
            if (pawn.workSettings != null)
            {
                pawn.workSettings.Notify_DisabledWorkTypesChanged();
            }
            //    pawn.story.Notify_TraitChanged();
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            foreach (var item in Props.traitsToGive) // Pawn.story.traits.allTraits)
            {
                if (Pawn.story.traits.HasTrait(item.def))
                {
                    Pawn.story.traits.allTraits.Remove(Pawn.story.traits.GetTrait(item.def));
                }
            }
            foreach (var item in OriginalTraits)
            {
                if (!Pawn.story.traits.HasTrait(item.def))
                {
                    Pawn.story.traits.GainTrait(item);
                }
            }
        }
    }
}
