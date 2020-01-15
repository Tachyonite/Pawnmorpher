// TransformationStageBase.cs modified by Iron Wolf for Pawnmorph on //2020 
// last updated 01/12/2020  2:04 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// base class for all trans-formative hediff stages 
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IExecutableStage" />
    public abstract class TransformationStageBase : HediffStage, IDescriptiveStage, IExecutableStage, IInitializable
    {
        /// <summary>
        /// Gets all mutation entries in this stage 
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        [NotNull]
        public abstract IEnumerable<MutationEntry> Entries { get; }

        /// <summary>
        /// returns all configuration errors in this stage
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public virtual IEnumerable<string> ConfigErrors()
        {
            yield break;
        }


        /// <summary>The description</summary>
        public string description;

        /// <summary>The label override</summary>
        public string labelOverride;

        /// <summary>The letter text</summary>
        public string letterText;

        /// <summary>The letter label</summary>
        public string letterLabel;


        /// <summary>
        /// the expected number of mutations a pawn would get per day at this stage 
        /// </summary>
        /// note, this is affected by MutagenSensitivity stat 
        public float meanMutationsPerDay=4; 


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
    }
}