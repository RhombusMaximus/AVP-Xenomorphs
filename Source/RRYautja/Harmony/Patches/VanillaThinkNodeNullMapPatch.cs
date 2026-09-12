using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Patches vanilla conditional think nodes that crash with NRE when
    /// a pawn has a null map (facehuggers being carried, despawned pawns,
    /// pawns in containers). The exceptions spam the log every tick and
    /// cause severe lag.
    ///
    /// Vanilla nodes affected (seen in player.log):
    ///   RimWorld.ThinkNode_ConditionalSkyDarker.Satisfied
    ///   RimWorld.ThinkNode_ConditionalSkyBrighter.Satisfied
    ///   RimWorld.ThinkNode_ConditionalPawnKind (crashes in sub-nodes)
    ///   RimWorld.ThinkNode_ConditionalNotPawnKind
    /// </summary>
    [StaticConstructorOnStartup]
    public static class VanillaThinkNodeNullMapPatch
    {
        static VanillaThinkNodeNullMapPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.thinknodenullmap");

                // Patch Satisfied(Pawn) on all vanilla ThinkNode_Conditional subclasses
                // that access the map. We find them by scanning RimWorld + Verse assemblies
                // for types deriving from ThinkNode_Conditional with a Satisfied method.
                int patched = 0;
                foreach (Type type in GetConditionalThinkNodeTypes())
                {
                    MethodInfo satisfied = AccessTools.Method(type, "Satisfied");
                    if (satisfied == null) continue;

                    var prefix = new HarmonyMethod(typeof(VanillaThinkNodeNullMapPatch), nameof(SatisfiedPrefix));
                    try
                    {
                        harmony.Patch(satisfied, prefix: prefix);
                        patched++;
                    }
                    catch { /* some may fail to patch — skip */ }
                }
                Log.Message($"[AVP Xenomorphs] Patched {patched} vanilla think node Satisfied methods with null-map guards");
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init vanilla think node patch: " + e.Message);
            }
        }

        private static IEnumerable<Type> GetConditionalThinkNodeTypes()
        {
            // The think nodes we've seen crash in logs. Patch these explicitly
            // rather than scanning the whole assembly (safer, avoids patching
            // our own already-guarded nodes twice — double null-check is fine,
            // but keep the list focused).
            yield return typeof(RimWorld.ThinkNode_ConditionalSkyDarker);
            yield return typeof(RimWorld.ThinkNode_ConditionalSkyBrighter);
            yield return typeof(RimWorld.ThinkNode_ConditionalPawnKind);
            yield return typeof(RimWorld.ThinkNode_ConditionalNotPawnKind);
        }

        /// <summary>
        /// Prefix on ThinkNode_Conditional.Satisfied(Pawn).
        /// If the pawn is null or has a null map, return false (not satisfied)
        /// and skip the original method — prevents NRE spam and the lag it causes.
        /// </summary>
        public static bool SatisfiedPrefix(Pawn pawn, ref bool __result)
        {
            if (pawn == null || pawn.Map == null)
            {
                __result = false;
                return false; // skip original — it would crash
            }
            return true; // run original
        }
    }
}