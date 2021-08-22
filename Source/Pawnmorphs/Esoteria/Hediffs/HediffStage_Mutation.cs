using System;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;
using Verse;

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

        /// <summary>
        /// Returns a debug string displayed when inspecting hediffs in dev mode
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public string DebugString(Hediff_MutagenicBase hediff)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("--" + spreadOrder);
            string text = spreadOrder.DebugString(hediff);
            if (!text.NullOrEmpty())
                stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            stringBuilder.AppendLine("--" + mutationRate);
            text = mutationRate.DebugString(hediff);
            if (!text.NullOrEmpty())
                stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            stringBuilder.AppendLine("--" + mutationTypes);
            text = mutationTypes.DebugString(hediff);
            if (!text.NullOrEmpty())
                stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            return stringBuilder.ToString();
        }
    }
}