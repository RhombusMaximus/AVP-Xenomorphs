using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Blocks zone designation on tiles occupied by hive/tunnel buildings.
    ///
    /// Setting canOverlapZones=false via reflection on ThingDef did not fix the
    /// lag caused by Replace Stuff's transpiler scanning buildings during zone
    /// drawing. Blocking the designation at the source — in
    /// Designator_ZoneAdd.CanDesignateCell — prevents the zone from being
    /// placed on those tiles in the first place, so Replace Stuff never has to
    /// scan them.
    ///
    /// Blocked defNames:
    ///   RRY_Xenomorph_Hive, RRY_Xenomorph_Hive_Child, RRY_Xenomorph_Hive_Wall
    ///   TunnelHiveLikeSpawner, TunnelHiveLikeChildSpawner
    ///   RRY_Tunneler
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ZoneDesignatorPatch
    {
        static ZoneDesignatorPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.zonedesignator");
                var method = AccessTools.Method(typeof(Designator_ZoneAdd), "CanDesignateCell");
                if (method != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(ZoneDesignatorPatch), nameof(CanDesignateCellPrefix)));
                    Log.Message("[AVP Xenomorphs] Patched Designator_ZoneAdd.CanDesignateCell to block hive/tunnel tiles");
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] Could not find Designator_ZoneAdd.CanDesignateCell — zone block patch not applied");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init zone designator patch: " + e.Message);
            }
        }

        /// <summary>
        /// Prefix on Designator_ZoneAdd.CanDesignateCell(IntVec3 loc).
        /// Returns false (skipping the original method) with __result=false
        /// when the cell contains a hive/tunnel building, so the cell is not
        /// designatable for a zone.
        /// </summary>
        public static bool CanDesignateCellPrefix(IntVec3 loc, ref AcceptanceReport __result)
        {
            try
            {
                Map map = Find.CurrentMap;
                if (map == null) return true; // no map — let original handle it

                Building building = loc.GetFirstBuilding(map);
                if (building == null || building.def == null) return true;

                string defName = building.def.defName;
                if (defName == null) return true;

                if (defName.StartsWith("RRY_Xenomorph_Hive")
                    || defName.StartsWith("TunnelHiveLike")
                    || defName == "RRY_Tunneler")
                {
                    __result = false; // AcceptanceReport has implicit bool->report conversion
                    return false; // skip original CanDesignateCell
                }
            }
            catch
            {
                // Never let this patch throw into the designator — fall back to vanilla.
            }
            return true; // run original method
        }
    }
}