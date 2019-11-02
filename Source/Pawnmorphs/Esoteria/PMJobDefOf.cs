// PMJobDefOf.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 9:04 AM
// last updated 09/22/2019  9:04 AM

using RimWorld;
using Verse;
#pragma warning disable 1591
namespace Pawnmorph
{
    /// <summary> Static container for commonly referenced job defs. </summary>
    [DefOf]
    public static class PMJobDefOf
    {
        static PMJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDef));
        }

        public static JobDef PMLayEgg;
        public static JobDef PMMilkSelf;
        public static JobDef PMDrainChemcyst;
        public static JobDef PMShaveSelf;
        public static JobDef PMResurrect;
    }
}