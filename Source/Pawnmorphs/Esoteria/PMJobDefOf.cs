// PMJobDefOf.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 9:04 AM
// last updated 09/22/2019  9:04 AM

using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static container for commonly referenced job defs 
    /// </summary>
    [DefOf]
    public static class PMJobDefOf
    {
        static PMJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDef));
        }

        public static JobDef PMLayEgg;
    }
}