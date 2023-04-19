// TransformationRequest.cs modified by Iron Wolf for Pawnmorph on 08/18/2019 8:55 AM
// last updated 08/18/2019  8:55 AM

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
	/// <summary>
	/// class representing the request to transform pawns 
	/// </summary>
	public class TransformationRequest
	{
		/// <summary>
		/// Returns true if this instance is valid.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
		/// </value>
		public bool IsValid => originals != null && originals.Length > 0 && outputDef != null;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformationRequest"/> struct.
		/// </summary>
		/// <param name="outputDef">The output definition.</param>
		/// <param name="original">The original.</param>
		/// <param name="maxSeverity">the maximum severity of the former human hediff</param>
		public TransformationRequest(PawnKindDef outputDef, Pawn original, float maxSeverity = 1)
		{
			originals = new[] { original };
			this.outputDef = outputDef;
			forcedGender = TFGender.Original;
			forcedGenderChance = 50;
			cause = null;
			tale = null;
			this.maxSeverity = maxSeverity;
			addMutationToOriginal = true;
			noLetter = false;
			minSeverity = 0;
			forcedFaction = default;
			factionResponsible = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformationRequest"/> struct.
		/// </summary>
		/// <param name="outputDef">The output definition.</param>
		/// <param name="original">The original.</param>
		/// <param name="maxSeverity">the maximum severity of the former human hediff</param>
		public TransformationRequest(PawnKindDef outputDef, Pawn original, SapienceLevel maxSeverity)
		{

			originals = new[] { original };
			this.outputDef = outputDef;
			forcedGender = TFGender.Original;
			forcedGenderChance = 50;
			cause = null;
			tale = null;
			this.maxSeverity = maxSeverity.GetMidLevel();
			addMutationToOriginal = true;
			noLetter = false;
			minSeverity = 0;
			forcedFaction = default;
			factionResponsible = null;
			sendTransformationEvent = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformationRequest"/> struct.
		/// </summary>
		/// <param name="outputDef">The output definition.</param>
		/// <param name="originals">The originals.</param>
		public TransformationRequest(PawnKindDef outputDef, params Pawn[] originals)
		{
			this.originals = originals;
			this.outputDef = outputDef;
			forcedGender = TFGender.Original;
			forcedGenderChance = 50;
			cause = null;
			tale = null;
			addMutationToOriginal = true;
			noLetter = false;
			this.maxSeverity = 1;
			minSeverity = 0;
			forcedFaction = default;
			factionResponsible = null;
			sendTransformationEvent = true;
		}

		/// <summary>
		/// the tick this transformation is taking place, a null value indicates it happened some unknown amount of time in the past
		/// </summary>
		public int? transformedTick;

		/// <summary>The pawns to be transformed</summary>
		public Pawn[] originals;
		/// <summary>The output pawn kind</summary>
		public PawnKindDef outputDef;
		/// <summary>The forced gender option</summary>
		public TFGender forcedGender;
		/// <summary>
		/// if forcedGender is None, the chance to switch genders 
		/// </summary>
		public float forcedGenderChance;
		/// <summary>The cause of the transformation</summary>
		public Hediff cause;
		/// <summary>The tale to record</summary>
		public TaleDef tale;

		/// <summary>
		/// if true, send transformation event
		/// </summary>
		public bool sendTransformationEvent = true;

		/// <summary>
		/// optional backstory override
		/// </summary>
		public BackstoryDef backstoryOverride;

		/// <summary>
		/// override for the manhunter settings of the given animal 
		/// </summary>
		public ManhunterTfSettings? manhunterSettingsOverride;

		/// <summary>
		/// if not null then this represents the sapience level the tf'd pawn will have
		/// </summary>
		public float? forcedSapienceLevel;

		/// <summary>
		/// if true, no notification will be sent about the transformation
		/// </summary>
		public bool noLetter;

		/// <summary>
		/// if true add mutation to original
		/// </summary>
		public bool addMutationToOriginal;

		/// <summary>
		/// The faction responsible for this transformation 
		/// </summary>
		[CanBeNull]
		public Faction factionResponsible;

		/// <summary>
		/// the faction to put the resultant pawn into 
		/// </summary>
		public Faction forcedFaction;

		/// <summary>
		/// The minimum severity of the former human hediff
		/// </summary>
		public float minSeverity;

		/// <summary>
		/// The minimum severity of the former human hediff
		/// </summary>
		public float maxSeverity;

		[NotNull]
		private static readonly StringBuilder _builder = new StringBuilder();

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			_builder.Clear();
			_builder.AppendLine($"pawns:[{string.Join(",", originals.MakeSafe().Select(p => p?.Name?.ToStringFull ?? "Null"))}]");
			_builder.AppendLine($"output def:{outputDef?.defName ?? "NULL"}");
			_builder.AppendLine($"cause:{cause?.def?.defName ?? "NULL"}");
			return _builder.ToString();


		}
	}
}