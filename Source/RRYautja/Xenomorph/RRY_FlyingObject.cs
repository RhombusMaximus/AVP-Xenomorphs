using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    // Token: 0x02000013 RID: 19
    public class RRY_FlyingObject : ThingWithComps
    {
        // Token: 0x17000011 RID: 17
        // (get) Token: 0x0600005B RID: 91 RVA: 0x00003EE8 File Offset: 0x000020E8
        protected int StartingTicksToImpact
        {
            get
            {
                int num = Mathf.RoundToInt((this.origin - this.destination).magnitude / (this.speed / 100f));
                bool flag = num < 1;
                if (flag)
                {
                    num = 1;
                }
                return num;
            }
        }

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x0600005C RID: 92 RVA: 0x00003F31 File Offset: 0x00002131
        protected IntVec3 DestinationCell
        {
            get
            {
                return new IntVec3(this.destination);
            }
        }

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x0600005D RID: 93 RVA: 0x00003F40 File Offset: 0x00002140
        public virtual Vector3 ExactPosition
        {
            get
            {
                Vector3 b = (this.destination - this.origin) * (1f - (float)this.ticksToImpact / (float)this.StartingTicksToImpact);
                return this.origin + b + Vector3.up * this.def.Altitude;
            }
        }

        // Token: 0x17000014 RID: 20
        // (get) Token: 0x0600005E RID: 94 RVA: 0x00003FA4 File Offset: 0x000021A4
        public virtual Quaternion ExactRotation
        {
            get
            {
                return Quaternion.LookRotation(this.destination - this.origin);
            }
        }

        // Token: 0x17000015 RID: 21
        // (get) Token: 0x0600005F RID: 95 RVA: 0x00003FBC File Offset: 0x000021BC
        public override Vector3 DrawPos
        {
            get
            {
                return this.ExactPosition;
            }
        }

        // Token: 0x06000060 RID: 96 RVA: 0x00003FC4 File Offset: 0x000021C4
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector3>(ref this.origin, "origin", default(Vector3), false);
            Scribe_Values.Look<Vector3>(ref this.destination, "destination", default(Vector3), false);
            Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Values.Look<int>(ref this.timesToDamage, "timesToDamage", 0, false);
            Scribe_Values.Look<bool>(ref this.damageLaunched, "damageLaunched", true, false);
            Scribe_Values.Look<bool>(ref this.explosion, "explosion", false, false);
            Scribe_References.Look<Thing>(ref this.usedTarget, "usedTarget", false);
            Scribe_References.Look<Thing>(ref this.launcher, "launcher", false);
            Scribe_References.Look<Thing>(ref this.flyingThing, "flyingThing", false);
        }

        // Token: 0x06000061 RID: 97 RVA: 0x00004094 File Offset: 0x00002294
        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing, DamageInfo? impactDamage)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, impactDamage);
        }

        // Token: 0x06000062 RID: 98 RVA: 0x000040BC File Offset: 0x000022BC
        public void Launch(Thing launcher, LocalTargetInfo targ, Thing flyingThing)
        {
            this.Launch(launcher, base.Position.ToVector3Shifted(), targ, flyingThing, null);
        }

        public bool drafted;
        // Token: 0x06000063 RID: 99 RVA: 0x000040EC File Offset: 0x000022EC
        public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo targ, Thing flyingThing, DamageInfo? newDamageInfo = null)
        {
            bool spawned = flyingThing.Spawned;
            if (spawned)
            {
                flyingThing.DeSpawn(0);
            }

            drafted = ((Pawn)flyingThing).Drafted;
            this.launcher = launcher;
            this.origin = origin;
            this.impactDamage = newDamageInfo;
            this.flyingThing = flyingThing;
            bool flag = targ.Thing != null;
            if (flag)
            {
                this.usedTarget = targ.Thing;
            }
            this.destination = targ.Cell.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
            this.ticksToImpact = this.StartingTicksToImpact;
        }

        // Token: 0x06000064 RID: 100 RVA: 0x00004198 File Offset: 0x00002398
        protected override void Tick()
        {
            base.Tick();
            Vector3 exactPosition = this.ExactPosition;
            this.ticksToImpact--;
            bool flag = !GenGrid.InBounds(this.ExactPosition, base.Map);
            if (flag)
            {
                this.ticksToImpact++;
                base.Position = IntVec3Utility.ToIntVec3(this.ExactPosition);
                this.Destroy(0);
            }
            else
            {
                base.Position = IntVec3Utility.ToIntVec3(this.ExactPosition);
                bool flag2 = this.ticksToImpact <= 0;
                if (flag2)
                {
                    bool flag3 = GenGrid.InBounds(this.DestinationCell, base.Map);
                    if (flag3)
                    {
                        base.Position = this.DestinationCell;
                    }
                    this.ImpactSomething();
                }
            }
        }

        // Token: 0x06000065 RID: 101 RVA: 0x00004254 File Offset: 0x00002454
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            bool flag = this.flyingThing != null;
            if (flag)
            {
                bool flag2 = this.flyingThing is Pawn;
                if (flag2)
                {
                    Vector3 drawPos = this.DrawPos;
                    bool flag3 = false;
                    if (flag3)
                    {
                        return;
                    }
                    bool flag4 = !IntVec3Utility.ToIntVec3(this.DrawPos).IsValid;
                    if (flag4)
                    {
                        return;
                    }
                    Pawn pawn = this.flyingThing as Pawn;
                    pawn.Drawer.DrawAt(this.DrawPos);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.flyingThing.def.DrawMatSingle, 0);
                }
                base.Comps_PostDraw();
            }
        }

        // Token: 0x06000066 RID: 102 RVA: 0x00004308 File Offset: 0x00002508
        private void ImpactSomething()
        {
            bool flag = this.usedTarget != null;
            if (flag)
            {
                Pawn pawn = this.usedTarget as Pawn;
                bool flag2 = pawn != null /* && PawnUtility.GetPosture(pawn) != null */ && GenGeo.MagnitudeHorizontalSquared(this.origin - this.destination) >= 20.25f && Rand.Value > 0.2f;
                if (flag2)
                {
                    this.Impact(null);
                }
                else
                {
                    this.Impact(this.usedTarget);
                }
            }
            else
            {
                this.Impact(null);
            }
        }

        // Token: 0x06000067 RID: 103 RVA: 0x00004390 File Offset: 0x00002590
        protected virtual void Impact(Thing hitThing)
        {
            bool flag = hitThing == null;
            if (flag)
            {
                Pawn pawn;
                bool flag2 = (pawn = (GridsUtility.GetThingList(base.Position, base.Map).FirstOrDefault((Thing x) => x == this.usedTarget) as Pawn)) != null;
                if (flag2)
                {
                    hitThing = pawn;
                }
            }
            bool flag3 = this.impactDamage != null;
            if (flag3)
            {
                for (int i = 0; i < this.timesToDamage; i++)
                {
                    bool flag4 = this.damageLaunched;
                    if (flag4)
                    {
                        this.flyingThing.TakeDamage(this.impactDamage.Value);
                    }
                    else
                    {
                        hitThing.TakeDamage(this.impactDamage.Value);
                    }
                }
                bool flag5 = this.explosion;
                if (flag5)
                {
                    GenExplosion.DoExplosion(base.Position, base.Map, 0.9f, DamageDefOf.Stun, this, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);
                }
            }
            GenSpawn.Spawn(this.flyingThing, base.Position, base.Map, 0);
            Pawn p = (Pawn)this.flyingThing;
            if (drafted) p.drafter.Drafted = true;
            this.Destroy(0);
        }

        // Token: 0x04000035 RID: 53
        protected Thing usedTarget;

        // Token: 0x04000036 RID: 54
        public bool damageLaunched = true;

        // Token: 0x04000037 RID: 55
        protected Vector3 destination;

        // Token: 0x04000038 RID: 56
        public bool explosion;

        // Token: 0x04000039 RID: 57
        protected Thing flyingThing;

        // Token: 0x0400003A RID: 58
        public DamageInfo? impactDamage;

        // Token: 0x0400003B RID: 59
        protected Thing launcher;

        // Token: 0x0400003C RID: 60
        protected Vector3 origin;

        // Token: 0x0400003D RID: 61
        protected float speed = 30f;

        // Token: 0x0400003E RID: 62
        protected int ticksToImpact;

        // Token: 0x0400003F RID: 63
        public int timesToDamage = 3;
    }
}
