using Pawnmorph.Hediffs.Composable;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Abstract base class for all hediffs that cause mutations and transformation
    /// </summary>
    /// <seealso cref="Pawnmorph.IDescriptiveHediff" />
    /// <seealso cref="Verse.Hediff" />
    public abstract class HediffStage_MutationBase : HediffStage_MutagenicBase
    {
        private TFTypes transformationTypes;
        private TFGenderSettings transformationGenderSettings;
        private TFMiscSettings transformationSettings;
    }
}