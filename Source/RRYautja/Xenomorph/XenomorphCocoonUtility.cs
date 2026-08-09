using System;
using Verse;

namespace RRYautja
{
    // Token: 0x020006C2 RID: 1730
    public static class XenomorphCocoonUtility
    {
        // Token: 0x060024ED RID: 9453 RVA: 0x001199F9 File Offset: 0x00117DF9
        public static int GetSleepingSlotsCount(IntVec2 bedSize)
        {
            return bedSize.x;
        }

        // Token: 0x060024EE RID: 9454 RVA: 0x00119A04 File Offset: 0x00117E04
        public static IntVec3 GetSleepingSlotPos(int index, IntVec3 bedCenter, Rot4 bedRot, IntVec2 bedSize)
        {
            int sleepingSlotsCount = XenomorphCocoonUtility.GetSleepingSlotsCount(bedSize);
            if (index < 0 || index >= sleepingSlotsCount)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Tried to get sleeping slot pos with index ",
                    index,
                    ", but there are only ",
                    sleepingSlotsCount,
                    " sleeping slots available."
                });
                return bedCenter;
            }
            CellRect cellRect = GenAdj.OccupiedRect(bedCenter, bedRot, bedSize);
            if (bedRot == Rot4.North)
            {
                return new IntVec3(cellRect.minX + index, bedCenter.y, cellRect.maxZ);
            }
            if (bedRot == Rot4.East)
            {
                return new IntVec3(cellRect.maxX, bedCenter.y, cellRect.maxZ - index);
            }
            if (bedRot == Rot4.South)
            {
                return new IntVec3(cellRect.minX + index, bedCenter.y, cellRect.maxZ);
            }
            return new IntVec3(cellRect.maxX, bedCenter.y, cellRect.minZ - index);
        }
    }
}
