// TfHediffDefOf.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 7:40 PM
// last updated 08/13/2019  7:41 PM

using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary> Collection of misc tf related HediffDefs. </summary>
    [DefOf]
    public static class TfHediffDefOf
    {
        static TfHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef));
        }

        public static HediffDef TransformedHuman;
        public static HediffDef EtherBroken;
        public static HediffDef EtherBond;
    }
}