using JetBrains.Annotations;
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
        /// <summary>
        /// The description.
        /// </summary>
        [UsedImplicitly] public string description;

        /// <summary>
        /// The label override.
        /// </summary>
        [UsedImplicitly] public string labelOverride;

        /// <summary>
        /// Gets the description override.
        /// </summary>
        /// <value>The description override.</value>
        public string DescriptionOverride => description;

        /// <summary>
        /// Gets the label override.
        /// </summary>
        /// <value>The label override.</value>
        public string LabelOverride => labelOverride;
    }
}