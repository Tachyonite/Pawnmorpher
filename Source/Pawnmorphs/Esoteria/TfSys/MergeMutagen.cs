// MergeMutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 3:15 PM
// last updated 08/14/2019  3:15 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    /// implementation of mutagen that merges 2 or more pawns into a single meld 
    /// </summary>
    /// <seealso cref="Pawnmorph.TfSys.Mutagen{Pawnmorph.TfSys.MergedPawns}" />
    public class MergeMutagen : Mutagen<MergedPawns>
    {
        /// <summary>
        /// Transforms the pawns into a TransformedPawn instance of the given ace .
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="outputRace">The output race.</param>
        /// <param name="forcedGenderChance"></param>
        /// <param name="cause">The cause.</param>
        /// <param name="forcedGender"></param>
        /// <returns></returns>
        protected override MergedPawns TransformPawnsImpl(IEnumerable<Pawn> originals, ThingDef outputRace, TFGender forcedGender,
            float forcedGenderChance, Hediff_Morph cause)
        {
            throw new System.NotImplementedException(); //TODO move the code from mutagen chamber related to merging here 
        }

        /// <summary>
        /// Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected override bool TryRevertImpl(MergedPawns transformedPawn)
        {
            throw new System.NotImplementedException(); //TODO move the code in the reverter serum related to merges here 
        }
    }
}