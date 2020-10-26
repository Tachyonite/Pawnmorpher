// IRaceMutationRetriever.cs created by Iron Wolf for Pawnmorph on 10/24/2020 11:48 AM
// last updated 10/24/2020  11:48 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// interface for a type that retrieves mutations for a specific race at generation
    /// used for alien race compatibility 
    /// </summary>
    public interface IRaceMutationRetriever
    {
        /// <summary>
        /// Gets the configuration errors with this instance.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        IEnumerable<string> GetConfigErrors();

        /// <summary>
        /// Gets all mutations that should be given to a specified race at spawn time
        /// </summary>
        /// <param name="race">The race.</param>
        /// <param name="preGeneratedPawn">The pre generated pawn, can be null if being called outside of generation</param>
        /// <returns></returns>
        [NotNull]
        IEnumerable<MutationDef> GetMutationsFor([NotNull] ThingDef race, [CanBeNull] Pawn preGeneratedPawn);
    }
}