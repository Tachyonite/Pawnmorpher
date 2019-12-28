// SimpleDescriptiveHediff.cs modified by Iron Wolf for Pawnmorph on 12/21/2019 8:03 PM
// last updated 12/21/2019  8:03 PM

using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// simple implementation of the <see cref="IDescriptiveHediff"/> interface 
    /// </summary>
    /// <seealso cref="Verse.HediffWithComps" />
    /// <seealso cref="Pawnmorph.IDescriptiveHediff" />
    public class SimpleDescriptiveHediff : HediffWithComps, IDescriptiveHediff
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                if (CurStage is IDescriptiveStage dStage)
                {
                    return string.IsNullOrEmpty(dStage.DescriptionOverride) ? def.description : dStage.DescriptionOverride; 
                }

                return def.description; 
            }
        }

        /// <summary>
        /// Gets the label base.
        /// </summary>
        /// <value>
        /// The label base.
        /// </value>
        public override string LabelBase
        {
            get
            {
                if (CurStage is IDescriptiveStage ds)
                {
                    return string.IsNullOrEmpty(ds.LabelOverride) ? base.LabelBase : ds.LabelOverride; 
                }

                return base.LabelBase;
            }
        }
    }
}