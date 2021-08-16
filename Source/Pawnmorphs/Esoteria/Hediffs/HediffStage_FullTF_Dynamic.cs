using System;
using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Full transformation stage that causes a transformation into an animal species decided by the hediff
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_DynamicTf" />
    /// <seealso cref="Pawnmorph.Hediffs.FullTransformationStageBase" />
    public class HediffStage_FullTF_Dynamic : FullTransformationStageBase
    {
        /// <summary>
        /// Gets the pawn kind definition to turn the given pawn into
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        protected override PawnKindDef GetPawnKindDefFor(Pawn pawn)
        {
            throw new NotImplementedException();
        }
    }
}