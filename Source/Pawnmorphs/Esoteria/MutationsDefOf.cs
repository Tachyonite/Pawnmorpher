// MutationDefOf.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 8:36 AM
// last updated 09/22/2019  8:36 AM

using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static container for commonly referenced mutations 
    /// </summary>
    [DefOf]
    public static class MutationsDefOf
    {
        static MutationsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef));
        }

        public static HediffDef EtherEggLayer; 
    }
}