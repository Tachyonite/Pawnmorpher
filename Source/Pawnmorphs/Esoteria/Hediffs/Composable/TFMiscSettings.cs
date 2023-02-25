
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// A class that determines misc settings regarding the transformation
	/// 
	/// NOTE This can be broken up into its components if extended logic seems useful
	/// </summary>
	public class TFMiscSettings
	{
		/// <summary>
		/// The settings that define the chance of going manhunter
		/// </summary>
		[UsedImplicitly] public ManhunterTfSettings manhunterSettings = ManhunterTfSettings.Default;

		/// <summary>
		/// The tale to use for the transformation
		/// </summary>
		[UsedImplicitly] public TaleDef tfTale;

		/// <summary>
		/// if a warning is to be displayed during the previous stage 
		/// </summary>
		[UsedImplicitly] public bool displayWarning;

		/// <summary>
		/// The warning label override
		/// </summary>
		[UsedImplicitly] public string warningLabelOverride;
		/// <summary>
		/// The warning content override
		/// </summary>
		[UsedImplicitly] public string warningContentOverride;

		/// <summary>
		/// Forces the sapience to a specific value if present
		/// </summary>
		[UsedImplicitly] public float? forcedSapience;

		/// <summary>
		/// The warning letter definition
		/// </summary>
		[UsedImplicitly, CanBeNull] public LetterDef warningLetterDef;

		/// <summary>
		/// The settings that define the chance of going manhunter
		/// </summary>
		public virtual ManhunterTfSettings ManhunterSettings => manhunterSettings;

		/// <summary>
		/// The tale to use for the transformation
		/// </summary>
		public virtual TaleDef TfTale => tfTale;

		/// <summary>
		/// Forces the sapience to a specific value if present
		/// </summary>
		public virtual float? ForcedSapience => forcedSapience;

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public virtual string DebugString(Hediff_MutagenicBase hediff) => "";

		const string DEFAULT_WARNING_LABEL = "TransformationStageWarningLabel";
		const string DEFAULT_WARNING_CONTENT = "TransformationStageWarningContent";

		/// <summary>
		/// Tries to display the warning message.
		/// </summary>
		/// <param name="mBase">The m base.</param>
		public void TryDisplayWarning([NotNull] Hediff_MutagenicBase mBase)
		{
			if (!displayWarning) return;

			NamedArgument namedArgument = mBase.pawn.Named("PAWN");
			var wLabel = warningLabelOverride.NullOrEmpty() ? DEFAULT_WARNING_LABEL.Translate(namedArgument) : warningLabelOverride.Formatted(namedArgument);
			var wContent = warningContentOverride.NullOrEmpty()
							   ? DEFAULT_WARNING_CONTENT.Translate(namedArgument)
							   : warningContentOverride.Formatted(namedArgument);
			Find.LetterStack.ReceiveLetter(wLabel, wContent, warningLetterDef ?? LetterDefOf.NeutralEvent, new LookTargets(mBase.pawn));


		}
	}
}
