using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Diagnostic timing for multi-cell designation (zone placement).
    /// Reports how long Designator.DesignateMultiCell took and over how many
    /// cells, to pin down the "zone placed over a xenomorph hive lags" report.
    ///
    /// - Logs EVERY placement slower than 100 ms (one line, no spam).
    /// - Logs every placement in detail when the mod's DebugMode setting is on.
    /// If placement time is small but the game still hitches, the cost is
    /// downstream (mesh regeneration, pathing, pawn job replanning), not the
    /// designator path.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class DesignatorMultiCellTimerPatch
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static int cellCount;

        static DesignatorMultiCellTimerPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.zoneperfprobe");
                var method = AccessTools.Method(typeof(Designator), "DesignateMultiCell");
                if (method != null)
                {
                    harmony.Patch(method,
                        prefix: new HarmonyMethod(typeof(DesignatorMultiCellTimerPatch), nameof(Prefix)),
                        postfix: new HarmonyMethod(typeof(DesignatorMultiCellTimerPatch), nameof(Postfix)));
                    Log.Message("[AVP Xenomorphs] Zone/designation perf probe installed (logs placements >100ms, or all with mod DebugMode)");
                }
                else
                {
                    Log.Warning("[AVP Xenomorphs] Could not find Designator.DesignateMultiCell — perf probe not applied");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init designation perf probe: " + e.Message);
            }
        }

        public static void Prefix(IEnumerable<IntVec3> cells)
        {
            cellCount = 0;
            foreach (IntVec3 c in cells) cellCount++;
            stopwatch.Restart();
        }

        public static void Postfix()
        {
            stopwatch.Stop();
            float ms = stopwatch.ElapsedMilliseconds;
            if (ms > 100f)
            {
                Log.Message($"[AVP Xenomorphs] [Perf] DesignateMultiCell took {ms:0.#} ms over {cellCount} cells (hive-area lag investigation)");
            }
            else if (AvPDebug.Enabled)
            {
                AvPDebug.LogOnce("multiCellTimer", $"[Perf] DesignateMultiCell probe active; recent placement: {ms:0.#} ms over {cellCount} cells");
            }
        }
    }
}
