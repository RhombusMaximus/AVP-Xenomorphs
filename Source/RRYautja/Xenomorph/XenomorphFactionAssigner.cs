using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    /// <summary>
    /// Ensures ALL spawned Xenomorph pawns get the Xenomorph faction.
    /// Patches PawnGenerator.GeneratePawn to set faction on Xenomorph pawns.
    /// </summary>
    [StaticConstructorOnStartup]
    static class XenomorphFactionAssigner
    {
        static XenomorphFactionAssigner()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.factionassigner");

                // Patch all GeneratePawn overloads
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
        /// After any pawn is generated, if it's a Xenomorph and has no faction, assign the Xenomorph faction.
        /// </summary>
        public static void GeneratePawnPostfix(ref Pawn __result)
        {
            if (__result == null) return;
            if (!__result.isXenomorph()) return;
            if (__result.Faction != null) return;

            // Find the Xenomorph faction
            FactionDef xenoDef = XenomorphDefOf.RRY_Xenomorph;
            if (xenoDef == null) return;

            Faction xenoFaction = Find.FactionManager?.FirstFactionOfDef(xenoDef);
            if (xenoFaction != null)
            {
                __result.SetFaction(xenoFaction);
                AvPDebug.Log("Faction", "Force-set faction on " + __result.LabelShort + " (was null)");
            }
            else if (AvPDebug.Enabled)
            {
                AvPDebug.Warning("Xenomorph faction is null - " + __result.LabelShort + " spawned with no faction");
            }
        }
    }
}