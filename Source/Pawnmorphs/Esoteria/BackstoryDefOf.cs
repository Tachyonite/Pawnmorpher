// BackstoryDefOf.cs modified by Iron Wolf for Pawnmorph on 11/29/2019 7:35 AM
// last updated 11/29/2019  7:35 AM

using AlienRace;
using JetBrains.Annotations;
using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
    [DefOf]
    public static class BackstoryDefOf
    {
        [NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public static BackstoryDef FormerHumanNormal;

        [NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public static BackstoryDef FormerHumanChaomorph;

        static BackstoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BackstoryDef));
        }
    }
}