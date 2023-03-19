// SyringeTfStage.cs created by Iron Wolf for Pawnmorph on 05/18/2020 6:53 AM
// last updated 05/18/2020  6:53 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.TransformationStageBase" />
	public class SyringeTfStage : TransformationStageBase
	{
		[NotNull]
		private static readonly Dictionary<PawnKindDef, List<MutationEntry>> _cachedEntries = new Dictionary<PawnKindDef, List<MutationEntry>>();



		/// <summary>
		/// Gets the entries for the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="source"></param>
		/// <returns></returns>
		public override IEnumerable<MutationEntry> GetEntries(Pawn pawn, Hediff source)
		{
			var sHediff = source as SyringeRifleTf;
			if (sHediff == null) return Enumerable.Empty<MutationEntry>();

			var chosenKind = sHediff.ChosenKind;
			if (chosenKind == null) return Enumerable.Empty<MutationEntry>();



			if (_cachedEntries.TryGetValue(chosenKind, out var lst)) return lst;

			//compute the mutations for the given entry 
			var morph = chosenKind.race.GetMorphOfRace();
			if (morph == null)
			{
				return Enumerable.Empty<MutationEntry>();
			}

			lst = new List<MutationEntry>();
			foreach (MutationDef mutation in morph.AllAssociatedMutations)
			{
				lst.Add(new MutationEntry()
				{
					addChance = 0.85f,
					blocks = false,
					mutation = mutation
				});
			}
			//and cache the results since this will not change 
			_cachedEntries[chosenKind] = lst;
			return lst;

		}
	}
}