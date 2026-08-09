using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RRYautja
{
    public class Hediff_Bouncer : HediffWithComps
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.TimesBounced, "TimesBounced");
            Scribe_Values.Look<bool>(ref this.canBounce, "canBounce");
            Scribe_References.Look<Pawn>(ref this.OriginalPawn, "OriginalPawnRef");//, Props.pawn);
            Scribe_References.Look<Thing>(ref this.OriginalWeapon, "OriginalWeaponRef");//, Props.pawn);
            Scribe_References.Look<Projectile>(ref this.OriginalProjectile, "OriginalProjectileRef");//, Props.pawn);

            /*
            Scribe_Defs.Look<HediffDef>(ref MarkedhediffDef, "MarkedhediffDef");
            Scribe_References.Look<Corpse>(ref this.corpse, "corpseRef");//, Props.corpse);//
            Scribe_References.Look<Pawn>(ref this.pawn, "pawnRef");//, Props.pawn);
            Scribe_Values.Look<String>(ref this.MarkHedifftype, "thisMarktype");//, Props.Marklabel);
            Scribe_Values.Look<String>(ref this.MarkHedifflabel, "thislabel");//, Props.Marklabel);
            Scribe_Values.Look<bool>(ref this.predator, "thisPred");
            Scribe_Values.Look<float>(ref this.combatPower, "thiscombatPower");
            Scribe_Values.Look<float>(ref this.BodySize, "thisBodySize");
            */
        }

        public int TimesBounced = 0;
        public int disaapearsIn = 5;

        public ThingDef BounceDef = (DefDatabase<ThingDef>.GetNamed("RRY_SmartDisk_Thrown"));
        public ThingDef ReturnDef
        {
            get
            {
                if (OriginalPawn.kindDef.race!=YautjaDefOf.RRY_Alien_Yautja)
                {
                    return (DefDatabase<ThingDef>.GetNamed("RRY_SmartDisk_Thrown"));
                }
                return (DefDatabase<ThingDef>.GetNamed("RRY_SmartDisk_Returning"));
            }
        }

        public Pawn OriginalPawn;
        public Thing OriginalWeapon;
        public Projectile OriginalProjectile;

        public bool canBounce = false;
        public int ExtraTargets
        {
            get
            {
                int count = 0;
                bool enablebounce = false;
                if (OriginalPawn.Faction == Faction.OfPlayer)
                {
                    if (YautjaDefOf.RRY_YautjaRanged_Basic.IsFinished)
                    {
                        count++;
                    }
                    if (YautjaDefOf.RRY_YautjaRanged_Med.IsFinished)
                    {
                        count++;
                    }
                    if (YautjaDefOf.RRY_YautjaRanged_Adv.IsFinished)
                    {
                        count++;
                    }
                }

                if (OriginalPawn.apparel.WornApparelCount > 0)
                {
                    foreach (var item in OriginalPawn.apparel.WornApparel)
                    {
                        if (item.def.defName.Contains("RRY_Apparel_") && item.def.defName.Contains("BioMask"))
                        {
                            enablebounce = true;
                            this.canBounce = enablebounce;
                        }
                    }
                }

                if (!enablebounce)
                {
                    count = 0;
                }
                return count;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.ageTicks> disaapearsIn)
            {
                this.pawn.health.RemoveHediff(this);
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            this.pawn.health.RemoveHediff(this);
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            float chance = 0.5f + ((float)ExtraTargets / 10) - ((float)TimesBounced / 10);
        //    Log.Message(string.Format("chance: {0} = 0.5f + ({1} / 10) - ({2} / 10)", chance, ExtraTargets, TimesBounced));
            bool shouldBounce = Rand.Chance(chance) && TimesBounced < ExtraTargets;
        //    Log.Message(string.Format("shouldBounce: {0} 1", shouldBounce));
            IntVec3 hitpos = !this.pawn.Dead ? this.pawn.Position : this.pawn.PositionHeld;
            Map hitmap = !this.pawn.Dead ? this.pawn.Map : this.pawn.MapHeld;
            Thing launcher = this.pawn;
            Projectile projectile = shouldBounce ? this.OriginalProjectile : (Projectile)ThingMaker.MakeThing(ReturnDef, null);
            string msg = shouldBounce ? "bouncing" : "returning";
        //    Log.Message(string.Format("shouldBounce: {0} 4", msg));
            Thing targetthing;
        //    Log.Message(string.Format("finding "));
            if (shouldBounce)
            {
            //    Log.Message(string.Format("hostile target"));
                targetthing = GenClosest.ClosestThingReachable(hitpos, hitmap, ThingRequest.ForGroup(ThingRequestGroup.Pawn), Verse.AI.PathEndMode.Touch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), 10f, x => (x.Faction.HostileTo(this.OriginalPawn.Faction)) && x != this.pawn, null, 0, -1, false, RegionType.Set_Passable, false);
                if (targetthing==null)
                {
                    if (!this.pawn.Dead)
                    {
                        targetthing = this.pawn;
                    }
                    else
                    {
                        targetthing = (Thing)this.OriginalPawn;
                    }
                }
            }
            else
            {
            //    Log.Message(string.Format("Original pawn"));
                targetthing = (Thing)this.OriginalPawn;
            //    Log.Message(string.Format("{0}", this.OriginalPawn.LabelShortCap));
            }
            if (shouldBounce) TimesBounced++;
        //    Log.Message(string.Format("TimesBounced: {0}", TimesBounced));
            if (shouldBounce)
            {

                Returning_Projectile Rprojectile = (Returning_Projectile)projectile;
            //    Log.Message(string.Format("converting and storing "));
                Rprojectile.timesBounced = this.TimesBounced;
                Rprojectile.OriginalPawn = this.OriginalPawn;
                Rprojectile.OriginalWeapon = this.OriginalWeapon;
                Rprojectile.OriginalProjectile = this.OriginalProjectile;
            //    Log.Message(string.Format("TimesBounced: {0}, OriginalPawn: {1}, OriginalWeapon: {2}, OriginalProjectile: {3}, ", this.TimesBounced, this.OriginalPawn, this.OriginalWeapon, this.OriginalProjectile));
                GenSpawn.Spawn(Rprojectile, hitpos, hitmap, 0);
            //    Log.Message(string.Format("GenSpawn"));
                Rprojectile.Launch(OriginalWeapon, new LocalTargetInfo(targetthing), targetthing, ProjectileHitFlags.IntendedTarget, false, OriginalPawn.equipment.Primary);
            //    Log.Message(string.Format("Launch: ") + msg);
            //    Log.Message(msg);
            }
            else
            {
                GenSpawn.Spawn(projectile, hitpos, hitmap, 0);
            //    Log.Message(string.Format("GenSpawn"));
                projectile.Launch(OriginalWeapon, new LocalTargetInfo(targetthing), targetthing, ProjectileHitFlags.IntendedTarget, false, OriginalPawn.equipment.Primary);
            //    Log.Message(string.Format("Launch: ") + msg);
            //    Log.Message(msg);
            }
        }








    }
}
