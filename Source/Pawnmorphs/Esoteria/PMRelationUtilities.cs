// PMRelationUtilities.cs created by Iron Wolf for Pawnmorph on 03/10/2020 5:20 PM
// last updated 03/10/2020  5:20 PM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static class for relationship utilities 
    /// </summary>
    public static class PMRelationUtilities
    {
        /// <summary>
        /// Determines whether this pawn is related to a colonist pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this pawn is related to a colonist pawn; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">pawn</exception>
        public static bool IsRelatedToColonistPawn([NotNull] this Pawn pawn) {
            if (pawn is null)
            {
                throw new System.ArgumentNullException(nameof(pawn));
            }


            return pawn.relations?.DirectRelations?.Any(p => p.otherPawn?.Faction?.IsPlayer == true) == true; 
        }
    }
}