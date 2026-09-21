using Verse;
using Verse.AI;

namespace RimWorld
{
    /// <summary>
    /// Think-tree last-resort fallback: stand still briefly.
    /// Used where JobGiver_Wander fails to find a wander root
    /// (enclosed cells, flooded rooms) so the pawn never falls
    /// through to JobGiver_IdleError.
    /// </summary>
    public class JobGiver_XenomorphWait : ThinkNode_JobGiver
    {
        protected int ticks = 60;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_XenomorphWait copy = (JobGiver_XenomorphWait)base.DeepCopy(resolve);
            copy.ticks = this.ticks;
            return copy;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            return new Job(JobDefOf.Wait, this.ticks, false);
        }
    }
}