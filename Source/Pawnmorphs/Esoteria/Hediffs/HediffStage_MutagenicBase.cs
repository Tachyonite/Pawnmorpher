using System;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Abstract base class for all hediff stages that involve mutation or
    /// transformation, for use with Hediff_MutagenicBase.
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_MutagenicBase" />
    public abstract class HediffStage_MutagenicBase : HediffStage, IDescriptiveStage
    {
        //TODO
        public string DescriptionOverride => throw new NotImplementedException();
        public string LabelOverride => throw new NotImplementedException();
    }
}