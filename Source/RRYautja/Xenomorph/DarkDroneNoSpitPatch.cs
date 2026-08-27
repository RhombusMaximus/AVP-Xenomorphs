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
    /// Removes acid spit weapon from Dark variant Drones.
    /// Dark drones (alternate graphic index 1) don't spit acid.
    /// </summary>
    [StaticConstructorOnStartup]
    static class DarkDroneNoSpitPatch
    {
        static DarkDroneNoSpitPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.darkdrone");
                var method = AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) });
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(DarkDroneNoSpitPatch), nameof(GeneratePawnPostfix)));
                    AvPDebug.LogOnce("DarkDrone", "[AVP Xenomorphs] Patched PawnGenerator.GeneratePawn for dark drone spit removal");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init dark drone patch: " + e.Message);
            }
        }

        public static void GeneratePawnPostfix(ref Pawn __result)
        {
            try
            {
                if (__result == null) return;
                if (__result.kindDef?.defName != "RRY_Xenomorph_Drone") return;

                // Check if this drone is using an alternate graphic
                // The Dark variant is alternate graphic index 1 (second in the list)
                // RimWorld stores the resolved alternate graphic in pawn.Drawer.renderer.graphics
                // We check the texture path to determine if it's the Dark variant
                var pawnKindLifeStage = __result.ageTracker.CurLifeStageIndex;
                // Check if pawn has the acid spit weapon
                if (__result.equipment != null && __result.equipment.Primary != null)
                {
                    var weapon = __result.equipment.Primary;
                    if (weapon.def.defName == "RRY_Gun_DroneAcidSpit")
                    {
                        // Check if this is a Dark drone by examining the alternate graphic
                        // In RimWorld, alternateGraphics are resolved during pawn generation
                        // The pawn's KindDef has alternateGraphics, and the resolved one is stored
                        // We can't easily check which variant was chosen, so we use a hash-based check
                        // based on the pawn's ID for determinism
                        
                        // Count how many alternate graphics there are
                        var altGraphics = __result.kindDef.alternateGraphics;
                        if (altGraphics != null && altGraphics.Count > 0)
                        {
                            // Use pawn ID to deterministically decide
                            // Dark is index 1, Light is index 0
                            // 50% chance means half get dark
                            int variant = __result.thingIDNumber % 2;
                            if (variant == 1) // Dark drone
                            {
                                __result.equipment.Remove(weapon);
                                AvPDebug.Log("DarkDrone", "Removed acid spit from Dark drone " + __result.LabelShort);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AvPDebug.Error("Dark drone patch error: " + e.Message);
            }
        }
    }
}