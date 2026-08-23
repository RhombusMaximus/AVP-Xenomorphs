using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    /// <summary>
    /// When a Xenomorph mines rock, suppress chunk/filth spawning.
    /// </summary>
    [StaticConstructorOnStartup]
    static class AvP_XenomorphMinePatch
    {
        static AvP_XenomorphMinePatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.minepatch");

                // List all methods on Mineable for debugging
                var allMethods = typeof(Mineable).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var m in allMethods)
                {
                    Log.Message("[AVP Xenomorphs] Mineable method: " + m.Name + " (" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
                }

                // Try common method names for yield/drop/spawn
                string[] methodNames = { "YieldComponents", "DestroyedMineable", "StruckMineable",
                    "Notify_DestroyedMineable", "DropProducts", "SpawnProducts", "GetYield",
                    "Notify_MineableDestroyed", "OnMineableDestroyed", "ProduceYield",
                    "GetEffectiveMineableYield", "EffectiveMineableYield" };

                foreach (var name in methodNames)
                {
                    var method = AccessTools.Method(typeof(Mineable), name);
                    if (method != null)
                    {
                        Log.Message("[AVP Xenomorphs] Found Mineable." + name + " - patching");
                        harmony.Patch(method, prefix: new HarmonyMethod(typeof(AvP_XenomorphMinePatch), nameof(MineableMethodPrefix)));
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init mine patch: " + e.Message);
            }
        }

        /// <summary>
        /// Check if any Xenomorph pawn on the map is currently mining this thing.
        /// If so, suppress the method (return false = skip original).
        /// </summary>
        public static bool MineableMethodPrefix(Mineable __instance, string __methodName)
        {
            // For yield/drop/destroy methods, check if a Xenomorph is mining
            Map map = __instance.Map;
            if (map == null) return true;

            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.isXenomorph()) continue;
                if (pawn.CurJobDef == JobDefOf.Mine)
                {
                    var target = pawn.CurJob.targetA.Thing;
                    if (target == __instance)
                    {
                        Log.Message("[AVP Xenomorphs] Suppressing " + __methodName + " for Xenomorph-mined " + __instance.def.defName);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}