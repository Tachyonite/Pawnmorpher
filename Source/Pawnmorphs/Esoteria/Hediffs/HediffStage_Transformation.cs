using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Class for all hediff stages that full transformations.
    /// Any components defined in this class will override the equivalent component
    /// in the parent hediff
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_MutagenicBase" />
    public class HediffStage_Transformation : HediffStage_MutagenicBase
    {
        [UsedImplicitly] private TFChance tfChance;
        [UsedImplicitly] private TFTypes tfTypes;
        [UsedImplicitly] private TFGenderSelector tfGenderSelector;
        [UsedImplicitly] private TFMiscSettings tfSettings;
        [UsedImplicitly] private List<TFCallback> tfCallbacks;

        /// <summary>
        /// Controls the chance of a full transformation
        /// </summary>
        public TFChance TFChance => tfChance;

        /// <summary>
        /// Controls what kind of animals transformations can result in
        /// </summary>
        public TFTypes TFTypes => tfTypes;

        /// <summary>
        /// Controls the gender of the post-transformation pawn
        /// </summary>
        public TFGenderSelector TFGenderSelector => tfGenderSelector;

        /// <summary>
        /// Controls miscellaneous settings related to full transformations
        /// </summary>
        public TFMiscSettings TFMiscSettings => tfSettings;

        /// <summary>
        /// Callbacks called on the transformed pawn to perform additional behavior
        /// </summary>
        public List<TFCallback> TFCallbacks => tfCallbacks;
    }
}