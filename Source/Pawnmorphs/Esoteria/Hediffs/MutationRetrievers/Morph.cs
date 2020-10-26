// Morph.cs created by Iron Wolf for Pawnmorph on 10/24/2020 11:51 AM
// last updated 10/24/2020  11:51 AM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs.MutationRetrievers
{
    /// <summary>
    /// implementation of <see cref="IRaceMutationRetriever"/> that gets mutations by an associated animal class or group 
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.IRaceMutationRetriever" />
    public class AnimalClassRetriever : IRaceMutationRetriever
    {
        /// <summary>
        /// The animal class to get mutations from 
        /// </summary>
        public AnimalClassBase animalClass;

        /// <summary>
        /// Gets the configuration errors with this instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetConfigErrors()
        {
            if (animalClass == null) yield return $"{nameof(animalClass)} is not set!";
        }

        /// <summary>
        /// Gets all mutations that should be given to a specified race at spawn time
        /// </summary>
        /// <param name="race">The race.</param>
        /// <param name="preGeneratedPawn">The pre generated pawn, can be null if being called outside of generation</param>
        /// <returns></returns>
        public IEnumerable<MutationDef> GetMutationsFor(ThingDef race, Pawn preGeneratedPawn)
        {
            return animalClass.GetAllMutationIn();
        }
    }
}