using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RRYautja.ExtensionMethods
{

    public static class HiveExtensions
    {
        /*
        public static HiveGrid hiveGrid(this Map map)
        {
            MapComponent_AvPHiveGrid _AvPHiveCreep = map.GetComponent<MapComponent_AvPHiveGrid>();
            if (_AvPHiveCreep == null)
            {
                _AvPHiveCreep = new MapComponent_AvPHiveGrid(map);
                map.components.Add(_AvPHiveCreep);
                if (_AvPHiveCreep.hiveGrid==null)
                {
                    _AvPHiveCreep.hiveGrid = new HiveGrid(map);
                }
            }
            return _AvPHiveCreep.hiveGrid;
        }
        */


    }

    // Token: 0x02000C94 RID: 3220
    // Custom MapMeshFlagDef references for Hive and Goo.
    // In RimWorld 1.5+, MapMeshFlag became a MapMeshFlagDef Def class.
    // These are resolved at runtime from XML defs named RRY_Hive and RRY_Goo.
    public static class RRYMapMeshFlagDefOf
    {
        public static MapMeshFlagDef Hive;
        public static MapMeshFlagDef Goo;

        static RRYMapMeshFlagDefOf()
        {
            Hive = DefDatabase<MapMeshFlagDef>.GetNamed("RRY_Hive", false);
            Goo = DefDatabase<MapMeshFlagDef>.GetNamed("RRY_Goo", false);
        }
    }

    // Token: 0x02000FC0 RID: 4032
    [StaticConstructorOnStartup]
    public static class RRYMatBases
    {
        // Token: 0x040040B4 RID: 16564
        public static readonly Material LightOverlay = MatLoader.LoadMat("Lighting/LightOverlay", -1);

        // Token: 0x040040B5 RID: 16565
        public static readonly Material SunShadow = MatLoader.LoadMat("Lighting/SunShadow", -1);

        // Token: 0x040040B6 RID: 16566
        public static readonly Material SunShadowFade = MatBases.SunShadow;

        // Token: 0x040040B7 RID: 16567
        public static readonly Material EdgeShadow = MatLoader.LoadMat("Lighting/EdgeShadow", -1);

        // Token: 0x040040B8 RID: 16568
        public static readonly Material IndoorMask = MatLoader.LoadMat("Misc/IndoorMask", -1);

        // Token: 0x040040B9 RID: 16569
        public static readonly Material Hive = MatLoader.LoadMat("Misc/Snow", -1);

        // Token: 0x040040BA RID: 16570
        public static readonly Material Snow = MatLoader.LoadMat("Misc/FogOfWar", -1);
    }
}
