// AlertSettings.cs created by Iron Wolf for Pawnmorph on 09/07/2021 6:32 AM
// last updated 09/07/2021  6:32 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.Composable.Hediffs
{
	/// <summary>
	///     component that sends an alert when triggered
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.IInitializableStage" />
	public class StageAlert : IInitializableStage
	{
		private const string DEFAULT_WARNING_LABEL = "TransformationStageWarningLabel";
		private const string DEFAULT_WARNING_CONTENT = "TransformationStageWarningContent";
		private const string PAWN_ID = "PAWN";

		/// <summary>
		///     The letter label text to use will be formatted using <see cref="PAWN_ID" />
		/// </summary>
		public string letterLabelText;

		/// <summary>
		///     The letter content text to use. will be formatted using <see cref="PAWN_ID" />
		/// </summary>
		public string letterContentText;


		/// <summary>
		///     The letter definition to use, defaults to NeutralEvent
		/// </summary>
		public LetterDef letterDef;


		/// <summary>
		///     gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		///     Resolves all references in this instance.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public void ResolveReferences(HediffDef parent)
		{
			letterDef = letterDef ?? LetterDefOf.NeutralEvent;
		}


		/// <summary>
		///     Sends the alert.
		/// </summary>
		/// <param name="mBase">The m base.</param>
		public void SendAlert([NotNull] Hediff_MutagenicBase mBase)
		{
			NamedArgument pArg = mBase.pawn.Named(PAWN_ID);
			TaggedString label = string.IsNullOrEmpty(letterLabelText)
									 ? DEFAULT_WARNING_LABEL.Translate(pArg)
									 : letterLabelText.Formatted(PAWN_ID);
			TaggedString content = string.IsNullOrEmpty(letterContentText)
									   ? DEFAULT_WARNING_CONTENT.Translate(pArg)
									   : letterContentText.Formatted(PAWN_ID);
			Find.LetterStack.ReceiveLetter(label, content, letterDef ?? LetterDefOf.NeutralEvent, new LookTargets(mBase.pawn));
		}
	}
}