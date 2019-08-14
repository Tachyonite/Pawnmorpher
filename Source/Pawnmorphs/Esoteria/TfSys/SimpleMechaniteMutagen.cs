// SimpleMechaniteMutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 2:58 PM
// last updated 08/14/2019  2:59 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    /// simple implementation of Mutagen that just transforms a single pawn into a single animal 
    /// </summary>
    /// <seealso cref="Pawnmorph.TfSys.Mutagen{Pawnmorph.TfSys.TransformedPawnSingle}" />
    public class SimpleMechaniteMutagen : Mutagen<TransformedPawnSingle>
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
        protected override TransformedPawnSingle TransformPawnsImpl(IEnumerable<Pawn> originals, ThingDef outputRace, TFGender forcedGender,
            float forcedGenderChance, Hediff_Morph cause)
        {
            throw new System.NotImplementedException(); //TODO move the code from TransformerUtilities.TransformPawn here 
        }

        /// <summary>
        /// Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected override bool TryRevertImpl(TransformedPawnSingle transformedPawn)
        {
            throw new System.NotImplementedException(); //TODO move the code for single reversion in Reverter serum here 
        }
    }
}