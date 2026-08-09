using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RRYautja
{
    // Token: 0x02000E49 RID: 3657
    public abstract class Stuffable_Projectile : ThingWithComps
    {
        // Token: 0x17000D58 RID: 3416
        // (get) Token: 0x06005293 RID: 21139 RVA: 0x001295AB File Offset: 0x001279AB
        // (set) Token: 0x06005294 RID: 21140 RVA: 0x001295E1 File Offset: 0x001279E1
        public ProjectileHitFlags HitFlags
        {
            get
            {
                if (this.def.projectile.alwaysFreeIntercept)
                {
                    return ProjectileHitFlags.All;
                }
                if (this.def.projectile.flyOverhead)
                {
                    return ProjectileHitFlags.None;
                }
                return this.desiredHitFlags;
            }
            set
            {
                this.desiredHitFlags = value;
            }
        }

        // Token: 0x17000D59 RID: 3417
        // (get) Token: 0x06005295 RID: 21141 RVA: 0x001295EC File Offset: 0x001279EC
        protected int StartingTicksToImpact
        {
            get
            {
                int num = Mathf.RoundToInt((this.origin - this.destination).magnitude / (this.def.projectile.speed / 100f));
                if (num < 1)
                {
                    num = 1;
                }
                return num;
            }
        }

        // Token: 0x17000D5A RID: 3418
        // (get) Token: 0x06005296 RID: 21142 RVA: 0x00129639 File Offset: 0x00127A39
        protected IntVec3 DestinationCell
        {
            get
            {
                return new IntVec3(this.destination);
            }
        }

        // Token: 0x17000D5B RID: 3419
        // (get) Token: 0x06005297 RID: 21143 RVA: 0x00129648 File Offset: 0x00127A48
        public virtual Vector3 ExactPosition
        {
            get
            {
                Vector3 b = (this.destination - this.origin) * (1f - (float)this.ticksToImpact / (float)this.StartingTicksToImpact);
                return this.origin + b + Vector3.up * this.def.Altitude;
            }
        }

        // Token: 0x17000D5C RID: 3420
        // (get) Token: 0x06005298 RID: 21144 RVA: 0x001296A7 File Offset: 0x00127AA7
        public virtual Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation(this.destination - this.origin);
            }
        }

        // Token: 0x17000D5D RID: 3421
        // (get) Token: 0x06005299 RID: 21145 RVA: 0x001296BF File Offset: 0x00127ABF
        public override Vector3 DrawPos
        {
            get
            {
                return this.ExactPosition;
            }
        }

        // Token: 0x17000D5E RID: 3422
        // (get) Token: 0x0600529A RID: 21146 RVA: 0x001296C7 File Offset: 0x00127AC7
        public int DamageAmount
        {
            get
            {
                return this.def.projectile.GetDamageAmount(this.weaponDamageMultiplier, null);
            }
        }

        // Token: 0x17000D5F RID: 3423
        // (get) Token: 0x0600529B RID: 21147 RVA: 0x001296E0 File Offset: 0x00127AE0
        public float ArmorPenetration
        {
            get
            {
                return this.def.projectile.GetArmorPenetration(this.weaponDamageMultiplier, null);
            }
        }

        // Token: 0x0600529C RID: 21148 RVA: 0x001296FC File Offset: 0x00127AFC
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref this.origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.destination, "destination", default(Vector3), false);
            Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_TargetInfo.Look(ref this.usedTarget, "usedTarget");
            Scribe_TargetInfo.Look(ref this.intendedTarget, "intendedTarget");
            Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_Defs.Look<ThingDef>(ref this.equipmentDef, "equipmentDef");
            Scribe_Defs.Look<ThingDef>(ref this.targetCoverDef, "targetCoverDef");
            Scribe_Values.Look<ProjectileHitFlags>(ref this.desiredHitFlags, "desiredHitFlags", ProjectileHitFlags.All, false);
            Scribe_Values.Look<float>(ref this.weaponDamageMultiplier, "weaponDamageMultiplier", 1f, false);
            Scribe_Values.Look<bool>(ref this.landed, "landed", false, false);
        }

        // Token: 0x0600529D RID: 21149 RVA: 0x001297E0 File Offset: 0x00127BE0
        public void Launch(Thing launcher, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment = null)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), usedTarget, intendedTarget, hitFlags, equipment, null);
        }

        // Token: 0x0600529E RID: 21150 RVA: 0x0012980C File Offset: 0x00127C0C
        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            this.launcher = launcher;
            this.origin = origin;
            this.usedTarget = usedTarget;
            this.intendedTarget = intendedTarget;
            this.targetCoverDef = targetCoverDef;
            this.HitFlags = hitFlags;
            if (equipment != null)
            {
                this.equipmentDef = equipment.def;
                this.weaponDamageMultiplier = equipment.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier, true);

            //    Log.Message(string.Format("Original weaponDamageMultiplier: {0}", weaponDamageMultiplier));
                if (equipment.Stuff != null)
                {
                //    Log.Message(string.Format("equipment.Stuff: {0}", equipment.Stuff));
                //    weaponDamageMultiplier *= equipment.Stuff.GetStatValueAbstract(equipmentDef.Verbs[0].defaultProjectile.projectile.damageDef.armorCategory.multStat, null);
                //    Log.Message(string.Format("Stuffable weaponDamageMultiplier: {0}", weaponDamageMultiplier));
                }
            }
            else
            {
                this.equipmentDef = null;
                this.weaponDamageMultiplier = 1f;
            }
            this.destination = usedTarget.Cell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.3f);
            this.ticksToImpact = this.StartingTicksToImpact;
            if (!this.def.projectile.soundAmbient.NullOrUndefined())
            {
                SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
                this.ambientSustainer = this.def.projectile.soundAmbient.TrySpawnSustainer(info);
            }
        }

        // Token: 0x0600529F RID: 21151 RVA: 0x001298F8 File Offset: 0x00127CF8
        protected override void Tick()
        {
            base.Tick();
            if (this.landed)
            {
                return;
            }
            Vector3 exactPosition = this.ExactPosition;
            this.ticksToImpact--;
            if (!this.ExactPosition.InBounds(base.Map))
            {
                this.ticksToImpact++;
                base.Position = this.ExactPosition.ToIntVec3();
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            Vector3 exactPosition2 = this.ExactPosition;
            if (this.CheckForFreeInterceptBetween(exactPosition, exactPosition2))
            {
                return;
            }
            base.Position = this.ExactPosition.ToIntVec3();
            if (this.ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && this.def.projectile.soundImpactAnticipate != null)
            {
                this.def.projectile.soundImpactAnticipate.PlayOneShot(this);
            }
            if (this.ticksToImpact <= 0)
            {
                if (this.DestinationCell.InBounds(base.Map))
                {
                    base.Position = this.DestinationCell;
                }
                this.ImpactSomething();
                return;
            }
            if (this.ambientSustainer != null)
            {
                this.ambientSustainer.Maintain();
            }
        }

        // Token: 0x060052A0 RID: 21152 RVA: 0x00129A28 File Offset: 0x00127E28
        private bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
        {
            IntVec3 intVec = lastExactPos.ToIntVec3();
            IntVec3 intVec2 = newExactPos.ToIntVec3();
            if (intVec2 == intVec)
            {
                return false;
            }
            if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
            {
                return false;
            }
            if (intVec2.AdjacentToCardinal(intVec))
            {
                return this.CheckForFreeIntercept(intVec2);
            }
            if (VerbUtility.InterceptChanceFactorFromDistance(this.origin, intVec2) <= 0f)
            {
                return false;
            }
            Vector3 vector = lastExactPos;
            Vector3 v = newExactPos - lastExactPos;
            Vector3 b = v.normalized * 0.2f;
            int num = (int)(v.MagnitudeHorizontal() / 0.2f);
            Stuffable_Projectile.checkedCells.Clear();
            int num2 = 0;
            for (; ; )
            {
                vector += b;
                IntVec3 intVec3 = vector.ToIntVec3();
                if (!Stuffable_Projectile.checkedCells.Contains(intVec3))
                {
                    if (this.CheckForFreeIntercept(intVec3))
                    {
                        break;
                    }
                    Stuffable_Projectile.checkedCells.Add(intVec3);
                }
                num2++;
                if (num2 > num)
                {
                    return false;
                }
                if (intVec3 == intVec2)
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x060052A1 RID: 21153 RVA: 0x00129B3C File Offset: 0x00127F3C
        private bool CheckForFreeIntercept(IntVec3 c)
        {
            if (this.destination.ToIntVec3() == c)
            {
                return false;
            }
            float num = VerbUtility.InterceptChanceFactorFromDistance(this.origin, c);
            if (num <= 0f)
            {
                return false;
            }
            bool flag = false;
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (this.CanHit(thing))
                {
                    bool flag2 = false;
                    if (thing.def.Fillage == FillCategory.Full)
                    {
                        Building_Door building_Door = thing as Building_Door;
                        if (building_Door == null || !building_Door.Open)
                        {
                            this.ThrowDebugText("int-wall", c);
                            this.Impact(thing);
                            return true;
                        }
                        flag2 = true;
                    }
                    float num2 = 0f;
                    if (thing is Pawn pawn)
                    {
                        num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                        if (pawn.GetPosture() != PawnPosture.Standing)
                        {
                            num2 *= 0.1f;
                        }
                        if (this.launcher != null && pawn.Faction != null && this.launcher.Faction != null && !pawn.Faction.HostileTo(this.launcher.Faction))
                        {
                            num2 *= 0.4f;
                        }
                    }
                    else if (thing.def.fillPercent > 0.2f)
                    {
                        if (flag2)
                        {
                            num2 = 0.05f;
                        }
                        else if (this.DestinationCell.AdjacentTo8Way(c))
                        {
                            num2 = thing.def.fillPercent * 1f;
                        }
                        else
                        {
                            num2 = thing.def.fillPercent * 0.15f;
                        }
                    }
                    num2 *= num;
                    if (num2 > 1E-05f)
                    {
                        if (Rand.Chance(num2))
                        {
                            this.ThrowDebugText("int-" + num2.ToStringPercent(), c);
                            this.Impact(thing);
                            return true;
                        }
                        flag = true;
                        this.ThrowDebugText(num2.ToStringPercent(), c);
                    }
                }
            }
            if (!flag)
            {
                this.ThrowDebugText("o", c);
            }
            return false;
        }

        // Token: 0x060052A2 RID: 21154 RVA: 0x00129D72 File Offset: 0x00128172
        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), base.Map, text, -1f);
            }
        }

        // Token: 0x060052A3 RID: 21155 RVA: 0x00129D98 File Offset: 0x00128198
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Mesh mesh = MeshPool.GridPlane(this.def.graphicData.drawSize);
            Graphics.DrawMesh(mesh, this.DrawPos, this.ExactRotation, this.def.DrawMatSingle, 0);
            base.Comps_PostDraw();
        }

        // Token: 0x060052A4 RID: 21156 RVA: 0x00129DE0 File Offset: 0x001281E0
        protected bool CanHit(Thing thing)
        {
            if (!thing.Spawned)
            {
                return false;
            }
            if (thing == this.launcher)
            {
                return false;
            }
            bool flag = false;
            foreach (IntVec3 c in thing.OccupiedRect())
            {
                List<Thing> thingList = c.GetThingList(base.Map);
                bool flag2 = false;
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] != thing && thingList[i].def.Fillage == FillCategory.Full && thingList[i].def.Altitude >= thing.def.Altitude)
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return false;
            }
            ProjectileHitFlags hitFlags = this.HitFlags;
            if (thing == this.intendedTarget && (hitFlags & ProjectileHitFlags.IntendedTarget) != ProjectileHitFlags.None)
            {
                return true;
            }
            if (thing != this.intendedTarget)
            {
                if (thing is Pawn)
                {
                    if ((hitFlags & ProjectileHitFlags.NonTargetPawns) != ProjectileHitFlags.None)
                    {
                        return true;
                    }
                }
                else if ((hitFlags & ProjectileHitFlags.NonTargetWorld) != ProjectileHitFlags.None)
                {
                    return true;
                }
            }
            return thing == this.intendedTarget && thing.def.Fillage == FillCategory.Full;
        }

        // Token: 0x060052A5 RID: 21157 RVA: 0x00129F58 File Offset: 0x00128358
        private void ImpactSomething()
        {
            if (this.def.projectile.flyOverhead)
            {
                RoofDef roofDef = base.Map.roofGrid.RoofAt(base.Position);
                if (roofDef != null)
                {
                    if (roofDef.isThickRoof)
                    {
                        this.ThrowDebugText("hit-thick-roof", base.Position);
                        this.def.projectile.soundHitThickRoof.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                        this.Destroy(DestroyMode.Vanish);
                        return;
                    }
                    if (base.Position.GetEdifice(base.Map) == null || base.Position.GetEdifice(base.Map).def.Fillage != FillCategory.Full)
                    {
                        RoofCollapserImmediate.DropRoofInCells(base.Position, base.Map, null);
                    }
                }
            }
            if (!this.usedTarget.HasThing || !this.CanHit(this.usedTarget.Thing))
            {
                Stuffable_Projectile.cellThingsFiltered.Clear();
                List<Thing> thingList = base.Position.GetThingList(base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if ((thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Pawn || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Plant) && this.CanHit(thing))
                    {
                        Stuffable_Projectile.cellThingsFiltered.Add(thing);
                    }
                }
                Stuffable_Projectile.cellThingsFiltered.Shuffle<Thing>();
                for (int j = 0; j < Stuffable_Projectile.cellThingsFiltered.Count; j++)
                {
                    Thing thing2 = Stuffable_Projectile.cellThingsFiltered[j];
                    float num;
                    if (thing2 is Pawn pawn)
                    {
                        num = 0.5f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                        if (pawn.GetPosture() != PawnPosture.Standing && (this.origin - this.destination).MagnitudeHorizontalSquared() >= 20.25f)
                        {
                            num *= 0.2f;
                        }
                        if (this.launcher != null && pawn.Faction != null && this.launcher.Faction != null && !pawn.Faction.HostileTo(this.launcher.Faction))
                        {
                            num *= VerbUtility.InterceptChanceFactorFromDistance(this.origin, base.Position);
                        }
                    }
                    else
                    {
                        num = 1.5f * thing2.def.fillPercent;
                    }
                    if (Rand.Chance(num))
                    {
                        this.ThrowDebugText("hit-" + num.ToStringPercent(), base.Position);
                        this.Impact(Stuffable_Projectile.cellThingsFiltered.RandomElement<Thing>());
                        return;
                    }
                    this.ThrowDebugText("miss-" + num.ToStringPercent(), base.Position);
                }
                this.Impact(null);
                return;
            }
            if (this.usedTarget.Thing is Pawn pawn2 && pawn2.GetPosture() != PawnPosture.Standing && (this.origin - this.destination).MagnitudeHorizontalSquared() >= 20.25f && !Rand.Chance(0.2f))
            {
                this.ThrowDebugText("miss-laying", base.Position);
                this.Impact(null);
                return;
            }
            this.Impact(this.usedTarget.Thing);
        }

        // Token: 0x060052A6 RID: 21158 RVA: 0x0012A2DA File Offset: 0x001286DA
        protected virtual void Impact(Thing hitThing)
        {
            GenClamor.DoClamor(this, 2.1f, ClamorDefOf.Impact);
            this.Destroy(DestroyMode.Vanish);
        }

        // Token: 0x040036E2 RID: 14050
        protected Vector3 origin;

        // Token: 0x040036E3 RID: 14051
        protected Vector3 destination;

        // Token: 0x040036E4 RID: 14052
        protected LocalTargetInfo usedTarget;

        // Token: 0x040036E5 RID: 14053
        protected LocalTargetInfo intendedTarget;

        // Token: 0x040036E6 RID: 14054
        protected ThingDef equipmentDef;

        // Token: 0x040036E7 RID: 14055
        protected Thing launcher;

        // Token: 0x040036E8 RID: 14056
        protected ThingDef targetCoverDef;

        // Token: 0x040036E9 RID: 14057
        private ProjectileHitFlags desiredHitFlags = ProjectileHitFlags.All;

        // Token: 0x040036EA RID: 14058
        protected float weaponDamageMultiplier = 1f;

        // Token: 0x040036EB RID: 14059
        protected bool landed;

        // Token: 0x040036EC RID: 14060
        protected int ticksToImpact;

        // Token: 0x040036ED RID: 14061
        private Sustainer ambientSustainer;

        // Token: 0x040036EE RID: 14062
        private static List<IntVec3> checkedCells = new List<IntVec3>();

        // Token: 0x040036EF RID: 14063
        private static readonly List<Thing> cellThingsFiltered = new List<Thing>();
    }
}
