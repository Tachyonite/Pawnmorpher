using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// A class that determines the gender of the pawn post-transformation
	/// </summary>
	public abstract class TFGenderSelector : IInitializableStage
	{
		/// <summary>
		/// Gets the gender of the pawn post-transformation
		/// </summary>
		/// <param name="hediff">The TFing hediff.</param>
		public abstract TFGender GetGender(Hediff_MutagenicBase hediff);

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
		public void ResolveReferences(HediffDef parent)
		{
			//empty 
		}
	}

	/// <summary>
	/// A gender selector that simply always uses the same gender as the pawn
	/// </summary>
	public class TFGenderSelector_Same : TFGenderSelector
	{
		/// <summary>
		/// Gets the gender of the pawn post-transformation
		/// </summary>
		/// <param name="hediff">The TFing hediff.</param>
		public override TFGender GetGender(Hediff_MutagenicBase hediff) => TFGender.Original;
	}

	/// <summary>
	/// A gender selector that has a 50/50 chance of being male or female
	/// </summary>
	public class TFGenderSelector_Random : TFGenderSelector
	{
		/// <summary>
		/// Gets the gender of the pawn post-transformation
		/// </summary>
		/// <param name="hediff">The TFing hediff.</param>
		public override TFGender GetGender(Hediff_MutagenicBase hediff) => Rand.Bool ? TFGender.Female : TFGender.Male;
	}

	/// <summary>
	/// A gender selector that has a configurable chance to swap the gender
	/// </summary>
	public class TFGenderSelector_Swap : TFGenderSelector
	{
		/// <summary>
		/// The chance of swapping the pawn's gender.
		/// </summary>
		[UsedImplicitly] public float chance = 1f;

		/// <summary>
		/// Gets the gender of the pawn post-transformation
		/// </summary>
		/// <param name="hediff">The TFing hediff.</param>
		public override TFGender GetGender(Hediff_MutagenicBase hediff)
		{
			return Rand.Value < chance ? TFGender.Switch : TFGender.Original;
		}

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public override string DebugString(Hediff_MutagenicBase hediff) => $"Swap chance: {chance.ToStringPercent()}";
	}

	/// <summary>
	/// A gender selector that has a configurable chance to force a specific gender
	/// </summary>
	public class TFGenderSelector_Gender : TFGenderSelector
	{
		/// <summary>
		/// The forced gender
		/// </summary>
		[UsedImplicitly] public TFGender gender;

		/// <summary>
		/// The chance of forcing the specified gender.
		/// </summary>
		[UsedImplicitly] public float chance = 1f;

		/// <summary>
		/// Gets the gender of the pawn post-transformation
		/// </summary>
		/// <param name="hediff">The TFing hediff.</param>
		public override TFGender GetGender(Hediff_MutagenicBase hediff)
		{
			return Rand.Value < chance ? gender : TFGender.Original;
		}

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public override string DebugString(Hediff_MutagenicBase hediff)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"Forced gender: {gender}");
			builder.AppendLine($"Force chance: {chance.ToStringPercent()}");
			return builder.ToString();
		}
	}
}
