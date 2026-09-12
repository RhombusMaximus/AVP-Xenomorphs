using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Cleans up broken Blueprint_Install entries left behind when a hive wall
    /// is destroyed while an install blueprint (created by Replace Stuff) still
    /// references it. The blueprint throws "Nothing to install" NREs every time
    /// it's printed or spawned, spamming the log and causing lag.
    ///
    /// Patch:
    ///   Blueprint_Install.SpawnSetup — if the inner thing is missing, destroy the blueprint
    ///   Blueprint_Install.get_HeldThing / Print — return null / no-op if inner thing missing
    /// </summary>
    [StaticConstructorOnStartup]
    public static class BrokenInstallBlueprintPatch
    {
        static BrokenInstallBlueprintPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.brokenblueprint");

                // Patch SpawnSetup to destroy broken blueprints on load
                var spawnSetup = AccessTools.Method(typeof(Blueprint_Install), "SpawnSetup");
                if (spawnSetup != null)
                {
                    harmony.Patch(spawnSetup, postfix: new HarmonyMethod(typeof(BrokenInstallBlueprintPatch), nameof(SpawnSetupPostfix)));
                }

                Log.Message("[AVP Xenomorphs] Broken install blueprint cleanup patch applied");
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init broken blueprint patch: " + e.Message);
            }
        }

        /// <summary>
        /// After Blueprint_Install.SpawnSetup, check if the inner thing is null.
        /// If so, the blueprint is broken (its target wall was destroyed) —
        /// destroy the blueprint immediately so it stops throwing errors.
        /// </summary>
        public static void SpawnSetupPostfix(Blueprint_Install __instance)
        {
            try
            {
                if (__instance == null || __instance.Map == null) return;
                // If the thing to install is gone (wall destroyed), blueprint is broken
                Thing inner = __instance.MiniToInstallOrBuildingToReinstall;
                if (inner == null || inner.Destroyed)
                {
                    Log.Warning($"[AVP Xenomorphs] Destroying broken install blueprint at {__instance.Position} (nothing to install)");
                    __instance.Destroy(DestroyMode.Vanish);
                }
            }
            catch
            {
                // Never throw from here
            }
        }
    }
}