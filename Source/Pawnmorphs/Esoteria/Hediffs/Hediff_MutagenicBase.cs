using Pawnmorph.Hediffs.Composable;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Abstract base class for all hediffs that cause mutations and transformation
    /// </summary>
    /// <seealso cref="Pawnmorph.IDescriptiveHediff" />
    /// <seealso cref="Verse.Hediff" />
    public abstract class Hediff_MutagenicBase : HediffWithComps, IDescriptiveHediff
    {
        private MutSpreadOrder spreadOrder;
        private MutRate mutationRate;
        private MutTypes mutationTypes;
        private TFTypes transformationTypes;
        private TFGenderSettings transformationGenderSettings;
        private TFMiscSettings transformationSettings;

        /// <summary>
        /// Gets the description of this hediff, used for the tooltip.
        /// </summary>
        /// <value>The description.</value>
        public virtual string Description
        {
            get
            {
                if (CurStage is IDescriptiveStage dStage && !dStage.DescriptionOverride.NullOrEmpty())
                    return dStage.DescriptionOverride;

                //TODO check this
                if (!def.overrideTooltip.NullOrEmpty())
                    return def.overrideTooltip;

                return def.description;
            }
        }
    }
}