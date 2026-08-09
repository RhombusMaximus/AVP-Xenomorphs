// TODO: DebugAction system was reworked in RimWorld 1.6 - needs migration
#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RRYautja;
using RRYautja.ExtensionMethods;
using Verse.AI.Group;

namespace Verse
{
    public static class DebugToolsPawnAvP
    {
        [DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void XenomorphFacehugger(Pawn p)
        {
            HediffDef hediffDef = XenomorphDefOf.RRY_FaceHuggerInfection;
            PawnKindDef kindDef = XenomorphDefOf.RRY_Xenomorph_FaceHugger;
            if (!p.isPotentialHost(out string fail))
            {
                if (!fail.NullOrEmpty())
                {
                    Log.Error(string.Format("Failed to Add {0} to {1}, Reason: {2}", hediffDef.LabelCap, p.NameShortColored, fail));
                }
            }
            BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                                             where !x.def.conceptual && x.def.defName.Contains("Head") && !x.def.defName.Contains("Claw")
                                             select x).First<BodyPartRecord>();
            if (bodyPartRecord != null)
            {
                p.health.AddHediff(hediffDef, bodyPartRecord, null, null);
                Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                HediffComp_XenoFacehugger hediffcomp = hediff.TryGetComp<HediffComp_XenoFacehugger>();
                Pawn hugger = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph), PawnGenerationContext.NonPlayer, newborn: true));
                hediffcomp.instigator = hugger;
                hediffcomp.TryAcceptThing(hugger);
            }
            else
            {
                Log.Error(string.Format("Failed to Add {0} to {1}, Reason: bodyPartRecord == Null", hediffDef.LabelCap, p.NameShortColored, fail));
            }
        }

        [DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void XenomorphFacehuggerRoyale(Pawn p)
        {
            HediffDef hediffDef = XenomorphDefOf.RRY_FaceHuggerInfection;
            PawnKindDef kindDef = XenomorphDefOf.RRY_Xenomorph_RoyaleHugger;
            if (!p.isPotentialHost(out string fail))
            {
                if (!fail.NullOrEmpty())
                {
                    Log.Error(string.Format("Failed to Add {0} to {1}, Reason: {2}", hediffDef.LabelCap, p.NameShortColored, fail));
                }
            }
            BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                                             where !x.def.conceptual && x.def.defName.Contains("Head") && !x.def.defName.Contains("Claw")
                                             select x).First<BodyPartRecord>();
            if (bodyPartRecord != null)
            {
                p.health.AddHediff(hediffDef, bodyPartRecord, null, null);
                Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                HediffComp_XenoFacehugger hediffcomp = hediff.TryGetComp<HediffComp_XenoFacehugger>();
                Pawn hugger = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, Find.FactionManager.FirstFactionOfDef(XenomorphDefOf.RRY_Xenomorph), PawnGenerationContext.NonPlayer, newborn: true));
                hediffcomp.instigator = hugger;
                hediffcomp.royaleHugger = true;
                hediffcomp.TryAcceptThing(hugger);
            }
            else
            {
                Log.Error(string.Format("Failed to Add {0} to {1}, Reason: bodyPartRecord == Null", hediffDef.LabelCap, p.NameShortColored));
            }
        }

        [DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void XenomorphImpregnation(Pawn p)
        {
            HediffDef hediffDef = XenomorphDefOf.RRY_XenomorphImpregnation;
            if (!p.isPotentialHost(out string fail))
            {
                if (!fail.NullOrEmpty())
                {
                    Log.Error(string.Format("Failed to Add {0} to {1}, Reason: {2}", hediffDef.LabelCap, p.NameShortColored, fail));
                }
            }
            BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                                             where !x.def.conceptual && x.IsCorePart
                                             select x).First<BodyPartRecord>();
            if (bodyPartRecord != null)
            {
                p.health.AddHediff(hediffDef, bodyPartRecord, null, null);
                Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                hediff.Severity = 0.90f;
            }
            else
            {
                Log.Error(string.Format("Failed to Add {0} to {1}, Reason: bodyPartRecord == Null", hediffDef.LabelCap, p.NameShortColored));
            }
        }

        [DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void XenomorphImpregnationRoyale(Pawn p)
        {
            HediffDef hediffDef = XenomorphDefOf.RRY_XenomorphImpregnation;
            if (!p.isPotentialHost(out string fail))
            {
                if (!fail.NullOrEmpty())
                {
                    Log.Error(string.Format("Failed to Add {0} to {1}, Reason: {2}", hediffDef.LabelCap, p.NameShortColored, fail));
                }
            }
            BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                                             where !x.def.conceptual && x.IsCorePart
                                             select x).First<BodyPartRecord>();
            if (bodyPartRecord != null)
            {
                p.health.AddHediff(hediffDef, bodyPartRecord, null, null);
                Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                HediffComp_XenoSpawner hediffcomp = hediff.TryGetComp<HediffComp_XenoSpawner>();
                hediffcomp.royaleHugger = true;
                hediff.Severity = 0.90f;
            }
            else
            {
                Log.Error(string.Format("Failed to Add {0} to {1}, Reason: bodyPartRecord == Null", hediffDef.LabelCap, p.NameShortColored));
            }
        }

        [DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void NeomorphImpregnation(Pawn p)
        {
            HediffDef hediffDef = XenomorphDefOf.RRY_NeomorphImpregnation;
            if (!p.isPotentialHost(out string fail))
            {
                if (!fail.NullOrEmpty())
                {
                    Log.Error(string.Format("Failed to Add {0} to {1}, Reason: {2}", hediffDef.LabelCap, p.NameShortColored, fail));
                }
            }
            BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                                             where !x.def.conceptual && x.IsCorePart
                                             select x).First<BodyPartRecord>();
            if (bodyPartRecord != null)
            {
                p.health.AddHediff(hediffDef, bodyPartRecord, null, null);
                Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                HediffComp_NeoSpawner hediffcomp = hediff.TryGetComp<HediffComp_NeoSpawner>();
                Rand.PushState();
                hediffcomp.spawnCount = Rand.RangeInclusive(1,4);
                Rand.PopState();
                hediff.Severity = 0.90f;
            }
            else
            {
                Log.Error(string.Format("Failed to Add {0} to {1}, Reason: bodyPartRecord == Null", hediffDef.LabelCap, p.NameShortColored));
            }
        }
    }
}

#endif
