using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using RRYautja.settings;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    [StaticConstructorOnStartup]
    class Main
    {
        public static readonly Type patchType = typeof(Main);
        static Main()
        {
            Type pawnRenderer = typeof(PawnRenderer);
            Type pawn_HealthTracker = typeof(Pawn_HealthTracker);
            Type pawn_NativeVerbs = typeof(Pawn_NativeVerbs);

            // TODO(1.5+): The "pawn" private field in PawnRenderer may have been renamed in RimWorld 1.5+.
            // If this returns null, PawnRenderer_GetPawn will return null and callers should handle it gracefully.
            Main.pawnField_PawnRenderer = pawnRenderer.GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
            if (Main.pawnField_PawnRenderer == null)
            {
                Log.Error("RRYautja.Main: Failed to reflect PawnRenderer.pawn field. This field may have been renamed in RimWorld 1.5+. Cloak/stealth rendering may not work correctly.");
            }
            Main.int_Pawn_HealthTracker_GetPawn = pawn_HealthTracker.GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            Main._cachedVerbProperties = pawn_NativeVerbs.GetField("cachedVerbProperties", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
            Main._pawnPawnNativeVerbs = pawn_NativeVerbs.GetField("pawn", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
            //    HarmonyInstance.DEBUG = true;
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x=> x.apparel!=null))
            {
                if (def.apparel.wornGraphicPath.NullOrEmpty())
                {
                    if (!AlienRace.RaceRestrictionSettings.apparelWhiteDict.ContainsKey(key: def))
                        AlienRace.RaceRestrictionSettings.apparelWhiteDict.Add(key: def, value: new List<AlienRace.ThingDef_AlienRace>());
                    AlienRace.RaceRestrictionSettings.apparelWhiteDict[key: def].Add(item: ((AlienRace.ThingDef_AlienRace)YautjaDefOf.RRY_Alien_Yautja));
                }
            }

            ThingDef thing = DefDatabase<ThingDef>.GetNamedSilentFail("O21_AntiInfestationThumper");
            if (thing != null)
            {
                ThumperPatch();
            }
        }
        public static void ThumperPatch()
        {
            AvPMod.harmony.Patch(AccessTools.Method(typeof(Thumper.HarmonyPatches), "AlienVsPredator_Compatibility", null, null), new HarmonyMethod(Main.patchType, "AlienVsPredator_Compatibility_Prefix", null));
        }
        public static bool AlienVsPredator_Compatibility_Prefix()
        {
        //    Log.Message("FYT");
            return false;
        }

        public static Pawn pawnPawnNativeVerbs(Pawn_NativeVerbs instance)
        {
            return (Pawn)Main._pawnPawnNativeVerbs.GetValue(instance);
        }

        public static List<VerbProperties> cachedVerbProperties(Pawn_NativeVerbs instance)
        {
            return (List<VerbProperties>)Main._cachedVerbProperties.GetValue(instance);
        }

        public static Pawn Pawn_HealthTracker_GetPawn(Pawn_HealthTracker instance)
        {
            return (Pawn)Main.int_Pawn_HealthTracker_GetPawn.GetValue(instance);
        }

        public static Pawn PawnRenderer_GetPawn(object instance)
        {
            // TODO(1.5+): pawnField_PawnRenderer may be null if the "pawn" field was renamed in RimWorld 1.5+.
            if (Main.pawnField_PawnRenderer == null) return null;
            return (Pawn)Main.pawnField_PawnRenderer.GetValue(instance);
        }
        /*
        public static void BaseHeadOffsetAtPostfix(PawnRenderer __instance, ref Vector3 __result)
        {
            Pawn pawn = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();
            Vector2 offset = Vector2.zero;
            if (pawn.InBed() && pawn.CurrentBed() is Building_XenomorphCocoon cocoonthing)
            {
                //   Log.Message(string.Format("true"));
                if (cocoonthing.Rotation == Rot4.North)
                {
                    __result.z -= 0.5f;
                }
            }
            else
            {
                //   Log.Message(string.Format("false"));
            }
            __result.x += offset.x;
            __result.z += offset.y;
        }
        */
        public static Vector3 PushResult(Thing Caster, Thing thingToPush, int pushDist, out bool collision)
        {
            Vector3 vector = GenThing.TrueCenter(thingToPush);
            Vector3 result = vector;
            bool flag = false;
            for (int i = 1; i <= pushDist; i++)
            {
                int num = i;
                int num2 = i;
                bool flag2 = vector.x < GenThing.TrueCenter(Caster).x;
                if (flag2)
                {
                    num = -num;
                }
                bool flag3 = vector.z < GenThing.TrueCenter(Caster).z;
                if (flag3)
                {
                    num2 = -num2;
                }
                Vector3 vector2 = new Vector3(vector.x + (float)num, 0f, vector.z + (float)num2);
                bool flag4 = GenGrid.Standable(IntVec3Utility.ToIntVec3(vector2), Caster.Map);
                if (flag4)
                {
                    result = vector2;
                }
                else
                {
                    bool flag5 = thingToPush is Pawn;
                    if (flag5)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            collision = flag;
            return result;
        }

        public static void PushEffect(Thing Caster, Thing target, int distance, bool damageOnCollision = false)
        {
            if (target is Building)
            {
                return;
            }
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Pawn pawn;
                if (target != null && (pawn = (target as Pawn)) != null && pawn.Spawned && !pawn.Downed && !pawn.Dead && (pawn?.MapHeld) != null)
                {
                    bool drafted = pawn.Drafted;
                    Vector3 vector = Main.PushResult(Caster, target, distance, out bool flag2);
                    RRY_FlyingObject flyingObject = (RRY_FlyingObject)GenSpawn.Spawn(ThingDef.Named("JT_FlyingObject"), pawn.PositionHeld, pawn.MapHeld, 0);
                    bool flag3 = flag2 & damageOnCollision;
                    if (flag3)
                    {
                        flyingObject.Launch(Caster, new LocalTargetInfo(IntVec3Utility.ToIntVec3(vector)), target, new DamageInfo?(new DamageInfo(DamageDefOf.Blunt, (float)Rand.Range(8, 10), 0f, -1f, null, null, null, 0, null)));
                    }
                    else
                    {
                        flyingObject.Launch(Caster, new LocalTargetInfo(IntVec3Utility.ToIntVec3(vector)), target);
                    }

                }
            }, "PushingCharacter", false, null);
        }
        private static FieldInfo pawnField_PawnRenderer;

        public static FieldInfo int_Pawn_HealthTracker_GetPawn;

        private const BindingFlags allFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

    //    public static FieldInfo _jobsGivenRecentTicksTextual;

        public static FieldInfo _cachedVerbProperties;

        public static FieldInfo _pawnPawnNativeVerbs;
    }
    
    public abstract class CompWearable : ThingComp
    {
        public virtual IEnumerable<Gizmo> CompGetGizmosWorn() {
            // return no Gizmos
            return new List<Gizmo>();
        }
    }
   
}