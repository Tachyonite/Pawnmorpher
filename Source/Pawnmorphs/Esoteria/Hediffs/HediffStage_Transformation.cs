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
        [UsedImplicitly] private TFTypes transformationTypes;
        [UsedImplicitly] private TFGenderSettings transformationGenderSettings;
        [UsedImplicitly] private TFMiscSettings transformationSettings;

        public TFTypes TFTypes => transformationTypes;
        public TFGenderSettings TFGenderSettings => transformationGenderSettings;
        public TFMiscSettings TFMiscSettings => transformationSettings;
    }
}