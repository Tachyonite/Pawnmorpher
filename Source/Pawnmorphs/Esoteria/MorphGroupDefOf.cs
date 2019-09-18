// MorphGroupDefOf.cs modified by Iron Wolf for Pawnmorph on 09/09/2019 7:33 PM
// last updated 09/09/2019  7:33 PM

using RimWorld;

namespace Pawnmorph
{
    [DefOf]
    public static class MorphGroupDefOf
    {
        static MorphGroupDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MorphGroupDef));
        }

        public static MorphGroupDef Humans;

    }
}