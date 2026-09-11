using RRYautja;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // Token: 0x0200002D RID: 45
    public class JobDriver_LayXenoEgg : JobDriver
    {
        // Token: 0x060001B3 RID: 435 RVA: 0x00010CE0 File Offset: 0x0000F0E0
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        // Token: 0x060001B4 RID: 436 RVA: 0x00010CE4 File Offset: 0x0000F0E4
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_General.WaitWith(TargetIndex.A, LayEgg, true);
            yield return Toils_General.Do(delegate
            {
                var eggLayer = this.pawn.GetComp<CompXenoEggLayer>();
                if (eggLayer == null) return;
                var eggs = eggLayer.ProduceEggs();
                Map map = base.Map;
                if (map == null) return;
                foreach (var egg in eggs)
                {
                    IntVec3 spawnLoc = this.pawn.Position;
                    if (eggs.Count > 1)
                    {
                        spawnLoc = CellFinder.RandomClosewalkCellNear(this.pawn.Position, map, 2);
                    }
                    GenSpawn.Spawn(egg, spawnLoc, map, WipeMode.Vanish);
                }
            });
            yield break;
        }

        // Token: 0x040001AE RID: 430
        private const int LayEgg = 500;

        // Token: 0x040001AF RID: 431
        private const TargetIndex LaySpotInd = TargetIndex.A;
    }
}
