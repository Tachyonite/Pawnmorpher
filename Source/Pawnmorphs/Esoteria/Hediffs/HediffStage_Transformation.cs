using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Class for all hediff stages that full transformations.
    /// Any components defined in this class will override the equivalent component
    /// in the parent hediff
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_MutagenicBase" />
    public class HediffStage_Transformation : HediffStage_MutagenicBase
    {
        /// <summary>
        /// Controls the chance of a full transformation
        /// </summary>
        [UsedImplicitly] public TFChance tfChance;

        /// <summary>
        /// Controls what kind of animals transformations can result in
        /// </summary>
        [UsedImplicitly] public TFTypes tfTypes;

        /// <summary>
        /// Controls the gender of the post-transformation pawn
        /// </summary>
        [UsedImplicitly] public TFGenderSelector tfGenderSelector;

        /// <summary>
        /// Controls miscellaneous settings related to full transformations
        /// </summary>
        [UsedImplicitly] public TFMiscSettings tfSettings;

        /// <summary>
        /// Callbacks called on the transformed pawn to perform additional behavior
        /// </summary>
        [UsedImplicitly] public List<TFCallback> tfCallbacks;

        /// <summary>
        /// Returns a debug string displayed when inspecting hediffs in dev mode
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public string DebugString(Hediff_MutagenicBase hediff)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("--" + tfChance.GetType().Name);
            string text = tfChance.DebugString(hediff);
            if (!text.NullOrEmpty())
                stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            stringBuilder.AppendLine("--" + tfTypes.GetType().Name);
            text = tfTypes.DebugString(hediff);
            if (!text.NullOrEmpty())
                stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            stringBuilder.AppendLine("--" + tfGenderSelector.GetType().Name);
            text = tfGenderSelector.DebugString(hediff);
            if (!text.NullOrEmpty())
                stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            stringBuilder.AppendLine("--" + tfSettings.GetType().Name);
            text = tfSettings.DebugString(hediff);
            if (!text.NullOrEmpty())
                stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            //TODO
            //stringBuilder.AppendLine("--" + tfCallbacks);
            //text = tfCallbacks.DebugString(hediff);
            //if (!text.NullOrEmpty())
                //stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

            return stringBuilder.ToString();
        }
    }
}