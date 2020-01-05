// TransformationStage.cs created by Iron Wolf for Pawnmorph on 01/02/2020 1:44 PM
// last updated 01/02/2020  1:44 PM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     hediff stage that adds the possibility of adding mutations
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
    public class TransformationStage : HediffStage, IDescriptiveStage, IExecutableStage
    {
        /// <summary>The mutations that this stage can add</summary>
        public List<MutationEntry> mutations;

        /// <summary>The description</summary>
        public string description;

        /// <summary>The label override</summary>
        public string labelOverride;

        /// <summary>The letter text</summary>
        public string letterText;

        /// <summary>The letter label</summary>
        public string letterLabel;
        
        /// <summary>
        ///     Gets the description override.
        /// </summary>
        /// <value>
        ///     The description override.
        /// </value>
        string IDescriptiveStage.DescriptionOverride => description;

        /// <summary>
        ///     Gets the label override.
        /// </summary>
        /// <value>
        ///     The label override.
        /// </value>
        string IDescriptiveStage.LabelOverride => labelOverride;

        /// <summary>called when the given hediff enters this stage</summary>
        /// <param name="hediff">The hediff.</param>
        void IExecutableStage.EnteredStage(Hediff hediff)
        {
            Pawn pawn = hediff.pawn;
            if (string.IsNullOrEmpty(this.letterLabel) || string.IsNullOrEmpty(letterText))
            {
                Log.Warning($"{hediff.def.defName} tried to execute {nameof(TransformationStage)} but {nameof(this.letterLabel)} or {nameof(letterText)} are not set!");
                return;
            }

            if (!PawnUtility.ShouldSendNotificationAbout(pawn)) return;
            string letterLabel = this.letterLabel.AdjustedFor(pawn);
            string letterContent = letterText.AdjustedFor(pawn);
            Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent, new LookTargets(pawn));
        }
    }
}