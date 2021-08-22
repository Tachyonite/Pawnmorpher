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
        /// <summary>
        /// Controls the order that mutations spread over the body
        /// </summary>
        [UsedImplicitly] public MutSpreadOrder spreadOrder;

        /// <summary>
        /// Controls how fast mutations are added
        /// </summary>
        [UsedImplicitly] public MutRate mutationRate;

        /// <summary>
        /// Controls what kinds of mutations can be added
        /// </summary>
        [UsedImplicitly] public MutTypes mutationTypes;
    }
}