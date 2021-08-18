using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Class for handling all hediffs that cause mutations and transformation
    /// Any components defined in this class will override the equivalent component
    /// in the parent hediff
    /// </summary>
    /// <seealso cref="Pawnmorph.IDescriptiveHediff" />
    /// <seealso cref="Verse.Hediff" />
    public class HediffStage_Mutation : HediffStage_MutagenicBase
    {
        [UsedImplicitly] private MutSpreadOrder spreadOrder;
        [UsedImplicitly] private MutRate mutationRate;
        [UsedImplicitly] private MutTypes mutationTypes;

        /// <summary>
        /// Controls the order that mutations spread over the body
        /// </summary>
        public MutSpreadOrder SpreadOrder => spreadOrder;

        /// <summary>
        /// Controls how fast mutations are added
        /// </summary>
        public MutRate MutationRate => mutationRate;

        /// <summary>
        /// Controls what kinds of mutations can be added
        /// </summary>
        public MutTypes MutationTypes => mutationTypes;
    }
}