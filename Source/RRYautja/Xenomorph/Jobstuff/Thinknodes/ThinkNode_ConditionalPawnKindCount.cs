using RRYautja;
using System;
using Verse;
using Verse.AI;
using System.Linq;

namespace RimWorld
{
    // Token: 0x020001EC RID: 492
    public class ThinkNode_ConditionalPawnKindCountGreater : ThinkNode_Conditional
    {
        // Token: 0x060009B8 RID: 2488 RVA: 0x0004E07C File Offset: 0x0004C47C
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalPawnKindCountGreater thinkNode_ConditionalCountPawnKind = (ThinkNode_ConditionalPawnKindCountGreater)base.DeepCopy(resolve);
            thinkNode_ConditionalCountPawnKind.pawnKind = this.pawnKind;
            thinkNode_ConditionalCountPawnKind.pawnKindCount = this.pawnKindCount;
            return thinkNode_ConditionalCountPawnKind;
        }

        // Token: 0x060009B9 RID: 2489 RVA: 0x0004E0A3 File Offset: 0x0004C4A3
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
        //    if (Find.Selector.SelectedObjects.Contains(pawn)) Log.Message(string.Format("{0} needs < {2} {3}, Result: {1}", this, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).Count < this.pawnKindCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).NullOrEmpty(), pawnKindCount, pawnKind.LabelCap));
            return pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).Count > this.pawnKindCount && !pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).NullOrEmpty();
        }

        // Token: 0x040003F6 RID: 1014
        public PawnKindDef pawnKind;
        public int pawnKindCount;
    }

    // Token: 0x020001EC RID: 492
    public class ThinkNode_ConditionalPawnKindCountLesser : ThinkNode_Conditional
    {
        // Token: 0x060009B8 RID: 2488 RVA: 0x0004E07C File Offset: 0x0004C47C
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalPawnKindCountLesser thinkNode_ConditionalCountPawnKind = (ThinkNode_ConditionalPawnKindCountLesser)base.DeepCopy(resolve);
            thinkNode_ConditionalCountPawnKind.pawnKind = this.pawnKind;
            thinkNode_ConditionalCountPawnKind.pawnKindCount = this.pawnKindCount;
            return thinkNode_ConditionalCountPawnKind;
        }

        // Token: 0x060009B9 RID: 2489 RVA: 0x0004E0A3 File Offset: 0x0004C4A3
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
        //    if (Find.Selector.SelectedObjects.Contains(pawn)) Log.Message(string.Format("{0} needs < {2} {3}, Result: {1}", this, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).Count < this.pawnKindCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).NullOrEmpty(), pawnKindCount, pawnKind.LabelCap));
            return pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).Count < this.pawnKindCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.kindDef == pawnKind).NullOrEmpty();
        }

        // Token: 0x040003F6 RID: 1014
        public PawnKindDef pawnKind;
        public int pawnKindCount;
    }

    public class ThinkNode_ConditionalImpregnatedCountGreater : ThinkNode_Conditional
    {
        // Token: 0x060009B8 RID: 2488 RVA: 0x0004E07C File Offset: 0x0004C47C
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalImpregnatedCountGreater thinkNode_ConditionalCountPawnKind = (ThinkNode_ConditionalImpregnatedCountGreater)base.DeepCopy(resolve);
            thinkNode_ConditionalCountPawnKind.victimCount = this.victimCount;
            return thinkNode_ConditionalCountPawnKind;
        }

        // Token: 0x060009B9 RID: 2489 RVA: 0x0004E0A3 File Offset: 0x0004C4A3
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
            if (Find.Selector.SelectedObjects.Contains(pawn)) Log.Message(string.Format("{0} needs > {2}, Found: {3} Result: {1}", this, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).Count < this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).NullOrEmpty(), victimCount, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).Count));
            return pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).Count > this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).NullOrEmpty();
        }

        public int victimCount;
    }

    // Token: 0x020001EC RID: 492
    public class ThinkNode_ConditionalImpregnatedCountLesser : ThinkNode_Conditional
    {
        // Token: 0x060009B8 RID: 2488 RVA: 0x0004E07C File Offset: 0x0004C47C
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalImpregnatedCountLesser thinkNode_ConditionalCountPawnKind = (ThinkNode_ConditionalImpregnatedCountLesser)base.DeepCopy(resolve);
            thinkNode_ConditionalCountPawnKind.victimCount = this.victimCount;
            return thinkNode_ConditionalCountPawnKind;
        }

        // Token: 0x060009B9 RID: 2489 RVA: 0x0004E0A3 File Offset: 0x0004C4A3
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
            if (Find.Selector.SelectedObjects.Contains(pawn)) Log.Message(string.Format("{0} needs < {2}, Found: {3} Result: {1}", this, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).Count < this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).NullOrEmpty(), victimCount, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).Count));
            return pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).Count < this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => XenomorphUtil.IsInfectedPawn(x)).NullOrEmpty();
        }

        public int victimCount;
    }

    public class ThinkNode_ConditionalFacehuggedCountGreater : ThinkNode_Conditional
    {
        // Token: 0x060009B8 RID: 2488 RVA: 0x0004E07C File Offset: 0x0004C47C
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalFacehuggedCountGreater thinkNode_ConditionalCountPawnKind = (ThinkNode_ConditionalFacehuggedCountGreater)base.DeepCopy(resolve);
            thinkNode_ConditionalCountPawnKind.victimCount = this.victimCount;
            return thinkNode_ConditionalCountPawnKind;
        }

        // Token: 0x060009B9 RID: 2489 RVA: 0x0004E0A3 File Offset: 0x0004C4A3
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
            if (Find.Selector.SelectedObjects.Contains(pawn)) Log.Message(string.Format("{0} needs > {2}, Found: {3} Result: {1}", this, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).Count < this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).NullOrEmpty(), victimCount, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).Count));
            return pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).Count > this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).NullOrEmpty();
        }

        public int victimCount;
    }

    // Token: 0x020001EC RID: 492
    public class ThinkNode_ConditionalFacehuggedCountLesser : ThinkNode_Conditional
    {
        // Token: 0x060009B8 RID: 2488 RVA: 0x0004E07C File Offset: 0x0004C47C
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalFacehuggedCountLesser thinkNode_ConditionalCountPawnKind = (ThinkNode_ConditionalFacehuggedCountLesser)base.DeepCopy(resolve);
            thinkNode_ConditionalCountPawnKind.victimCount = this.victimCount;
            return thinkNode_ConditionalCountPawnKind;
        }

        // Token: 0x060009B9 RID: 2489 RVA: 0x0004E0A3 File Offset: 0x0004C4A3
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null) return false;
            if (Find.Selector.SelectedObjects.Contains(pawn)) Log.Message(string.Format("{0} needs < {2}, Found: {3} Result: {1}", this, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).Count < this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).NullOrEmpty(), victimCount, pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).Count));
            return pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).Count < this.victimCount || pawn.Map.mapPawns.AllPawnsSpawned.ToList().FindAll(x => x.health.hediffSet.HasHediff(XenomorphDefOf.RRY_FaceHuggerInfection)).NullOrEmpty();
        }

        public int victimCount;
    }
}
