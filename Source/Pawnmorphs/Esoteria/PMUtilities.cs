// PMUtilities.cs created by Iron Wolf for Pawnmorph on 09/15/2019 7:38 PM
// last updated 09/15/2019  7:38 PM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// a collection of general Pawnmorpher related utilities 
    /// </summary>
    public static class PMUtilities
    {

        /// <summary>
        /// Gets a value indicating whether mutagenic diseases are enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [mutagenic diseases enabled]; otherwise, <c>false</c>.
        /// </value>
        public static bool MutagenicDiseasesEnabled => GetSettings()?.enableMutagenDiseases == true; 

        /// <summary>Gets the mod settings.</summary>
        /// <returns></returns>
        [NotNull]
        public static PawnmorpherSettings GetSettings()
        {
            return LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();
        }


        /// <summary>
        /// Determines whether this pawn is loading or spawning.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this pawn is loading or spawning ; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLoadingOrSpawning([NotNull] this Pawn pawn)
        {
            if (PawnGenerator.IsBeingGenerated(pawn)) return true; 
            if (pawn.health == null || pawn.mindState?.inspirationHandler == null || pawn.needs == null) return true;
            return false; 
        }
    }
}