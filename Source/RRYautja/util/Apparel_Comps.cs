using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using Verse;

namespace RRYautja
{

    public class CompProperties_WearableExplosive : CompProperties
    {

        /* Scales how likely this will explode when it takes damage.
         *   stability = MaxHitPoints cannot detonate until full destruction (0 hitpoints)
         *   stability = 2 cannot detonate until it has lost at least half its HP
         *   stability = 0.5 means there's at least 50/50 chance of exploding on the first hit
         *   stability = 0 could lead to an explosion without taking any damage in theory (but not in practice)
         */
        public float stability = 2f;

        // The following are properties defined for the Bomb DamageDef but not ProjectileProperties

        public float postExplosionSpawnChance;

        public int postExplosionSpawnThingCount = 1;

        public bool applyDamageToExplosionCellsNeighbors = true;

        public float preExplosionSpawnChance;

        public int preExplosionSpawnThingCount = 1;

        public float explosiveExpandPerStackcount;

        public EffecterDef explosionEffect;

        public CompProperties_WearableExplosive()
        {
            this.compClass = typeof(CompWearableExplosive);
        }
    }

    public class CompWearableExplosive : CompWearable
    {

        public CompProperties_WearableExplosive Props => (CompProperties_WearableExplosive)props;

        // Determine who is wearing this ThingComp. Returns a Pawn or null.
        protected virtual Pawn GetWearer
        {
            get
            {
                if (ParentHolder != null && ParentHolder is Pawn_ApparelTracker)
                {
                    return (Pawn)ParentHolder.ParentHolder;
                }
                else
                {
                    return null;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.Armed, "ThisArmed");
            Scribe_Values.Look<int>(ref this.ticksTillDetonation, "ThisticksTillDetonation");
        }

        public int ticksTillDetonation;
        public bool Armed;

        // Determine if this ThingComp is being worn presently. Returns True/False
        protected virtual bool IsWorn => (GetWearer != null);

        public int nextUpdateTick;
        // Token: 0x060053DD RID: 21469 RVA: 0x00264E3D File Offset: 0x0026323D
        public override void CompTick()
        {
            base.CompTick();
            if (IsWorn)
            {
                if ((GetWearer.Downed && GetWearer.Awake() && !GetWearer.InBed()) || (GetWearer.IsPrisonerOfColony) && !GetWearer.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection))
                {
                    if (Find.TickManager.TicksGame >= this.nextUpdateTick && !Armed)
                    {
                        this.nextUpdateTick = Find.TickManager.TicksGame + 1000;
                        float chanceBase = 0.01f;
                        chanceBase *= (this.parent.MaxHitPoints / this.parent.HitPoints); 
                        bool chance = Rand.Chance(chanceBase);
                    //    Log.Message(string.Format("chance: {0}, {1} = {2} / {3}", chance, chanceBase, this.parent.MaxHitPoints,this.parent.HitPoints));
                        if (chance && !Armed)
                        {
                            StartFuse();
                        }
                    }
                }
                if (Armed)
                {
                    string stringbit = null;
                    if (ticksTillDetonation > 0)
                    {
                        if (ticksTillDetonation == 1000)
                        {
                            stringbit = string.Format("Countdown Started {0} seconds remaining", (ticksTillDetonation / 100));
                            
                        }
                        if (ticksTillDetonation == 900)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 800)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 700)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 600)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 500)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 400)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 300)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 200)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (ticksTillDetonation == 100)
                        {
                            stringbit = string.Format("{0} seconds remaining", (ticksTillDetonation / 100));
                        }
                        if (!stringbit.NullOrEmpty())
                        {
                            IntVec3 vec3 = GetWearer.Position != null ? GetWearer.Position : GetWearer.PositionHeld;
                            Map map = GetWearer.Map ?? GetWearer.MapHeld;
                        //    vec3.x -= 1;
                            vec3.z += 1;
                            MoteMaker.ThrowText(vec3.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), map, stringbit, 5f);
                        }
                        ticksTillDetonation--;
                    }
                    else
                    {
                        Detonate();
                    }
                }
            }
        }

        // heavily based on Rimworld.CompExplosive.PostPreApplyDamage()
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            if (dinfo.Def.ExternalViolenceFor(parent))
            {
                if (dinfo.Amount >= parent.HitPoints)
                {
                    //Explode immediately from excessive incoming damage
                    Detonate();
                    absorbed = true;
                }
                else
                {
                    // As the hit points go down, explosion is more likely.
                    if (Rand.Value > (Props.stability * parent.HitPoints / parent.MaxHitPoints))
                    {
                        Detonate();
                        absorbed = true;
                    }
                }
            }
            // Currently ignores dinfo.Instigator
        }

        public void StartFuse()
        {
            Armed = true;
            ticksTillDetonation = 1000;
        //    IntVec3 vec3 = GetWearer.Position;
        //    vec3.x -= 1;
        //    vec3.z += 1;
        //    MoteMaker.ThrowText(vec3.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), GetWearer.Map, "Countdown Started", 5f);
        }

        public void StopFuse()
        {
            Armed = false;
            ticksTillDetonation = 1000;
            IntVec3 vec3 = GetWearer.Position;
        //    vec3.x -= 1;
            vec3.z += 1;
            MoteMaker.ThrowText(vec3.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), GetWearer.Map, "Countdown Stopped", 5f);
        }

        // heavily based on Rimwold.CompExplosive.Detonate()
        protected virtual void Detonate()
        {
            if (!parent.SpawnedOrAnyParentSpawned)
                return;

            // Use the wearing Pawn if there is one, otherwise use parent for determining location.
            ThingWithComps owner = IsWorn ? GetWearer : parent;
            Map map = owner.MapHeld;
            IntVec3 loc = owner.PositionHeld;

            if (GetWearer.Spawned)
                GetWearer.Kill(null);

            if (map == null)
            {
            //    Log.Warning("Tried to detonate CompWearableExplosive in a null map.");
                return;
            }
            if (loc == null)
            {
            //    Log.Warning("Tried to detonate CompWearableExplosive at a null position.");
                return;
            }

            var props = Props;
            var proj = parent.def.projectile;

            if (proj == null)
            {
            //    Log.Error("ThingDef with CompWearable is missing Projectile tag set. Cannot detonate.");
                return;
            }

            //Expand radius for stackcount
            float radius = proj.explosionRadius;
            if (!IsWorn && parent.stackCount > 1 && props.explosiveExpandPerStackcount > 0)
                radius += Mathf.Sqrt((parent.stackCount - 1) * props.explosiveExpandPerStackcount);

            if (props.explosionEffect != null)
            {
                var effect = props.explosionEffect.Spawn();
                effect.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
                effect.Cleanup();
            }
            for (int i = 0; i <= 3; i++)
            {
                    GenExplosion.DoExplosion
                    (
                        loc,
                        map,
                        radius,
                        proj.damageDef,
                        parent,
                        projectile: parent.def,
                        postExplosionSpawnThingDef: proj.postExplosionSpawnThingDef,
                        postExplosionSpawnChance: props.postExplosionSpawnChance,
                        postExplosionSpawnThingCount: props.postExplosionSpawnThingCount,
                        applyDamageToExplosionCellsNeighbors: props.applyDamageToExplosionCellsNeighbors,
                        preExplosionSpawnThingDef: proj.preExplosionSpawnThingDef,
                        preExplosionSpawnChance: props.preExplosionSpawnChance,
                        preExplosionSpawnThingCount: props.preExplosionSpawnThingCount
                    );
                
            }

        }

        public override IEnumerable<Gizmo> CompGetGizmosWorn()
        {
            int num = 700000108;
            if (GetWearer.Faction == Faction.OfPlayerSilentFail)
            {
                yield return new Command_Action
                {
                    action = Detonate,
                    defaultLabel = "WearableExplosives_Detonate_Label".Translate(),
                    defaultDesc = "WearableExplosives_Detonate_Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_BOOM", true),
                    activateSound = SoundDef.Named("Click"),
                    groupKey = num + 1
                };
                if (Armed)
                {
                    yield return new Command_Action
                    {
                        action = StopFuse,
                        defaultLabel = "WearableExplosives_Timer_Stop_Label".Translate(),
                        defaultDesc = "WearableExplosives_Timer_Desc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_BOOM", true),
                        activateSound = SoundDef.Named("Click"),
                        groupKey = num + 2
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        action = StartFuse,
                        defaultLabel = "WearableExplosives_Timer_Start_Label".Translate(),
                        defaultDesc = "WearableExplosives_Timer_Desc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_BOOM", true),
                        activateSound = SoundDef.Named("Click"),
                        groupKey = num + 2
                    };
                }
            }
            yield break;
        }
    }
}