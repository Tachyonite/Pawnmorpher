// AddRandomVeneratedMutation.cs created by Iron Wolf for Pawnmorph on 07/29/2021 5:31 PM
// last updated 07/29/2021  5:31 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Rituals.AttachableOutcomeEffectWorkers
{
	/// <summary>
	///     ritual outcome effect worker that adds a random number of mutations of a venerated animal on a target
	/// </summary>
	/// <seealso cref="RimWorld.RitualAttachableOutcomeEffectWorker" />
	public class AddRandomVeneratedMutation : RitualAttachableOutcomeEffectWorker
	{
		/// <summary>
		///     translation id for the translated outcome text
		/// </summary>
		protected const string MUTATION_TRANSLATION = "PM_PawnMutatated_Ritual";


		/// <summary>
		///     the tag that refers to the number of pawns mutated in the ritual
		/// </summary>
		protected const string COUNT_TAG = "Count";

		/// <summary>
		///     the tag for members of this ritual
		/// </summary>
		protected const string MEMBER_TAG = "Member";


		/// <summary>
		///     Applies the effect on the given ritual .
		/// </summary>
		/// <param name="totalPresence">The total presence.</param>
		/// <param name="jobRitual">The job ritual.</param>
		/// <param name="outcome">The outcome.</param>
		/// <param name="extraOutcomeDesc">The extra outcome desc.</param>
		/// <param name="letterLookTargets">The letter look targets.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome,
								   out string extraOutcomeDesc,
								   ref LookTargets letterLookTargets)
		{
			InitRitual(jobRitual, outcome);
			IEnumerable<Pawn> pawns = GetTargets(jobRitual, outcome);

			var count = 0;
			var scratchList = new List<MutationDef>();
			foreach (Pawn pawn in pawns)
			{
				int mutationCount = GetMutationCount(pawn, outcome);
				if (mutationCount == 0) continue;
				scratchList.Clear();
				scratchList.AddRange(GetMutationsToAdd(pawn, jobRitual, outcome, out _));

				int mAdded = 0, tries = 0;
				const int maxTries = 100;
				HediffSet hediffSet = pawn.health.hediffSet;
				while (mAdded < mutationCount && tries < maxTries && scratchList.Count > 0)
				{
					tries++;

					MutationDef mut = scratchList.RandomElement();
					scratchList.Remove(mut);
					if (hediffSet.GetFirstHediffOfDef(mut) != null) 
						continue;

					var res = MutationUtilities.AddMutation(pawn, mut);

					foreach (Hediff_AddedMutation added in res)
					{
						added.Causes.TryAddCause("ritualOutcome", def);
						added.Causes.TryAddPrecept(jobRitual.Ritual);
						added.Causes.SetLocation(jobRitual.Spot, jobRitual.Map);
					}

					mAdded++;
				}

				count++;
			}

			if (count > 0)
			{
				Ideo ideo = jobRitual.Ritual.ideo;
				string mName = count > 1 ? ideo.MemberNamePlural : ideo.memberName;
				extraOutcomeDesc = def.letterInfoText.Formatted(count.Named(COUNT_TAG), mName.Named(MEMBER_TAG));
			}
			else
			{
				extraOutcomeDesc = "";
			}
		}

		/// <summary>
		///     Determines whether this instance with the specified ritual can be applied
		/// </summary>
		/// <param name="ritual">The ritual.</param>
		/// <param name="map">The map.</param>
		/// <returns></returns>
		public override AcceptanceReport CanApplyNow(Precept_Ritual ritual, Map map)
		{
			return ritual.ideo.VeneratedAnimals.MakeSafe().Any(a => a.TryGetBestMorphOfAnimal() != null);
		}

		/// <summary>
		///     Gets the mutation count.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="outcomeChance">The outcome chance.</param>
		/// <returns></returns>
		protected virtual int GetMutationCount([NotNull] Pawn pawn, [NotNull] RitualOutcomePossibility outcomeChance)
		{
			return outcomeChance.positivityIndex * Rand.Range(1, 3);
		}


		/// <summary>
		///     Gets the mutations to add onto the given target
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="jobRitual">The job ritual.</param>
		/// <param name="outcome">The outcome.</param>
		/// <param name="chosenAnimal">The venerated animal that was chosen</param>
		/// <returns></returns>
		[NotNull]
		protected virtual IEnumerable<MutationDef> GetMutationsToAdd([NotNull] Pawn target, [NotNull] LordJob_Ritual jobRitual,
																	 [NotNull] RitualOutcomePossibility outcome, out ThingDef chosenAnimal)
		{
			Ideo ideo = jobRitual.Ritual.ideo;
			if (ideo == null)
			{
				Log.Error($"unable to find ideo while getting mutations for outcome of {def.defName}!");
				chosenAnimal = null;
				return Enumerable.Empty<MutationDef>();
			}

			if (!ideo.VeneratedAnimals.MakeSafe()
					 .Select(a => (a, a.TryGetBestMorphOfAnimal()))
					 .Where(tup => tup.Item2 != null)
					 .TryRandomElement(out (ThingDef a, MorphDef) rTup))
			{
				Log.Error($"{def.defName} is unable to get valid venerated animal from {ideo.name} to get mutations from!");
				chosenAnimal = null;
				return Enumerable.Empty<MutationDef>();
			}

			chosenAnimal = rTup.a;
			return rTup.Item2.AllAssociatedMutations;
		}

		[NotNull]
		private readonly List<Pawn> _scratchList = new List<Pawn>();

		/// <summary>
		/// Gets the targets to add mutations onto
		/// </summary>
		/// <param name="jobRitual">The job ritual.</param>
		/// <param name="outcome">The outcome.</param>
		/// <returns></returns>
		[NotNull]
		protected virtual IEnumerable<Pawn> GetTargets([NotNull] LordJob_Ritual jobRitual, [NotNull] RitualOutcomePossibility outcome)
		{
			RitualRole role = jobRitual.GetRole(RoleTags.TARGET_TAG);
			IEnumerable<Pawn> enumer;
			if (role == null) 
				enumer = jobRitual.assignments.Participants.Where(p => MutagenDefOf.defaultMutagen.CanInfect(p));
			else
				enumer = (jobRitual.assignments?.AssignedPawns(role)).MakeSafe();

			_scratchList.Clear();
			_scratchList.AddRange(enumer);

			// Remove invalid pawns.
			_scratchList.RemoveAll(x => x.health == null || x.Dead);

			float maxOutcome = jobRitual?.Ritual?.def?.ritualPatternBase?.ritualOutcomeEffect?.outcomeChances
									   ?.MaxBy(c => c.positivityIndex)
									   ?.positivityIndex
							?? 1f;
			maxOutcome = Mathf.Max(maxOutcome, 1);
			float outcomeAdj = outcome.positivityIndex
							 / (maxOutcome * 1.3f);
			int max = Mathf.CeilToInt(_scratchList.Count * Rand.Range(0.1f + outcomeAdj, 0.4f + outcomeAdj));
			int take = Mathf.Max(0, max);
			if (take == 0) 
				yield break;


			var i = 0;
			while (i < take && _scratchList.Count > 0)
			{
				Pawn r =
					_scratchList.RandomElementByWeight(p =>
														   GetSelectionWeight(p,
																			  jobRitual
																				 .Ritual
																				 .ideo)); //make it more likely non mutated pawns are chosen 
				_scratchList.Remove(r);
				i++;
				yield return r;
			}
		}


		float GetSelectionWeight([NotNull] Pawn p, [NotNull] Ideo ideo)
		{
			float count = 1;
			var mutations = (p.health?.hediffSet?.hediffs).MakeSafe().OfType<Hediff_AddedMutation>();
			foreach (Hediff_AddedMutation mutation in mutations)
			{
				if (ideo.VeneratedAnimals.Any(a => a.TryGetBestMorphOfAnimal()?.IsAnAssociatedMutation(mutation) == true))
				{
					count++;
				}
			}

			return 1 / count;
		}

		/// <summary>
		///     Initializes the ritual effects. called when starting to process the effects of this ritual
		/// </summary>
		/// <param name="ritual">The ritual.</param>
		/// <param name="outcome">The outcome.</param>
		protected virtual void InitRitual([NotNull] LordJob_Ritual ritual, [NotNull] RitualOutcomePossibility outcome)
		{
		}
	}
}