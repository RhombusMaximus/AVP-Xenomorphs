using Verse;

namespace RRYautja
{
    /// <summary>
    /// Shared tick throttle for job givers. Returns true only on every Nth tick,
    /// staggered by pawn hash so different pawns fire on different ticks.
    /// </summary>
    public static class JobGiverTickThrottle
    {
        public const int Interval = 3;

        /// <summary>
        /// Returns true if this pawn should run its job giver this tick.
        /// Staggered by pawn.thingIDNumber so not all pawns fire on the same tick.
        /// </summary>
        public static bool ShouldRun(Pawn pawn)
        {
            if (pawn == null) return false;
            return (Find.TickManager.TicksGame + pawn.thingIDNumber) % Interval == 0;
        }
    }
}