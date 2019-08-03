// PostTfHediffComp.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 9:05 AM
// last updated 07/30/2019  9:05 AM

using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// interface for any hediff component that has a special behavior when the transformation hediff ends naturally 
    /// </summary>
    public interface IPostTfHediffComp
    {
        /// <summary>
        /// called when the morph hediff ends naturally (after reaching 0 or below severity) 
        /// </summary>
        void FinishedTransformation(Pawn pawn, Hediff_Morph hediff); 
    }
}