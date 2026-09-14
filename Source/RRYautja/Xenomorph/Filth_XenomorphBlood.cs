using RimWorld;
using UnityEngine;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Xenomorph blood filth with 16 texture variants.
    /// Variant is chosen by thingIDNumber % 16 — stable across save/load
    /// because thingIDNumber is persisted with the Thing.
    /// Texture files: Things/Filth/XenomorphBlood_a.png ... _p.png
    /// </summary>
    public class Filth_XenomorphBlood : Filth
    {
        private static Graphic[] variants;
        private static readonly string[] Suffixes =
            { "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p" };

        private static Graphic[] Variants
        {
            get
            {
                if (variants == null)
                {
                    var list = new System.Collections.Generic.List<Graphic>();
                    foreach (var s in Suffixes)
                    {
                        string path = $"Things/Filth/XenomorphBlood_{s}";
                        if (ContentFinder<Texture2D>.Get(path, false) != null)
                        {
                            list.Add(GraphicDatabase.Get<Graphic_Single>(
                                path, ShaderDatabase.Transparent, Vector2.one, Color.white));
                        }
                    }
                    variants = list.ToArray();
                    Log.Message($"[AVP Xenomorphs] Filth_XenomorphBlood: {variants.Length} texture variants loaded");
                }
                return variants;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                Graphic[] pool = Variants;
                if (pool.Length == 0) return base.Graphic;
                return pool[Mathf.Abs(thingIDNumber) % pool.Length];
            }
        }
    }
}