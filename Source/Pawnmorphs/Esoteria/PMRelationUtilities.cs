// PMRelationUtilities.cs created by Iron Wolf for Pawnmorph on 03/10/2020 5:20 PM
// last updated 03/10/2020  5:20 PM

using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

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

            if (pawn.relations == null) return false; 

            var allPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.MakeSafe(); 
            foreach (Pawn cPawn in allPawns)
            {
                if (cPawn == pawn) continue;
                var relation = pawn.GetMostImportantRelation(pawn);
                if (relation != null && relation != PawnRelationDefOf.Bond)
                {
                    LogMsg(LogLevel.Messages, $"{pawn.Name} is related to {cPawn.Name} by relation {relation.defName}");
                    return true; 
                }
            }

            return false;
        }
    }
}