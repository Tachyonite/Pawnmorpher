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
                if (CurStage is DescriptiveStage dStage)
                {
                    return string.IsNullOrEmpty(dStage.description) ? def.description : dStage.description; 
                }

                return def.description; 
            }
        }
    }
}