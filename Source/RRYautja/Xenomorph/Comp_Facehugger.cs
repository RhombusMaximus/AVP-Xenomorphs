using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RRYautja
{
    public class CompProperties_Facehugger : CompProperties
    {
        public CompProperties_Facehugger()
        {
            this.compClass = typeof(Comp_Facehugger);
        }
    }

    public class Comp_Facehugger : ThingComp
    {
        public CompProperties_Facehugger Props
        {
            get
            {
                return (CompProperties_Facehugger)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.Impregnations, "Impregnations", 0, true);
            Scribe_Values.Look<int>(ref this.ticksSinceHeal, "ticksSinceHeal");
            /*
            Scribe_Values.Look<int>(ref this.pawnKills, "pawnKills");
            Scribe_Deep.Look<Hediff>(ref this.unmarked, "bloodedUnmarked");
            Scribe_Defs.Look<HediffDef>(ref this.MarkedhediffDef, "MarkedhediffDef");
            Scribe_References.Look<Corpse>(ref this.corpse, "corpseRef", true);
            Scribe_References.Look<Pawn>(ref this.pawn, "pawnRef", true);
            Scribe_Values.Look<String>(ref this.MarkHedifftype, "thisMarktype");
            Scribe_Values.Look<String>(ref this.MarkHedifflabel, "thislabel");
            Scribe_Values.Look<bool>(ref this.predator, "thisPred");
            Scribe_Values.Look<float>(ref this.combatPower, "thiscombatPower");
            Scribe_Values.Look<float>(ref this.BodySize, "thisBodySize");
            Scribe_Values.Look<bool>(ref this.TurretIsOn, "thisTurretIsOn");
            Scribe_Values.Look<bool>(ref this.blooded, "thisblooded");
            */
        }

        public Pawn Facehugger
        {
            get
            {
                return ((Pawn)this.parent);
            }
        }

        public bool RoyaleHugger
        {
            get
            {
                return Facehugger.kindDef == RoyaleKindDef;
            }
        }

        public int maxImpregnations
        {
            get
            {
                if (RoyaleHugger)
                {
                    return 2;
                }
                return 1;
            }
        }

        public PawnKindDef pawnKindDef
        {
            get
            {
                return RoyaleHugger ? RoyaleKindDef : HuggerKindDef;
            }
        }
        public int Impregnations;

        public PawnKindDef HuggerKindDef { get { return XenomorphDefOf.RRY_Xenomorph_FaceHugger; } }
        public PawnKindDef RoyaleKindDef { get { return XenomorphDefOf.RRY_Xenomorph_RoyaleHugger; } }

        public int healIntervalTicks = 100;
        public int deathIntervalTicks = 300 * Rand.RangeInclusive(1,5);
        public override void CompTick()
        {
            if (Facehugger.Faction==null && Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph)!=null)
            {
                Facehugger.SetFaction(Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph));
            }
            base.CompTick();
            this.ticksSinceHeal++;
            if (Impregnations >= maxImpregnations) this.ticksSinceImpregnation++;
            bool flag = this.ticksSinceHeal > this.healIntervalTicks;
            if (flag)
            {
                bool flag2 = Facehugger.health.hediffSet.HasNaturallyHealingInjury();
                if (flag2)
                {
                    float num = 4f;
                    Hediff_Injury hediff_Injury = GenCollection.RandomElement<Hediff_Injury>(from x in Facehugger.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                                                                                             where HediffUtility.CanHealNaturally(x)
                                                                                             select x);
                    hediff_Injury.Heal(num * Facehugger.HealthScale * 0.01f);
                    string text = string.Format("{0} healed.", Facehugger.LabelCap);
                }
                if (Impregnations>=maxImpregnations)
                {
                    bool flag3 = this.ticksSinceImpregnation > this.deathIntervalTicks;
                    if (flag3)
                    {
                        this.ticksSinceImpregnation = 0;
                        if (Rand.Chance(0.5f))
                        {
                            Facehugger.Kill(null);
                        }
                    }
                }
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {

            Pawn other = dinfo.Instigator as Pawn;
            Pawn pawn = base.parent as Pawn;



            base.PostPostApplyDamage(dinfo, totalDamageDealt);

        }
        public PawnKindDef host;
        public int ticksSinceHeal;
        public int ticksSinceImpregnation;
    }
    
}
