using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// A class that determines what the chance of a full transformation is
	/// </summary>
	public abstract class TFChance : IInitializableStage
	{
		/// <summary>
		/// Whether or not to transform the pawn.  Checked only upon entering a stage.
		/// </summary>
		/// <param name="hediff">The hediff doing the transformation.</param>
		public abstract bool ShouldTransform(Hediff_MutagenicBase hediff);

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public virtual string DebugString(Hediff_MutagenicBase hediff) => "";

		/// <summary>
		/// gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public virtual IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Resolves all references in this instance.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public virtual void ResolveReferences(HediffDef parent)
		{
			//empty 
		}
	}

	/// <summary>
	/// A simple TFChance class that just always transforms the pawn
	/// </summary>
	public class TFChance_Always : TFChance
	{
		/// <summary>
		/// Whether or not to transform the pawn.  Checked only upon entering a stage.
		/// </summary>
		/// <param name="hediff">The hediff doing the transformation.</param>
		public override bool ShouldTransform(Hediff_MutagenicBase hediff) => true;
	}

	/// <summary>
	/// A TFChance class that transforms the pawn with a random chance specified in the XML
	/// Also affected by the TransformationSensitivity stat, unless disabled
	/// </summary>
	public class TFChance_Random : TFChance
	{
		/// <summary>
		/// The chance of a transformation.
		/// </summary>
		[UsedImplicitly] public float chance;

		/// <summary>
		/// Whether or not transformation sensitivity is respected.
		/// If true, the chance will be multiplied by the sensitivity stat
		/// </summary>
		[UsedImplicitly] public bool affectedBySensitivity = true;

		/// <summary>
		/// Whether or not to transform the pawn.  Checked only upon entering a stage.
		/// </summary>
		/// <param name="hediff">The hediff doing the transformation.</param>
		public override bool ShouldTransform(Hediff_MutagenicBase hediff)
		{
			float tfChance = chance;

			if (affectedBySensitivity)
				tfChance *= hediff.TransformationSensitivity;

			tfChance = Mathf.Clamp(tfChance, 0f, 1f);
			return Rand.Value < tfChance;
		}

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public override string DebugString(Hediff_MutagenicBase hediff)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"Chance: {chance.ToStringPercent()}");
			if (affectedBySensitivity)
				builder.AppendLine($"Chance w/ Sensitivity: {(chance * hediff.TransformationSensitivity).ToStringPercent()}");
			return builder.ToString();
		}
	}

	/// <summary>
	/// A TFChance class that transforms the pawn with a random chance based on the full-TF setting
	/// Also affected by the TransformationSensitivity stat, unless disabled
	/// </summary>
	public class TFChance_BySetting : TFChance
	{
		/// <summary>
		/// The chance offset
		/// </summary>
		public float offset;
		/// <summary>
		/// The chance multiplier 
		/// </summary>
		public float mult = 1;

		/// <summary>
		/// Whether or not transformation sensitivity is respected.
		/// If true, the chance will be multiplied by the sensitivity stat
		/// </summary>
		[UsedImplicitly] public bool affectedBySensitivity = true;

		/// <summary>
		/// Whether or not to transform the pawn.  Checked only upon entering a stage.
		/// </summary>
		/// <param name="hediff">The hediff doing the transformation.</param>
		public override bool ShouldTransform(Hediff_MutagenicBase hediff)
		{
			// The setting is [0 - 100] rather than [0 - 1], so scale it down
			float tfChance = (LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance / 100f);
			tfChance = (tfChance + offset) * mult; //take into account any offset or multiplier 
			if (affectedBySensitivity)
				tfChance *= hediff.TransformationSensitivity;

			tfChance = Mathf.Clamp(tfChance, 0f, 1f);
			return Rand.Value < tfChance;
		}

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public override string DebugString(Hediff_MutagenicBase hediff)
		{
			StringBuilder builder = new StringBuilder();
			float chance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance / 100f;
			builder.AppendLine($"Chance: {chance.ToStringPercent()}");
			if (affectedBySensitivity)
				builder.AppendLine($"Chance w/ Sensitivity: {(chance * hediff.TransformationSensitivity).ToStringPercent()}");
			return builder.ToString();
		}
	}
}
