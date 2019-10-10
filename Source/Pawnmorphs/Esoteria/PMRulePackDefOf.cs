// PMRulePackDefOf.cs created by Iron Wolf for Pawnmorph on 10/09/2019 2:02 PM
// last updated 10/09/2019  2:02 PM

using RimWorld;
using Verse;

namespace Pawnmorph
{
    [DefOf]
    public static class PMRulePackDefOf
    {
        static PMRulePackDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RulePackDef));
        }

        /// <summary> Default rule pack used for generating mutation log entries. </summary>
        public static RulePackDef DefaultMutationRulePack;
    }
}