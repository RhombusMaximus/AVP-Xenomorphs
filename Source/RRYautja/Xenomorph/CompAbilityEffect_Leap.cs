using RimWorld;
using Verse;
using Verse.AI;

namespace RRYautja
{
    /// <summary>
    /// Leap ability effect for Facehuggers and Runners.
    /// Uses vanilla JumpUtility.DoJump to launch the pawn toward the target.
    /// </summary>
    public class CompAbilityEffect_Leap : CompAbilityEffect
    {
        public new CompProperties_AbilityLeap Props
        {
            get { return (CompProperties_AbilityLeap)this.props; }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = this.parent.pawn;
            if (pawn == null || pawn.Map == null) return;

            IntVec3 destCell = target.Cell;
            if (!destCell.IsValid || !destCell.InBounds(pawn.Map)) return;

            try
            {
                // Use vanilla JumpUtility.DoJump — the standard 1.6 way to make pawns leap
                JumpUtility.DoJump(pawn, target, null, this.parent.verb.verbProps, this.parent, target, null);
            }
            catch (System.Exception e)
            {
                Log.Error($"[AVP Xenomorphs] Leap failed for {pawn.LabelShort}: {e.Message}");
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = this.parent.pawn;
            if (pawn == null || pawn.Map == null) return false;
            if (!target.Cell.IsValid || !target.Cell.InBounds(pawn.Map)) return false;
            float dist = pawn.Position.DistanceTo(target.Cell);
            return dist >= Props.minDistance && dist <= Props.maxDistance;
        }

        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            return false;
        }
    }

    public class CompProperties_AbilityLeap : CompProperties_AbilityEffect
    {
        public float minDistance = 2f;
        public float maxDistance = 8f;

        public CompProperties_AbilityLeap()
        {
            this.compClass = typeof(CompAbilityEffect_Leap);
        }
    }
}