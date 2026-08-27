using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RRYautja
{
    /// <summary>
    /// Ensures ALL spawned Xenomorph pawns get the Xenomorph faction.
    /// Patches PawnGenerator.GeneratePawn to set faction on Xenomorph pawns.
    /// </summary>
    [StaticConstructorOnStartup]
    static class XenomorphFactionAssigner
    {
        private static Faction cachedXenoFaction = null;
        private static int lastFactionCheckTick = -1;

        static XenomorphFactionAssigner()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.factionassigner");

                var methods = typeof(PawnGenerator).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                int patched = 0;
                foreach (var m in methods)
                {
                    if (m.Name == "GeneratePawn" && m.ReturnType == typeof(Pawn))
                    {
                        harmony.Patch(m, postfix: new HarmonyMethod(typeof(XenomorphFactionAssigner), nameof(GeneratePawnPostfix)));
                        patched++;
                    }
                }
                AvPDebug.LogOnce("Patch", "[AVP Xenomorphs] Patched PawnGenerator.GeneratePawn (" + patched + " overloads) for faction assignment");
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init faction assigner: " + e.Message);
            }
        }

        /// <summary>
        /// Get the Xenomorph faction, caching the result and refreshing every 250 ticks.
        /// </summary>
        private static Faction GetXenoFaction()
        {
            // Cache the faction for 250 ticks to avoid repeated lookups
            int currentTick = Find.TickManager?.TicksGame ?? 0;
            if (cachedXenoFaction != null && currentTick - lastFactionCheckTick < 250)
            {
                return cachedXenoFaction;
            }
            lastFactionCheckTick = currentTick;

            FactionDef xenoDef = XenomorphDefOf.RRY_Xenomorph;
            if (xenoDef == null) return null;

            cachedXenoFaction = Find.FactionManager?.FirstFactionOfDef(xenoDef);
            
            // If faction still null, try to create it
            if (cachedXenoFaction == null)
            {
                try
                {
                    cachedXenoFaction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(xenoDef));
                    Find.FactionManager.Add(cachedXenoFaction);
                    Log.Message("[AVP Xenomorphs] Created Xenomorph faction during pawn generation (was missing).");
                }
                catch (System.Exception e)
                {
                    Log.Error("[AVP Xenomorphs] Failed to create faction during pawn gen: " + e.Message);
                }
            }
            
            return cachedXenoFaction;
        }

        public static void GeneratePawnPostfix(ref Pawn __result)
        {
            if (__result == null) return;
            if (__result.Faction != null) return;
            
            // Fast check: only Xenomorph races need faction assignment
            // Check flesh type directly instead of using isXenomorph() extension
            if (__result.RaceProps?.FleshType != XenomorphRacesDefOf.RRY_Xenomorph) return;

            Faction xenoFaction = GetXenoFaction();
            if (xenoFaction != null)
            {
                __result.SetFaction(xenoFaction);
            }
        }
    }
}