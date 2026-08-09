using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000007 RID: 7
    public class Thing_AddsHediffDef : ThingDef
    {
        // Token: 0x0400001A RID: 26
        public HediffDef addHediff;

        // Token: 0x0400001B RID: 27
        public float hediffAddChance = 1f;

        // Token: 0x0400001C RID: 28
        public float hediffSeverity = 0.05f;

        // Token: 0x0400001D RID: 29
        public int tickUpdateSpeed = 250;

        // Token: 0x0400001E RID: 30
        public bool onlyAffectLungs = true;

        // Token: 0x0400001F RID: 31
        public bool isAcid = false;
    }

    // Token: 0x02000006 RID: 6
    public class Thing_AddsHediff : Gas
    {
        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000021 RID: 33 RVA: 0x00002D9F File Offset: 0x00000F9F
        // (set) Token: 0x06000022 RID: 34 RVA: 0x00002DA7 File Offset: 0x00000FA7
        public object cachedLabelMouseover { get; private set; }

        // Token: 0x06000023 RID: 35 RVA: 0x00002DB0 File Offset: 0x00000FB0
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.TickRate = this.Ticks;
            this.Ticks = this.TickRate;
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00002DD4 File Offset: 0x00000FD4
        protected override void Tick()
        {
            bool destroyed = base.Destroyed;
            if (!destroyed)
            {
                base.Tick();
                bool flag = this.destroyTick <= Find.TickManager.TicksGame && !base.Destroyed;
                if (flag)
                {
                    this.Destroy(0);
                }
                else
                {
                    this.graphicRotation += this.graphicRotationSpeed;
                    this.Ticks--;
                    bool flag2 = this.Ticks <= 0;
                    if (flag2)
                    {
                        this.TickTack();
                        this.Ticks = this.TickRate;
                    }
                }
            }
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00002E68 File Offset: 0x00001068
        public void TickTack()
        {
            bool destroyed = base.Destroyed;
            if (!destroyed)
            {
                Thing_AddsHediffDef thing_AddsHediffDef = this.def as Thing_AddsHediffDef;
                bool flag = thing_AddsHediffDef.addHediff != null;
                if (flag)
                {
                    List<Thing> thingList = GridsUtility.GetThingList(base.Position, base.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Pawn pawn = thingList[i] as Pawn;
                        bool flag2 = pawn != null && !this.touchingPawns.Contains(pawn);
                        if (flag2)
                        {
                            this.touchingPawns.Add(pawn);
                            this.addHediffToPawn(pawn, thing_AddsHediffDef.addHediff, thing_AddsHediffDef.hediffAddChance, thing_AddsHediffDef.hediffSeverity, thing_AddsHediffDef.onlyAffectLungs);
                        }
                    }
                    for (int j = 0; j < this.touchingPawns.Count; j++)
                    {
                        Pawn pawn2 = this.touchingPawns[j];
                        bool flag3 = !pawn2.Spawned || pawn2.Position != base.Position;
                        if (flag3)
                        {
                            this.touchingPawns.Remove(pawn2);
                        }
                    }
                }
                this.cachedLabelMouseover = null;
            }
        }

        // Token: 0x06000026 RID: 38 RVA: 0x00002F9C File Offset: 0x0000119C
        public void damageEntities(Thing e, int amt)
        {
            DamageInfo damageInfo;
            damageInfo = new DamageInfo(DamageDefOf.Burn, (float)amt, 0f, -1f, null, null, null, 0, null);
            bool flag = e != null;
            if (flag)
            {
                e.TakeDamage(damageInfo);
            }
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00002FDC File Offset: 0x000011DC
        public void damageBuildings(int amt)
        {
            IntVec3 intVec = GenAdj.RandomAdjacentCell8Way(this);
            bool flag = GenGrid.InBounds(intVec, base.Map);
            bool flag2 = flag;
            if (flag2)
            {
                Building firstBuilding = GridsUtility.GetFirstBuilding(intVec, base.Map);
                DamageInfo damageInfo;
                damageInfo = new DamageInfo(DamageDefOf.Burn, (float)amt, 0f, -1f, null, null, null, 0, null);
                bool flag3 = firstBuilding != null;
                bool flag4 = flag3;
                if (flag4)
                {
                    firstBuilding.TakeDamage(damageInfo);
                }
            }
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00003048 File Offset: 0x00001248
        public void addHediffToPawn(Pawn p, HediffDef _heddiff, float _addhediffChance, float _hediffseverity, bool onlylungs)
        {
            bool flag = !Rand.Chance(_addhediffChance);
            if (!flag)
            {
                Hediff hediff = HediffMaker.MakeHediff(_heddiff, p, null);
                hediff.Severity = _hediffseverity;
                bool flag2 = onlylungs && p.health.capacities.CapableOf(PawnCapacityDefOf.Breathing);
                if (flag2)
                {
                    List<BodyPartRecord> list = new List<BodyPartRecord>();
                    float num = 0.028758334f;
                    num *= StatExtension.GetStatValue(p, StatDefOf.ToxicResistance, true);
                    bool flag3 = num != 0f;
                    if (flag3)
                    {
                        float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 74374237));
                        num *= num2;
                    }
                    float statValue = StatExtension.GetStatValue(p, StatDefOf.ToxicResistance, true);
                    hediff.Severity = _hediffseverity * statValue;
                    foreach (BodyPartRecord bodyPartRecord in p.health.hediffSet.GetNotMissingParts(0, BodyPartDepth.Inside, null, null))
                    {
                        bool flag4 = bodyPartRecord.def.tags.Contains(BodyPartTagDefOf.BreathingSource);
                        if (flag4)
                        {
                            list.Add(bodyPartRecord);
                        }
                    }
                    bool flag5 = list.Count > 0;
                    if (flag5)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            Hediff hediff2;
                            if (p == null)
                            {
                                hediff2 = null;
                            }
                            else
                            {
                                Pawn_HealthTracker health = p.health;
                                if (health == null)
                                {
                                    hediff2 = null;
                                }
                                else
                                {
                                    HediffSet hediffSet = health.hediffSet;
                                    hediff2 = (hediffSet?.GetFirstHediffOfDef(_heddiff, false));
                                }
                            }
                            Hediff hediff3 = hediff2;
                            float num3 = Rand.Range(0.1f, 0.2f);
                            bool flag6 = hediff3 != null;
                            if (flag6)
                            {
                                hediff3.Severity += num3 * statValue;
                            }
                            else
                            {
                                p.health.AddHediff(hediff, list[i], null, null);
                            }
                        }
                    }
                }
                else
                {
                    Hediff hediff4;
                    if (p == null)
                    {
                        hediff4 = null;
                    }
                    else
                    {
                        Pawn_HealthTracker health2 = p.health;
                        if (health2 == null)
                        {
                            hediff4 = null;
                        }
                        else
                        {
                            HediffSet hediffSet2 = health2.hediffSet;
                            hediff4 = (hediffSet2?.GetFirstHediffOfDef(_heddiff, false));
                        }
                    }
                    Hediff hediff5 = hediff4;
                    float num4 = Rand.Range(0.1f, 0.2f);
                    float statValue2 = StatExtension.GetStatValue(p, StatDefOf.ToxicResistance, true);
                    bool flag7 = hediff5 != null;
                    if (flag7)
                    {
                        hediff5.Severity += num4 * statValue2;
                    }
                    else
                    {
                        hediff.Severity = _hediffseverity * statValue2;
                        p.health.AddHediff(hediff, null, null, null);
                    }
                }
            }
        }

        // Token: 0x04000015 RID: 21
        private List<Pawn> touchingPawns = new List<Pawn>();

        // Token: 0x04000016 RID: 22
        private List<Thing> touchingThings = new List<Thing>();

        // Token: 0x04000017 RID: 23
        private int Ticks = 250;

        // Token: 0x04000018 RID: 24
        private int TickRate = 250;
    }


}
