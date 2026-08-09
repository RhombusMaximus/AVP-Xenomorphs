using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RRYautja
{
    // Token: 0x02000014 RID: 20
    [StaticConstructorOnStartup]
    public class Cloakgen : Apparel
    {
        // Token: 0x06000055 RID: 85 RVA: 0x00003BC0 File Offset: 0x00001DC0
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
            Scribe_Values.Look<int>(ref this.uses, "uses", 0, false);
        }
        public int uses = 0;
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000007 RID: 7 RVA: 0x00002149 File Offset: 0x00000349
        public CompMedicalInjector kitComp
        {
            get
            {
                return base.GetComp<CompMedicalInjector>();
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002151 File Offset: 0x00000351
        public override void PostMake()
        {
            base.PostMake();
        //    this.uses = base.GetComp<CompMedicalInjector>().Props.Uses;
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002174 File Offset: 0x00000374
        public void UseKit()
        {
            bool flag = this.uses >= 1;
            if (flag)
            {
                this.uses--;
            }
            else
            {
                bool flag2 = !base.Destroyed;
                if (flag2)
                {
                    //    this.Destroy(0);
                }
            }
        }
        // Token: 0x06000010 RID: 16 RVA: 0x000022D2 File Offset: 0x000004D2
        public override IEnumerable<Gizmo> GetGizmos()
        {
            bool HealthshardReseached = (YautjaDefOf.RRY_YautjaHealthShard.IsFinished);
            if (HealthshardReseached)
            {
                yield return new Gizmo_InjectorStatus
                {
                    kit = this
                };
                foreach (Gizmo item in base.GetGizmos())
                {
                    yield return item;
                    //    item = null;
                }
            //    IEnumerator<Gizmo> enumerator = null;
            }
            base.GetGizmos();
            yield break;
        }
        // Token: 0x1700000A RID: 10
        // (get) Token: 0x06000050 RID: 80 RVA: 0x00003B12 File Offset: 0x00001D12
        private float EnergyMax
        {
            get
            {
                return this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
            }
        }

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x06000051 RID: 81 RVA: 0x00003B20 File Offset: 0x00001D20
        private float EnergyGainPerTick
        {
            get
            {
                return this.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60f;
            }
        }

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x06000052 RID: 82 RVA: 0x00003B34 File Offset: 0x00001D34
        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000053 RID: 83 RVA: 0x00003B3C File Offset: 0x00001D3C
        public CloakState cloakState
        {
            get
            {
                if ((this.ticksToReset > 0))// || (this.cloakMode == Cloakgen.CloakMode.ForcedOff))
                {
                    return Cloakgen.CloakState.Resetting;
                }
                else if ((this.cloakMode == Cloakgen.CloakMode.On))
                {
                    return Cloakgen.CloakState.Active;
                }
                return Cloakgen.CloakState.InActive;
            }
        }

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x06000054 RID: 84 RVA: 0x00003B4C File Offset: 0x00001D4C
        private bool ShouldDisplay
        {
            get
            {
                Pawn wearer = base.Wearer;
                return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000020EE File Offset: 0x000002EE
        public void RefreshCloakState()
        {
            if (this.ComputeCloakState())
            {
                this.SwitchOnCloak();
                return;
            }
            this.SwitchOffCloak();
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002108 File Offset: 0x00000308
        public bool ComputeCloakState()
        {
            return base.Wearer != null && !base.Wearer.Dead && !base.Wearer.Downed && base.Wearer.Awake() && (this.cloakMode == Cloakgen.CloakMode.On || (this.cloakMode != Cloakgen.CloakMode.Off));
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000021F0 File Offset: 0x000003F0
        public void SwitchOnCloak()
        {
            IntVec3 intVec = base.Wearer.DrawPos.ToIntVec3();
            if (!this.cloak.DestroyedOrNull() && intVec != this.cloak.Position)
            {
                this.SwitchOffCloak();
            }
            if (this.cloak.DestroyedOrNull())
            {
                /*
                Wearer.health.AddHediff(YautjaDefOf.RRY_Hediff_Cloaked);
                Hediff hediff = Wearer.health.hediffSet.GetFirstHediffOfDef(YautjaDefOf.RRY_Hediff_Cloaked);
                // hediff.TryGetComp<HediffComp_Blur>().blurTick = Find.TickManager.TicksGame;
                */
            }
            this.cloakIsOn = true;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x0000227D File Offset: 0x0000047D
        public void SwitchOffCloak()
        {
            if (Wearer != null)
            {
                if (Wearer.health.hediffSet.HasHediff(YautjaDefOf.RRY_Hediff_Cloaked))
                {
                    /*
                    Hediff hediff = Wearer.health.hediffSet.GetFirstHediffOfDef(YautjaDefOf.RRY_Hediff_Cloaked);
                    Wearer.health.RemoveHediff(hediff);
                    */
                    this.cloakMode = CloakMode.Off;
                }
            }
            this.cloakIsOn = false;
        }

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x0600000B RID: 11 RVA: 0x000021D8 File Offset: 0x000003D8
        public TargetingParameters TargetingParams
        {
            get
            {
                return new TargetingParameters
                {
                    canTargetBuildings = false,
                    canTargetItems = false,
                    canTargetSelf = false
                };
            }
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002204 File Offset: 0x00000404

        public void TendSelf()
        {
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
            Job job = new Job(YautjaDefOf.RRY_Yautja_TendSelf);
            base.Wearer.jobs.TryTakeOrderedJob(job, 0);
        }

        public void UseShard()
        {
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
            Job job = new Job(YautjaDefOf.RRY_Yautja_HealthShard);
            base.Wearer.jobs.TryTakeOrderedJob(job, 0);
        }

        // Token: 0x06002739 RID: 10041 RVA: 0x0012AA20 File Offset: 0x00128E20
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            base.GetWornGizmos();
            if (Find.Selector.SelectedObjects.Contains(base.Wearer) && base.Wearer.Faction == Faction.OfPlayer)
            {// this.uses
                bool MedicompReseached = (YautjaDefOf.RRY_YautjaMediComp.IsFinished);
                if (MedicompReseached)
                {
                    yield return new Command_Action
                    {
                        action = new Action(this.TendSelf),
                        defaultLabel = Translator.Translate("MedicompTendSelf"),
                        defaultDesc = Translator.Translate("MedicompTendSelfDesc"),
                        hotKey = KeyBindingDefOf.Misc3,
                        icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_TendSelfB", true)
                    };
                    bool HealthshardReseached = (YautjaDefOf.RRY_YautjaHealthShard.IsFinished);
                    if (HealthshardReseached)
                    {
                        if (Find.Selector.SingleSelectedObject == (base.Wearer))
                        {
                            yield return new Gizmo_InjectorStatus
                            {
                                kit = this
                            };
                            foreach (Gizmo item in base.GetWornGizmos())
                            {
                                yield return item;
                                //    item = null;
                            }
                        }
                        if (this.uses > 0)
                        {
                            yield return new Command_Action
                            {
                                action = new Action(this.UseShard),
                                defaultLabel = Translator.Translate("UseHealthShard"),
                                defaultDesc = Translator.Translate("UseHealthShardDesc"),
                                hotKey = KeyBindingDefOf.Misc2,
                                icon = ContentFinder<Texture2D>.Get("Things/Resources/Manufactured/HealthShard/HealthShard_a", true)
                            };
                        }
                    }
                }
                bool CloakReseached = (YautjaDefOf.RRY_YautjaCloakGenerator.IsFinished);
                if (CloakReseached)
                {
                    if (Find.Selector.SingleSelectedObject == (base.Wearer))
                    {
                        yield return new Gizmo_CloakgenStatus
                        {
                            cloak = this
                        };
                    }
                    int num = 700000102;
                    if (this.cloakMode == CloakMode.On)
                    {
                        yield return new Command_Action
                        {
                            icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_CloakMode", true),
                            defaultLabel = "Cloak: off.",
                            defaultDesc = "Switch mode.",
                            activateSound = SoundDef.Named("Click"),
                            action = new Action(this.SwitchCloakMode),
                            groupKey = num + 1,
                        };
                    }
                    if (this.cloakMode == CloakMode.Off)
                    {
                        yield return new Command_Action
                        {
                            icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_CloakMode", true),
                            defaultLabel = "Cloak: on.",
                            defaultDesc = "Switch mode.",
                            activateSound = SoundDef.Named("Click"),
                            action = new Action(this.SwitchCloakMode),
                            groupKey = num + 1,
                        };
                    }
                }
            }
            yield break;
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000023A4 File Offset: 0x000005A4
        public void SwitchCloakMode()
        {
            switch (this.cloakMode)
            {
                case Cloakgen.CloakMode.Off:
                    this.cloakMode = Cloakgen.CloakMode.On;
                    break;
                case Cloakgen.CloakMode.On:
                    this.cloakMode = Cloakgen.CloakMode.Off;
                    break;
            }
            this.RefreshCloakState();
        }

        // Token: 0x06000057 RID: 87 RVA: 0x00003C1D File Offset: 0x00001E1D
        public override float GetSpecialApparelScoreOffset()
        {
            return this.EnergyMax * this.ApparelScorePerEnergyMax;
        }

        // Token: 0x06000058 RID: 88 RVA: 0x00003C2C File Offset: 0x00001E2C
        protected override void Tick()
        {
            base.Tick();

            if (this.cloakIsOn || Find.TickManager.TicksGame >= this.nextUpdateTick)
            {
                this.nextUpdateTick = Find.TickManager.TicksGame + 60;
                this.RefreshCloakState();
            }
            if (base.Wearer == null)
            {
                this.energy = 0f;
                return;
            }
            else if (Wearer.Faction == Faction.OfPlayer)
            {
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.RRY_Concept_Gauntlet) && Wearer.IsColonist)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.RRY_Concept_Gauntlet, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.RRY_Concept_Wistblade) && Wearer.IsColonist)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.RRY_Concept_Wistblade, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.RRY_Concept_SelfDestruct) && (Wearer.IsColonist || Wearer.IsPrisoner))
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.RRY_Concept_SelfDestruct, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.RRY_Concept_MediComp) && Wearer.IsColonist && YautjaDefOf.RRY_YautjaMediComp.IsFinished)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.RRY_Concept_MediComp, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.RRY_Concept_ShardInjector) && Wearer.IsColonist && YautjaDefOf.RRY_YautjaHealthShard.IsFinished)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.RRY_Concept_ShardInjector, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.RRY_Concept_Cloak) && Wearer.IsColonist && YautjaDefOf.RRY_YautjaCloakGenerator.IsFinished)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.RRY_Concept_Cloak, OpportunityType.GoodToKnow);
                }
            }
            if (this.cloakState == Cloakgen.CloakState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                    return;
                }
            }
            else if (this.cloakState == Cloakgen.CloakState.Active & this.cloakMode == CloakMode.On)
            {
                this.energy -= this.EnergyGainPerTick / 10;
                if (this.energy <= 0)
                {
                    this.energy = 0;
                    this.cloakMode = CloakMode.Off;
                    this.SwitchOffCloak();
                }
            }
            else if (this.cloakMode == CloakMode.Off)
            {
                this.energy += this.EnergyGainPerTick;
                if (this.energy > this.EnergyMax)
                {
                    this.energy = this.EnergyMax;
                }
            }
        }

        // Token: 0x06000059 RID: 89 RVA: 0x00003CB0 File Offset: 0x00001EB0
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (dinfo.Instigator == base.Wearer)
            {
                return true;
            }
            if (this.cloakState == Cloakgen.CloakState.Active && (dinfo.Instigator != null || dinfo.Def.isExplosive))
            {
                if (dinfo.Instigator != null && dinfo.Instigator.Position.AdjacentTo8WayOrInside(base.Wearer.Position))
                {
                    this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
                }
                this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
                if (this.energy < 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }
                return true;
            }
            if (this.cloakState != Cloakgen.CloakState.Active || dinfo.Instigator != null)
            {
                return false;
            }
            this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
            if (this.energy < 0f)
            {
                this.Break();
                return false;
            }
            this.AbsorbedDamage(dinfo);
            return true;
        }

        // Token: 0x0600005A RID: 90 RVA: 0x00003DB9 File Offset: 0x00001FB9
        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        // Token: 0x0600005B RID: 91 RVA: 0x00003DCC File Offset: 0x00001FCC
        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = base.Wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + (float)dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        // Token: 0x0600005C RID: 92 RVA: 0x00003EBC File Offset: 0x000020BC
        private void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            MoteMaker.MakeStaticMote(base.Wearer.TrueCenter(), base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                MoteMaker.ThrowDustPuff(base.Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), base.Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
            this.energy = 0f;
            this.ticksToReset = this.StartingTicksToReset;
            cloakMode = CloakMode.Off;
        }

        // Token: 0x0600005D RID: 93 RVA: 0x00003F90 File Offset: 0x00002190
        private void Reset()
        {
            if (base.Wearer.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
                MoteMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;
        }

        // Token: 0x0600005E RID: 94 RVA: 0x00004008 File Offset: 0x00002208
        public override void DrawWornExtras()
        {
            if (this.cloakMode == CloakMode.On && this.ShouldDisplay && false)
            {
                // Wearer.Graphic.color.a = 0.25f;
                //Wearer.Drawer.renderer.graphics.pawn.DefaultGraphic.color.a = 0.25f;
                float num = Mathf.Lerp(1.2f, 1.55f, this.energy);
                Vector3 vector = base.Wearer.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = 0f;// (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            //    Graphics.DrawMesh(MeshPool.plane10, matrix, Cloakgen.BubbleMat, 0);
            }

        }

        // Token: 0x04000003 RID: 3
        public Thing cloak;

        public int nextUpdateTick;
        // Token: 0x04000019 RID: 25
        private float energy;

        // Token: 0x0400001A RID: 26
        private int ticksToReset = -1;

        // Token: 0x0400001B RID: 27
        private int lastKeepDisplayTick = -9999;

        // Token: 0x0400001C RID: 28
        private Vector3 impactAngleVect;

        // Token: 0x0400001D RID: 29
        private int lastAbsorbDamageTick = -9999;

        // Token: 0x0400001E RID: 30
        private const float MinDrawSize = 2.0f;

        // Token: 0x0400001F RID: 31
        private const float MaxDrawSize = 2.50f;

        // Token: 0x04000020 RID: 32
        private const float MaxDamagedJitterDist = 0.05f;

        // Token: 0x04000021 RID: 33
        private const int JitterDurationTicks = 8;

        // Token: 0x04000022 RID: 34
        private int StartingTicksToReset = 360;

        // Token: 0x04000023 RID: 35
        private float EnergyOnReset = 0.0f;

        // Token: 0x04000024 RID: 36
        private float EnergyLossPerDamage = 0.01f;

        // Token: 0x04000025 RID: 37
        private int KeepDisplayingTicks = 600;

        // Token: 0x04000026 RID: 38
        private float ApparelScorePerEnergyMax = 0.25f;

        // Token: 0x04000004 RID: 4
        public bool cloakIsOn;

        // Token: 0x04000005 RID: 5
        public Cloakgen.CloakMode cloakMode;

        // Token: 0x02000004 RID: 4
        public enum CloakMode
        {
            // Token: 0x04000008 RID: 8
            Off,
            // Token: 0x04000009 RID: 9
            On
        }

        // Token: 0x04000005 RID: 5
        // public Cloakgen.CloakState cloakState;
        public enum CloakState : byte
        {
            InActive,
            Active,
            Resetting
        }
        // Things/Gas/Puff
        // Other/CloakActive

       
    }
}
