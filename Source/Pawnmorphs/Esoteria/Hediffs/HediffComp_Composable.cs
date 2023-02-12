// HediffComp_Composible.cs created by Iron Wolf for Pawnmorph on 09/05/2021 8:08 PM
// last updated 09/05/2021  8:08 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// A comp for composable mutagenic hediffs where the Hediff is responsible for deciding what kinds of mutations to apply.
	/// Needed because hediff stages can't store state and so the state must be saved to the Hediff or one of its comps.
	/// Meant to be used with <see cref="MutTypes_FromComp"/>
	/// </summary>
	/// <seealso cref="Pawnmorph.Utilities.HediffCompBase{HediffCompProps_Composable}" />
	public class HediffComp_Composable : HediffCompBase<HediffCompProps_Composable>, IMutRate
	{

		/// <summary>
		/// Gets the mut rate.
		/// </summary>
		/// <value>
		/// The rate.
		/// </value>
		[CanBeNull]
		public MutRate Rate => Props.mutRate;

		/// <summary>
		/// Gets the types.
		/// </summary>
		/// <value>
		/// The types.
		/// </value>
		[CanBeNull]
		public MutTypes Types => Props.mutTypes;

		string IMutRate.DebugString(Hediff_MutagenicBase hediff)
		{
			return Rate.DebugString(hediff);
		}

		int IMutRate.GetMutationsPerSecond(Hediff_MutagenicBase hediff)
		{
			return Rate.GetMutationsPerSecond(hediff);
		}

		int IMutRate.GetMutationsPerSeverity(Hediff_MutagenicBase hediff, float sevChange)
		{
			return Rate.GetMutationsPerSeverity(hediff, sevChange);
		}
	}


	/// <summary>
	/// properties for <see cref="HediffComp_Composable"/>
	/// </summary>
	/// <seealso cref="Pawnmorph.Utilities.HediffCompPropertiesBase{HediffComp_Composable}" />
	public class HediffCompProps_Composable : HediffCompPropertiesBase<HediffComp_Composable>
	{

		/// <summary>
		/// The mute rate
		/// </summary>
		public MutRate mutRate;
		/// <summary>
		/// The mut types
		/// </summary>
		public MutTypes mutTypes;


		/// <summary>
		/// get all configuration errors with this instance 
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
			{
				yield return configError;
			}

			if (mutTypes == null && mutRate == null)
			{
				yield return $"neither {nameof(mutTypes)} nor {mutRate} is set!";
			}
		}
	}

}