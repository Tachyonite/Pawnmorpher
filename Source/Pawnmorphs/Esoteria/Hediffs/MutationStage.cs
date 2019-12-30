// DescriptiveStage.cs modified by Iron Wolf for Pawnmorph on 12/14/2019 7:32 PM
// last updated 12/14/2019  7:32 PM

using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff stage with an extra description field  
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    public class MutationStage : HediffStage, IDescriptiveStage
    {
        /// <summary>
        /// optional description override for a hediff in this stage 
        /// </summary>
        public string description;

        /// <summary>
        /// The label override
        /// </summary>
        public string labelOverride;

        /// <summary>
        /// the base chance that the mutation will stop progressing at this stage  
        /// </summary>
        /// this should be in [0,1]
        public float stopChance;

        string IDescriptiveStage.DescriptionOverride => description;
        string IDescriptiveStage.LabelOverride => labelOverride;
    }
}