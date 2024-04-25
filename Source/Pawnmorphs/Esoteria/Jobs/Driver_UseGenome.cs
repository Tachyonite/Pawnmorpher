// Driver_UseGenome.cs created by Iron Wolf for Pawnmorph on 08/07/2020 3:06 PM
// last updated 08/07/2020  3:06 PM

using System.Collections.Generic;
using System.Linq;
using AlienRace;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
using Pawnmorph.ThingComps;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	/// job driver for using a genome 
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	public class Driver_UseGenome : JobDriver
	{
		private const string NO_MORE_MUTATIONS_MESSAGE = "PMNoMoreMutations";
		private const string TAGGED_MUTATIONS_MESSAGE = "MutationAddedToDatabase";
		private const string TAGGED_ANIMAL_MESSAGE = "PMAnimalAddedToDatabase";

		const TargetIndex MUTAGEN_BENCH_INDEX = TargetIndex.A;
		const TargetIndex GENOME_INDEX = TargetIndex.B;
		private const TargetIndex HAUL_INDEX = TargetIndex.C;

		private const int DURATION = 100;
		private Building MutagenBench => (Building)job.GetTarget(MUTAGEN_BENCH_INDEX).Thing;

		private ThingWithComps Genome => (ThingWithComps)job.GetTarget(GENOME_INDEX).Thing;




		/// <summary>
		/// Tries the make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
		/// <returns></returns>
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(MutagenBench, job, errorOnFailed: errorOnFailed))
			{
				return pawn.Reserve(Genome, job, errorOnFailed: errorOnFailed);
			}

			return false;
		}

		/// <summary>
		/// Makes the new toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_General.DoAtomic(delegate { job.count = 1; });
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
								   .FailOnDespawnedNullOrForbidden(TargetIndex.B)
								   .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, false);
			yield return Toils_General.Wait(600)
									  .FailOnDestroyedNullOrForbidden(TargetIndex.B)
									  .FailOnDestroyedNullOrForbidden(TargetIndex.A)
									  .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell)
									  .WithProgressBarToilDelay(TargetIndex.A);
			var toil = new Toil
			{
				initAction = ApplyGenome,
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return toil;
		}

		private void ApplyGenome()
		{
			var db = Find.World.GetComponent<ChamberDatabase>();
			var mutationComp = Genome.GetComp<MutationGenomeStorage>();

			if (mutationComp == null)
			{
				var animalComp = Genome.GetComp<AnimalGenomeStorageComp>();
				if (animalComp == null)
				{
					Log.Error("Tried to use a genome with no mutation or animal genome comp!");
					return;
				}

				bool added = db.TryAddToDatabase(new AnimalGenebankEntry(animalComp.Animal));
				if (added)
				{
					if (PawnmorpherMod.Settings.AutoSequenceAnimalGenome)
					{
						foreach (MutationDef item in animalComp.Animal.GetAllMutationsFrom())
						{
							// Try add until fail.
							if (db.TryAddToDatabase(new MutationGenebankEntry(item)) == false)
								break;
						}
					}

					if (animalComp.ConsumedOnUse)
						Genome.Destroy();

					TaggedString msg = TAGGED_ANIMAL_MESSAGE.Translate(animalComp.Animal.Named("animal"));
					Messages.Message(msg, MessageTypeDefOf.PositiveEvent);
				}
			}
			else
			{
				// Don't check CanAddToDatabase here, since that will fail even if the database is just out of power or storage space.
				// Instead, explicitly check for taggable mutations that haven't been added yet.
				// If we're out of power/space, the call to TryAddToDatabase will fail and display the appropriate fail message.
				MutationCategoryDef mCategory = mutationComp.Mutation;
				List<MutationDef> validMutations = mCategory.AllMutations
						.Where(CanBeAdded)
						.Where(m => !db.StoredMutations.Contains(m))
						.ToList();

				if (validMutations.Count == 0)
				{
					Messages.Message(NO_MORE_MUTATIONS_MESSAGE.Translate(Genome), MessageTypeDefOf.RejectInput);
					return;
				}


				bool CanBeAdded(MutationDef mDef)
				{
					return mDef.RestrictionLevel <= mCategory.restrictionLevel && mDef.RestrictionLevel != RestrictionLevel.Always;
				}

				MutationDef mutation = validMutations.RandomElement();
				bool added = db.TryAddToDatabase(new MutationGenebankEntry(mutation));
				if (added)
				{
					if (mutationComp.Mutation.genomeConsumedOnUse)
						Genome.Destroy();

					TaggedString msg = TAGGED_MUTATIONS_MESSAGE.Translate(mutation.Named("Mutation"));
					Messages.Message(msg, MessageTypeDefOf.PositiveEvent);
				}
			}
		}
	}
}