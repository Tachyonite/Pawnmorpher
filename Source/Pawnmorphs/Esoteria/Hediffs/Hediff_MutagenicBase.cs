using Pawnmorph.Hediffs.Composable;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Abstract base class for all hediffs that cause mutations and transformation
    /// </summary>
    /// <seealso cref="Pawnmorph.IDescriptiveHediff" />
    /// <seealso cref="Verse.Hediff" />
    public abstract class Hediff_MutagenicBase : Hediff_Descriptive
    {
        private MutSpreadOrder spreadOrder;
        private MutRate mutationRate;
        private MutTypes mutationTypes;
        private TFTypes transformationTypes;
        private TFGenderSettings transformationGenderSettings;
        private TFMiscSettings transformationSettings;
    }
}