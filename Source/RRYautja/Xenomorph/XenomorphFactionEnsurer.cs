using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Ensures the Xenomorph faction exists when a new game starts.
    /// Patches Game.InitNewGame and Game.LoadGame to create the faction if missing.
    /// </summary>
    [StaticConstructorOnStartup]
    static class XenomorphFactionEnsurer
    {
        static XenomorphFactionEnsurer()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.factionensurer");

                // Patch Game.InitNewGame
                var initMethod = AccessTools.Method(typeof(Game), "InitNewGame");
                if (initMethod != null)
                {
                    harmony.Patch(initMethod, postfix: new HarmonyMethod(typeof(XenomorphFactionEnsurer), nameof(EnsureFactionPostfix)));
                    AvPDebug.LogOnce("Patch", "[AVP Xenomorphs] Patched Game.InitNewGame for faction ensurer");
                }

                // Patch Game.LoadGame
                var loadMethod = AccessTools.Method(typeof(Game), "LoadGame");
                if (loadMethod != null)
                {
                    harmony.Patch(loadMethod, postfix: new HarmonyMethod(typeof(XenomorphFactionEnsurer), nameof(EnsureFactionPostfix)));
                    AvPDebug.LogOnce("Patch", "[AVP Xenomorphs] Patched Game.LoadGame for faction ensurer");
                }
            }
            catch (System.Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init faction ensurer: " + e.Message);
            }
        }

        public static void EnsureFactionPostfix()
        {
            FactionDef xenoDef = XenomorphDefOf.RRY_Xenomorph;
            if (xenoDef == null)
            {
                Log.Error("[AVP Xenomorphs] RRY_Xenomorph FactionDef is null!");
                return;
            }

            Faction existing = Find.FactionManager?.FirstFactionOfDef(xenoDef);
            if (existing != null)
            {
                AvPDebug.LogOnce("FactionExists", "Xenomorph hive faction already exists.");
                return;
            }

            // Create the faction using RimWorld's faction manager
            Faction faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(xenoDef));
            Find.FactionManager.Add(faction);

            // Initialize relations with all existing factions (fixes "null relation" warnings)
            foreach (var other in Find.FactionManager.AllFactions)
            {
                if (other == faction) continue;
                if (faction.RelationWith(other, false) == null)
                {
                    // Create initial relation, then set hostile (Xenomorphs are permanent enemies)
                    faction.TryMakeInitialRelationsWith(other);
                    faction.SetRelationDirect(other, FactionRelationKind.Hostile, false, null, null);
                }
            }

            AvPDebug.LogOnce("FactionCreated", "Created Xenomorph hive faction (was missing).");
        }
    }
}