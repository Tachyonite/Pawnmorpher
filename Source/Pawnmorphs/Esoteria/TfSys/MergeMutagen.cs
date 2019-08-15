// MergeMutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 3:15 PM
// last updated 08/14/2019  3:15 PM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    /// implementation of mutagen that merges 2 or more pawns into a single meld 
    /// </summary>
    /// <seealso cref="Pawnmorph.TfSys.Mutagen{Pawnmorph.TfSys.MergedPawns}" />
    public class MergeMutagen : Mutagen<MergedPawns>
    {
        private const string FORMER_HUMAN_HEDIFF = "2xMergedHuman";//can't put this in a hediffDefOf because of the name 


        /// <summary>
        /// Transforms the pawns into a TransformedPawn instance of the given ace .
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="outputPawnKind"></param>
        /// <param name="forcedGender"></param>
        /// <param name="forcedGenderChance"></param>
        /// <param name="cause">The cause.</param>
        /// <param name="tale"></param>
        /// <returns></returns>
        protected override MergedPawns TransformPawnsImpl(IEnumerable<Pawn> originals, PawnKindDef outputPawnKind,
            TFGender forcedGender,
            float forcedGenderChance, Hediff_Morph cause, TaleDef tale)
        {
            throw new System.NotImplementedException(); //TODO move the code from mutagen chamber related to merging here 
        }

        /// <summary>
        /// Determines whether this instance can revert pawn the specified transformed pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can revert pawn  the specified transformed pawn; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanRevertPawnImp(MergedPawns transformedPawn)
        {
            if (!transformedPawn.IsValid) return false;

            var def = HediffDef.Named(FORMER_HUMAN_HEDIFF);

            return transformedPawn.meld.health.hediffSet.HasHediff(def); 

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