// Mutations.cs created by Iron Wolf for Pawnmorph on 10/24/2020 7:07 PM
// last updated 10/24/2020  7:07 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs.MutationRetrievers
{
    /// <summary>
    /// implementation of <see cref="IRaceMutationRetriever"/> that gives a race a set of specific mutations 
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.IRaceMutationRetriever" />
    public class Mutations : IRaceMutationRetriever
    {
        /// <summary>
        /// The mutations to give the race 
        /// </summary>
        public List<MutationDef> mutations;


        /// <summary>
        /// Gets the configuration errors with this instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetConfigErrors()
        {
            if (mutations == null) yield return $"{nameof(mutations)} is not set!";
            else if (mutations.Count == 0) yield return $"{nameof(mutations)} is empty!";
        }

        /// <summary>
        /// Gets all mutations that should be given to a specified race at spawn time
        /// </summary>
        /// <param name="race">The race.</param>
        /// <param name="preGeneratedPawn">The pre generated pawn, can be null if being called outside of generation</param>
        /// <returns></returns>
        public IEnumerable<MutationDef> GetMutationsFor(ThingDef race, Pawn preGeneratedPawn)
        {
            return mutations; 
        }
    }
}