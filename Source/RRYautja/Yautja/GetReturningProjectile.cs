using System;
using RimWorld;
using Verse;

namespace RRYautja
{

    public class GetReturningProjectile : MoteThrown
    {

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.timesBounced, "timesBounced");
            Scribe_References.Look<Pawn>(ref this.OriginalPawn, "OriginalPawnRef");//, Props.pawn);
            Scribe_References.Look<Thing>(ref this.OriginalWeapon, "OriginalWeaponRef");//, Props.pawn);
            Scribe_References.Look<Projectile>(ref this.OriginalProjectile, "OriginalProjectileRef");//, Props.pawn);
        }

        public Pawn OriginalPawn;
        public Thing OriginalWeapon;
        public Projectile OriginalProjectile;
        public int timesBounced = 0;

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        protected override void Tick()
        {
            bool flag = base.Map == null;
            if (flag)
            {
                this.Destroy(0);
            }
        //    Log.Message(string.Format("Returning_Projectile Impact 1"));
            Projectile projectile2 = (Projectile)ThingMaker.MakeThing(YautjaDefOf.RRY_SmartDisk_Returning, null);
        //    Log.Message(string.Format("Returning_Projectile Impact 2"));
            GenSpawn.Spawn(projectile2, base.Position, base.Map, 0);
        //    Log.Message(string.Format("Returning_Projectile Impact 3 redirecting to {0}", OriginalPawn));
            projectile2.Launch(this, new LocalTargetInfo(OriginalPawn), OriginalPawn, ProjectileHitFlags.IntendedTarget, false, OriginalWeapon);
        //    Log.Message(string.Format("Returning_Projectile Impact 4"));
            this.Destroy(0);
        }
    }

}
