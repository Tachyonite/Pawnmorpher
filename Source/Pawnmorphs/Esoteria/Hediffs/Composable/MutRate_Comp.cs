// MutRate_Comp.cs created by Iron Wolf for Pawnmorph on 09/05/2021 8:28 PM
// last updated 09/05/2021  8:28 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// mute rate that defers it's logic to a <see cref="HediffComp_Composable"/> class 
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.MutRate" />
	public class MutRate_Comp : MutRate
	{
		private IMutRate _cachedRate;

		[CanBeNull]
		IMutRate GetCompRate([NotNull] Hediff hDiff)
		{
			if (hDiff == null) throw new ArgumentNullException(nameof(hDiff));
			if (_cachedRate != null) return _cachedRate; //ok to cache this, comps should never be added/removed during runtime 
			var cHDiff = hDiff as HediffWithComps;
			var rate = cHDiff?.comps?.OfType<IMutRate>().FirstOrDefault();
			if (rate == null)
			{

				string mErr = "";
				if (cHDiff != null)
				{
					mErr = string.Join(",", cHDiff.comps.MakeSafe().Select(c => c.GetType().Name));
				}
				else
				{
					mErr = $"{hDiff.def.defName} is not a hediff with comps";
				}

				Log.ErrorOnce($"{hDiff.def} has {nameof(MutRate_Comp)} but no {nameof(IMutRate)}!checked comps:{mErr}",
							  hDiff.def.shortHash);

			}


			_cachedRate = rate;
			return _cachedRate;
		}

		/// <summary>
		/// How many mutations to queue up for the next second.
		/// 
		/// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
		/// be spread out by that class, so no rate limiting needs to happen here.
		/// </summary>
		/// <returns>The number of mutations to add.</returns>
		/// <param name="hediff">Hediff.</param>
		public override int GetMutationsPerSecond(Hediff_MutagenicBase hediff)
		{
			return GetCompRate(hediff)?.GetMutationsPerSecond(hediff) ?? 0;
		}

		/// <summary>
		/// How many mutations to queue up for a given severity change.  Note that severity
		/// changes can be negative, and negative mutations are allowed.
		/// (negative mutations can cancel queued mutations but won't remove existing ones)
		/// 
		/// Called any time severity changes in Hediff_MutagenicBase.  Queued up mutations will
		/// be spread out by that class, so no rate limiting needs to happen here.
		/// </summary>
		/// <returns>The number of mutations to add.</returns>
		/// <param name="hediff">Hediff.</param>
		/// <param name="sevChange">How much severity changed by.</param>
		public override int GetMutationsPerSeverity(Hediff_MutagenicBase hediff, float sevChange)
		{
			return GetCompRate(hediff)?.GetMutationsPerSeverity(hediff, sevChange) ?? 0;
		}

		/// <summary>
		/// gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			var props = parentDef.CompProps<HediffCompProps_Composable>();
			if (props == null) yield return $"no {nameof(HediffCompProps_Composable)}!";
		}
	}
}