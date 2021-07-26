// PMIssueDefOf.cs created by Iron Wolf for Pawnmorph on 07/25/2021 5:37 PM
// last updated 07/25/2021  5:37 PM

using JetBrains.Annotations;
using RimWorld;

namespace Pawnmorph
{
    /// <summary>
    /// static class containing commonly used issue defs 
    /// </summary>
    [DefOf]
    public static class PMIssueDefOf
    {
        /// <summary>
        /// The pm sapience loss issue 
        /// </summary>
        [NotNull]
        public static IssueDef PM_SapienceLoss;

        static PMIssueDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PMIssueDefOf));
        }
    }
}