// Giver_TransformPrisoner.cs created by Iron Wolf for Pawnmorph on 10/20/2020 7:01 AM
// last updated 10/20/2020  7:01 AM

using JetBrains.Annotations;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Work
{
	/// <summary>
	/// work giver for transforming prisoners 
	/// </summary>
	/// <seealso cref="RimWorld.WorkGiver_Warden" />
	public class Giver_TransformPrisoner : WorkGiver_Warden
	{
		private const string NON_TRANSFORMABLE_PAWN_MESSAGE = "PMCannotTransformPrisoner";

		bool EnsurePrisonerIsTransformable([NotNull] Pawn prisoner)
		{
			var guest = prisoner.guest;
			if (guest == null) return false;
			if (!MutagenDefOf.MergeMutagen.CanTransform(prisoner))
			{
				Messages.Message(NON_TRANSFORMABLE_PAWN_MESSAGE.Translate(prisoner), prisoner, MessageTypeDefOf.RejectInput);
				guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.MaintainOnly);
				return false;
			}

			return true;
		}



		/// <summary>
		/// gets the job on the given thing 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="t">The t.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns></returns>
		[CanBeNull]
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			bool logging = pawn.jobs?.debugLog == true;
			if (!(t is Pawn prisoner))
			{
				if (logging)
				{
					Log.Message($"{pawn.Name} cannot transform non pawn {t.ThingID}");
				}
				return null;
			}

			if (!prisoner.IsPrisonerOfColony)
			{
				if (logging)
				{
					Log.Message($"{prisoner.Name} is not a prisoner");
				}

				return null;
			}

			if (pawn.CanReserve(prisoner, ignoreOtherReservations: forced) == false)
			{
				if (logging)
				{
					Log.Message($"{prisoner.Name} cannot be reserved!");
				}

				return null;
			}

			var guest = prisoner.guest;

			if (guest == null)
			{
				if (logging)
				{
					Log.Message($"{prisoner.Name} does not have a guest tracker");
				}

				return null;
			}

			if (guest.ExclusiveInteractionMode != PMPrisonerInteractionModeDefOf.PM_Transform)
			{
				if (logging)
					Log.Message($"{prisoner.Name} is not set to be transformed");
				return null;
			}

			if (!EnsurePrisonerIsTransformable(prisoner))
			{
				if (logging)
				{
					Log.Message($"{prisoner.Name} cannot be transformed!");
				}

				return null;
			}

			var chamber = FindValidChamberFor(pawn, prisoner);
			if (chamber == null)
			{
				if (logging)
					Log.Message($"cannot find chamber for {prisoner.Name}");
				return null;
			}

			var jobDef = PMJobDefOf.PM_TransformPrisoner;

			var job = JobMaker.MakeJob(jobDef, prisoner, chamber);
			job.count = 1;
			return job;
		}

		[CanBeNull]
		MutaChamber FindValidChamberFor([NotNull] Pawn worker, [NotNull] Pawn prisoner)
		{
			var chamber = MutaChamber.FindMutaChamberFor(prisoner, worker);
			return chamber;
		}

	}
}