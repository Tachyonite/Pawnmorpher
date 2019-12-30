// MorphDisease.cs modified by Iron Wolf for Pawnmorph on 11/24/2019 3:43 PM
// last updated 11/24/2019  3:43 PM

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff for morph diseases 
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediff_Morph" />
    public class MorphDisease : Hediff_Morph
    {
        /// <summary>the stage to display a warning message about the pawn fully transforming.</summary>
        /// <value>The transformation warning stage.</value>
        protected override int TransformationWarningStage => def.stages.Count - 2; 
    }
}