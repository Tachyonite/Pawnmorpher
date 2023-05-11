using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     mutation hediff giver that will only grab mutations from specific categories
	/// </summary>
	/// <seealso cref="Verse.HediffGiver" />
	public class Giver_MutationCategoryGiver : HediffGiver
	{
		/// <summary>
		///     list of mutation categories to look for
		/// </summary>
		[NotNull] public List<MutationCategoryDef> mutationCategories = new List<MutationCategoryDef>();

		/// <summary>
		///     The morph categories to get mutations from
		/// </summary>
		[NotNull] public List<MorphCategoryDef> morphCategories = new List<MorphCategoryDef>();

		/// <summary>
		///     The MTB days
		/// </summary>
		public float mtbDays = 0.4f;

		/// <summary>
		///     The MTB unit
		/// </summary>
		public float mtbUnits = 60000f;

		private List<MutationDef> _mutations;

		[NotNull]
		private List<MutationDef> Mutations
		{
			get
			{
				if (_mutations == null)
				{
					_mutations = new List<MutationDef>();
					foreach (MorphDef morphDef in morphCategories.SelectMany(m => m.AllMorphsInCategories))
						foreach (MutationDef mutation in morphDef.AllAssociatedMutations)
							if (!_mutations.Contains(mutation))
								_mutations.Add(mutation); //add only distinct values 

					foreach (MutationDef mutation in mutationCategories.SelectMany(m => m.AllMutations))
					{
						if (!_mutations.Contains(mutation))
							_mutations.Add(mutation);
					}
				}

				return _mutations;
			}
		}

		/// <summary>
		///     occurs every so often for all hediffs that have this giver
		/// </summary>
		/// <param name="pawn"></param>
		/// <param name="cause"></param>
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (Mutations.Count == 0) return;

			if (Rand.MTBEventOccurs(mtbDays, mtbUnits, 60) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
			{
				MutagenDef mutagen = cause?.def?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;
				TryApply(pawn, cause, mutagen);
			}
		}


		/// <summary>
		///     Tries to apply this hediff giver
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <param name="mutagen">The mutagen.</param>
		public void TryApply(Pawn pawn, Hediff cause, [NotNull] MutagenDef mutagen)
		{
			if (mutagen == null) throw new ArgumentNullException(nameof(mutagen));
			var mut = Mutations[Rand.Range(0, Mutations.Count)]; //grab a random mutation 
			if (MutationUtilities.AddMutation(pawn, mut))
			{
				IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
				if (cause.def.HasComp(typeof(HediffComp_Single))) pawn.health.RemoveHediff(cause);
				mutagen.TryApplyAspects(pawn);
				if (mut.mutationTale != null) TaleRecorder.RecordTale(mut.mutationTale, pawn);
			}
		}
	}
}