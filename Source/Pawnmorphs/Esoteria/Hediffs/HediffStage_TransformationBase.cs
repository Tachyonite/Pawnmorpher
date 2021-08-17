using Pawnmorph.Hediffs.Composable;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Abstract base class for all hediff stages that full transformations.
    /// Any components defined in this class will override the equivalent component
    /// in the parent hediff
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_MutagenicBase" />
    public abstract class HediffStage_TransformationBase : HediffStage_MutagenicBase
    {
        private MutSpreadOrder spreadOrder;
        private MutRate mutationRate;
        private MutTypes mutationTypes;
    }
}