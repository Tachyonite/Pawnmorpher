// MutationDefOf.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 8:36 AM
// last updated 09/22/2019  8:36 AM

using RimWorld;
using Verse;
#pragma warning disable 1591 
namespace Pawnmorph
{
    /// <summary> Static container for commonly referenced mutations. </summary>
    [DefOf]
    public static class MutationsDefOf
    {
        static MutationsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MutationsDefOf));
        }

        public static HediffDef EtherEggLayer;
        public static HediffDef EtherUdder;
        public static HediffDef EtherChemfuelUdder;
        public static HediffDef EtherWooly;
        public static HediffDef EtherMegawoolly;
    }
}