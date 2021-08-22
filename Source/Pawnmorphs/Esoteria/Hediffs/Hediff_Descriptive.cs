using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A class for hediff with description tooltips.  Used as a base for all
    /// Pawnmorpher hediffs, but also usable by itself if you just want to add
    /// custom description tooltips to a hediff.
    /// </summary>
    public class Hediff_Descriptive : HediffWithComps, IDescriptiveHediff
    {
        /// <summary>
        /// Controls the description tooltip rendered by Pawnmorpher.
        /// </summary>
        /// <value>
        /// The tooltip description.
        /// </value>
        public virtual string Description
        {
            get
            {
                if (CurStage is IDescriptiveStage dStage && !dStage.DescriptionOverride.NullOrEmpty())
                    return dStage.DescriptionOverride;

                //TODO check this doesn't get rendered twice
                if (!def.overrideTooltip.NullOrEmpty())
                    return def.overrideTooltip;

                return def.description;
            }
        }

        /// <summary>
        /// Controls the base portion of the label (the part not in parentheses)
        /// </summary>
        /// <value>The base label.</value>
        public override string LabelBase
        {
            get
            {
                if (CurStage is IDescriptiveStage dStage && !dStage.LabelOverride.NullOrEmpty())
                    return dStage.LabelOverride;

                return base.LabelBase;
            }
        }
    }
}
