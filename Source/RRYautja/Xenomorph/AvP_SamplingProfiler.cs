using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using HarmonyLib;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Main-thread sampling profiler for lag diagnosis.
    ///
    /// Active ONLY while the mod's DebugMode setting is enabled (settings ->
    /// AVP Xenomorphs -> Debug). While active it:
    ///   - measures frame/tick time (postfix on TickManager.DoSingleTick)
    ///   - samples the main thread's stack ~40x per second
    ///   - every ~45 seconds, dumps a report: sample count, slow-tick count,
    ///     and the top aggregated main-thread stacks with hit percentages.
    ///
    /// The hot method during a lag episode will dominate the dump regardless
    /// of which mod it belongs to. StackTrace(Thread, bool) suspends the main
    /// thread for the snapshot; at 25 ms intervals the overhead is negligible.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class AvP_SamplingProfiler
    {
        private const int SampleIntervalMs = 25;
        private const int DumpEverySamples = 1800; // ~45 s of sampling

        private static Thread mainThread;
        private static Thread samplerThread;
        private static readonly Stopwatch tickWatch = new Stopwatch();
        private static readonly object statsLock = new object();
        private static readonly Dictionary<string, int> stackCounts = new Dictionary<string, int>();
        private static int totalSamples;
        private static int slowTicks;      // inter-tick gap > 40 ms
        private static long worstTickMs;
        private static long totalTickMs;
        private static int tickMeasurements;

        static AvP_SamplingProfiler()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.samplingprofiler");
                var method = AccessTools.Method(typeof(TickManager), "DoSingleTick");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(AvP_SamplingProfiler), nameof(TickPostfix)));
                    Log.Message("[AVP Xenomorphs] Sampling profiler installed (active while mod DebugMode is on; dumps a report every ~45s)");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init sampling profiler: " + e.Message);
            }
        }

        public static void TickPostfix()
        {
            try
            {
                if (mainThread == null)
                {
                    mainThread = Thread.CurrentThread;
                    tickWatch.Start();
                    return;
                }
                tickWatch.Stop();
                long ms = tickWatch.ElapsedMilliseconds;
                tickWatch.Restart();
                lock (statsLock)
                {
                    tickMeasurements++;
                    totalTickMs += ms;
                    if (ms > worstTickMs) worstTickMs = ms;
                    if (ms > 40) slowTicks++;
                }
                EnsureSamplerThread();
            }
            catch
            {
                // never throw into the tick loop
            }
        }

        private static void EnsureSamplerThread()
        {
            if (!AvPDebug.Enabled) return;
            if (samplerThread != null && samplerThread.IsAlive) return;
            samplerThread = new Thread(SamplerLoop) { IsBackground = true, Name = "AvP_SamplingProfiler" };
            samplerThread.Start();
        }

        private static void SamplerLoop()
        {
            while (AvPDebug.Enabled && Current.ProgramState == ProgramState.Playing)
            {
                int samplesAtDump;
                lock (statsLock)
                {
                    samplesAtDump = totalSamples + DumpEverySamples;
                }
                int windowStartTicks = slowTicks;
                long windowStartWorst = worstTickMs;
                while (true)
                {
                    Thread.Sleep(SampleIntervalMs);
                    lock (statsLock)
                    {
                        if (!AvPDebug.Enabled) return;
                        totalSamples++;
                        if (totalSamples >= samplesAtDump) break;
                    }
                    RecordSample();
                }
                DumpReport(windowStartTicks, windowStartWorst);
            }
        }

        private static void RecordSample()
        {
#if NETFRAMEWORK
            // StackTrace(Thread, bool) only exists on .NET Framework — the 1.6 build.
            // On net10 (1.7/Waylith) thread-stack sampling is unavailable and we
            // fall back to tick/frame timing only.
            try
            {
                if (mainThread == null) return;
                var trace = new StackTrace(mainThread, false);
                var frames = trace.GetFrames();
                if (frames == null) return;
                // Key = the two topmost managed frames (enough to identify the hot method)
                string key = null;
                int taken = 0;
                var sb = new StringBuilder();
                for (int i = 0; i < frames.Length && taken < 2; i++)
                {
                    var m = frames[i].GetMethod();
                    if (m == null) continue;
                    var declaring = m.DeclaringType;
                    if (declaring == null) continue;
                    var name = declaring.FullName + "." + m.Name;
                    if (name.StartsWith("System.") || name.StartsWith("mscorlib")) continue;
                    sb.Append(taken == 0 ? name : (" <- " + name));
                    taken++;
                }
                if (taken == 0) return;
                key = sb.ToString();
                lock (statsLock)
                {
                    stackCounts.TryGetValue(key, out int c);
                    stackCounts[key] = c + 1;
                }
            }
            catch
            {
                // sample failed (e.g. thread in native transition) — ignore
            }
#endif
        }

        private static void DumpReport(int windowStartSlowTicks, long windowStartWorst)
        {
            try
            {
            List<KeyValuePair<string, int>> top;
            int samples;
            int slow, meas;
            long worst, avg;
            lock (statsLock)
            {
                samples = totalSamples;
                slow = slowTicks;
                meas = tickMeasurements;
                worst = worstTickMs;
                avg = meas > 0 ? totalTickMs / meas : 0;
                top = stackCounts.OrderByDescending(kv => kv.Value).Take(25).ToList();
                stackCounts.Clear();
                // reset per-window slow/worst baselines so each report covers its own window
                slowTicks = 0;
                worstTickMs = 0;
                tickMeasurements = 0;
                totalTickMs = 0;
            }
            if (samples == 0) return;
            var report = new StringBuilder();
            report.AppendLine($"[AVP Xenomorphs] [Perf] Sampling report: {samples} samples, tick/frame avg {avg} ms, worst {worst} ms, slow (>40ms) {slow}");
            foreach (var kv in top)
            {
                report.AppendLine($"[AVP Xenomorphs] [Perf]   {100 * kv.Value / (double)samples,5:0.0}%  {kv.Key}");
            }
            Log.Message(report.ToString().TrimEndNewlines());
            }
            catch (Exception e)
            {
                // Never let the report kill the sampler thread — log and keep sampling.
                Verse.Log.Warning("[AVP Xenomorphs] [Perf] Sampling report failed: " + e.Message);
            }
        }
    }
}
