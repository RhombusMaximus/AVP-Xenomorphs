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
                    Log.Message("[AVP Xenomorphs] Patched Game.InitNewGame for faction ensurer");
                }

                // Patch Game.LoadGame
                var loadMethod = AccessTools.Method(typeof(Game), "LoadGame");
                if (loadMethod != null)
                {
                    harmony.Patch(loadMethod, postfix: new HarmonyMethod(typeof(XenomorphFactionEnsurer), nameof(EnsureFactionPostfix)));
                    Log.Message("[AVP Xenomorphs] Patched Game.LoadGame for faction ensurer");
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
                Log.Message("[AVP Xenomorphs] Xenomorph hive faction already exists.");
                return;
            }

            // Create the faction using RimWorld's faction manager
            Faction faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(xenoDef));
            Find.FactionManager.Add(faction);
            Log.Message("[AVP Xenomorphs] Created Xenomorph hive faction (was missing).");
        }
    }
}