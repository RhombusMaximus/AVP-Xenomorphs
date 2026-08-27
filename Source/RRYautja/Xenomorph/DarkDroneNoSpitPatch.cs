using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    /// <summary>
    /// Removes acid spit weapon from Dark variant Drones after spawn.
    /// Dark drones (odd pawn ID) don't spit acid.
    /// </summary>
    [StaticConstructorOnStartup]
    static class DarkDroneNoSpitPatch
    {
        static DarkDroneNoSpitPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.darkdrone");
                var method = AccessTools.Method(typeof(Pawn), "SpawnSetup");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(DarkDroneNoSpitPatch), nameof(SpawnSetupPostfix)));
                    AvPDebug.LogOnce("DarkDrone", "[AVP Xenomorphs] Patched Pawn.SpawnSetup for dark drone spit removal");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init dark drone patch: " + e.Message);
            }
        }

        public static void SpawnSetupPostfix(Pawn __instance)
        {
            try
            {
                if (__instance == null) return;
                if (__instance.kindDef?.defName != "RRY_Xenomorph_Drone") return;

                // Dark drones have odd ID numbers — no acid spit
                if (__instance.thingIDNumber % 2 != 1) return;

                // Remove acid spit weapon if present
                if (__instance.equipment == null) return;
                var weapon = __instance.equipment.Primary;
                if (weapon != null && weapon.def.defName == "RRY_Gun_DroneAcidSpit")
                {
                    __instance.equipment.Remove(weapon);
                    AvPDebug.Log("DarkDrone", "Removed acid spit from Dark drone " + __instance.LabelShort);
                }
            }
            catch (Exception e)
            {
                AvPDebug.Error("Dark drone patch error: " + e.Message);
            }
        }
    }
}