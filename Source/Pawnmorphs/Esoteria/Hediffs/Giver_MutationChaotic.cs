using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff giver based off of HediffGiver_Mutation, but instead of one mutation it gives one of many 
	/// </summary>
	public class Giver_MutationChaotic : HediffGiver
	{
		private List<MutationDef> _possibleMutations;

		/// <summary>
		/// list of morph categories to exclude 
		/// </summary>
		public List<MorphCategoryDef> blackListCategories = new List<MorphCategoryDef>();
		/// <summary>
		/// list of hediff defs to ignore 
		/// </summary>
		public List<HediffDef> blackListDefs = new List<HediffDef>();
		/// <summary>
		/// list of morphs to exclude 
		/// </summary>
		public List<MorphDef> blackListMorphs = new List<MorphDef>();

		/// <summary>
		/// if true, then this giver can give restricted mutations as well
		/// </summary>
		public bool allowRestricted;


		/// <summary>
		/// how often to give mutations 
		/// </summary>
		public float mtbDays = 0.4f;

		[NotNull]
		List<MutationDef> Mutations //hediff giver doesn't seem to have a on load or resolve references so I'm using lazy initialization
		{
			get
			{
				if (_possibleMutations == null)
				{
					_possibleMutations = MutationDef.AllMutations.Where(CheckMutation).ToList();

					if (_possibleMutations.Count == 0)
					{
						Log.Error($"a ChaoticMutation can't get any mutations to add! either things didn't load or the black lists are too large ");
					}

				}

				return _possibleMutations;
			}
		}

		private bool CheckMutation(MutationDef arg)
		{
			foreach (var blackMorph in blackListCategories.MakeSafe().SelectMany(c => c.AllMorphsInCategories))
			{
				if (arg.ClassInfluences.Any(x => x.Contains(blackMorph))) return false;
			}

			if (arg.IsRestricted && !allowRestricted) return false;


			return true;
		}

		/// <summary>
		/// The MTB unit
		/// </summary>
		public float mtbUnits = 60000f;

		/// <summary>
		/// occurs every so often for all hediffs that have this giver 
		/// </summary>
		/// <param name="pawn"></param>
		/// <param name="cause"></param>
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (Mutations.Count == 0) { return; }

			var singleComp = cause.TryGetComp<HediffComp_Single>();
			float mult = singleComp?.stacks
					  ?? 1; //the more stacks of partial morphs the pawn has the faster the mutation rate should be 
			mult *= pawn.GetStatValue(PMStatDefOf.MutagenSensitivity);
			mult *= singleComp?.Props?.mutationRateMultiplier ?? 1;

			mult = Mathf.Max(0.001f, mult); //prevent division by zero 

			if (Rand.MTBEventOccurs(mtbDays / mult, mtbUnits, 60) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
			{
				//mutagen is what contains information like infect-ability of a pawn and post mutation effects 
				var mutagen = cause?.def?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;

				TryApply(pawn, cause, mutagen);
			}
		}
		/// <summary>
		/// Tries to apply this hediff giver 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <param name="mutagen">The mutagen.</param>
		public void TryApply(Pawn pawn, Hediff cause, MutagenDef mutagen)
		{
			MutationDef mut = GetRandomMutation(pawn); //grab a random mutation 
			int mPart = mut.parts?.Count ?? 0;

			int maxCount;
			if (mPart == 0) maxCount = 0;
			else
				maxCount = GetMaxCount(pawn, mut.parts);


			if (MutationUtilities.AddMutation(pawn, mut, maxCount))
			{
				IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);

				var comp = cause.TryGetComp<HediffComp_Single>();
				if (comp != null)
				{
					comp.stacks--;
					if (comp.stacks <= 0)
					{
						pawn.health.RemoveHediff(cause);
					}
				}

			}
		}

		private int GetMaxCount([NotNull] Pawn pawn, [NotNull] List<BodyPartDef> mutParts)
		{
			var bDef = pawn.RaceProps.body;
			int counter = 0;
			foreach (BodyPartRecord bodyPartRecord in bDef.AllParts)
			{
				if (bodyPartRecord.IsMissingAtAllIn(pawn)) continue;
				if (mutParts.Contains(bodyPartRecord.def)) counter++;  //count all parts that can be added 
			}

			return counter;
		}

		private const int MAX_TRIES = 10;

		private MutationDef GetRandomMutation([NotNull] Pawn pawn)
		{
			var mutationGiver = Mutations[Rand.Range(0, Mutations.Count)];
			int i = 0;
			while (i < MAX_TRIES) //doing don't waist too much memory building temporary lists with LINQ 
								  //also means we won't return null if no mutation can be given 
			{
				i++; //make sure we terminate eventually 

				mutationGiver = Mutations[Rand.Range(0, Mutations.Count)];

				if (!pawn.HasMutation(mutationGiver)) break; //break if the pawn does not have the mutation yet

			}

			return mutationGiver;
		}
	}
}