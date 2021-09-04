using System;
using RimWorld;
using Verse;

namespace Pawnmorph.Letters
{
    [DefOf]
    public static class PMLetterDefOf
    {
        public static LetterDef PMFormerHumanJoinRequest;

        static PMLetterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PMLetterDefOf));
        }
    }
}
