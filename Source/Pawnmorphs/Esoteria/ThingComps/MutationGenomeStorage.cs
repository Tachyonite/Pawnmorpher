// MutationGenomeStorage.cs created by Iron Wolf for Pawnmorph on 08/07/2020 1:43 PM
// last updated 08/07/2020  1:43 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	///     comp that acts like a techprint comp for mutation genomes
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class MutationGenomeStorage : ThingComp
	{
		private const string NO_MUTATION_BENCH_AVAILABLE = "PMNoMutationBenchAvailable";

		private const string WORK_APPLY_GENOME = "PMApplyGenome";

		private const string CANNOT_STORE_MUTATION = "PMNoStorageAvailableForGenome";

		private const string MUTATION_NAME = "MUTATION";

		/// <summary>
		///     Gets the mutation this provides the genome info for.
		/// </summary>
		/// <value>
		///     The mutation.
		/// </value>
		[NotNull]
		public MutationCategoryDef Mutation => Props.mutation;

		private MutationGenomeStorageProps Props => (MutationGenomeStorageProps)props;

		/// <summary>
		///     gets float menu options for this comp .
		/// </summary>
		/// <param name="selPawn">The sel pawn.</param>
		/// <returns></returns>
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			JobFailReason.Clear();

			var wComp = Find.World.GetComponent<ChamberDatabase>();
			if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Research) || selPawn.WorkTagIsDisabled(WorkTags.Intellectual))
			{
				JobFailReason.Is("WillNever".Translate("Research".TranslateSimple().UncapitalizeFirst()));
			}
			else if (!selPawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Some))
			{
				JobFailReason.Is("CannotReach".Translate());
			}
			else if (!selPawn.CanReserve(parent))
			{
				Pawn pawn = selPawn.Map.reservationManager.FirstRespectedReserver(parent, selPawn);
				if (pawn == null) pawn = selPawn.Map.physicalInteractionReservationManager.FirstReserverOf(selPawn);
				if (pawn != null)
					JobFailReason.Is("ReservedBy".Translate(pawn.LabelShort, pawn));
				else
					JobFailReason.Is("Reserved".Translate());
			}
			else if (!wComp.CanAddAnyToDatabase(Mutation.AllMutations, out string reason))
			{
				JobFailReason.Is(reason);
			}

			HaulAIUtility.PawnCanAutomaticallyHaul(selPawn, parent, true);
			Thing thing2 =
				GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map, ThingRequest.ForDef(PMThingDefOf.MutagenLab),
												 PathEndMode.InteractionCell,
												 TraverseParms.For(selPawn, Danger.None),
												 validator: t => t.Faction == selPawn.Faction && selPawn.CanReserve(t));
			Job job = null;
			if (thing2 != null)
			{
				job = JobMaker.MakeJob(PMJobDefOf.PM_UseMutationGenome);
				job.targetA = thing2;
				job.targetB = parent;
				job.targetC = thing2.Position;
			}

			if (JobFailReason.HaveReason)
			{
				yield return new
					FloatMenuOption("CannotGenericWorkCustom".Translate(WORK_APPLY_GENOME.Translate(parent.Label)) + ": " + JobFailReason.Reason.CapitalizeFirst(),
									null);
				JobFailReason.Clear();
				yield break;
			}

			yield return new FloatMenuOption(WORK_APPLY_GENOME.Translate(parent.Label).CapitalizeFirst(), delegate
			{
				if (job == null)
					Messages.Message(NO_MUTATION_BENCH_AVAILABLE.Translate(), MessageTypeDefOf.RejectInput);
				else
					selPawn.jobs.TryTakeOrderedJob(job);
			});
		}
	}

	/// <summary>
	///     comp properties for <see cref="MutationGenomeStorage" />
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class MutationGenomeStorageProps : CompProperties
	{
		/// <summary>
		///     The mutation this provides the genomes info about
		/// </summary>
		public MutationCategoryDef mutation;

		/// <summary>
		///     Initializes a new instance of the <see cref="MutationGenomeStorageProps" /> class.
		/// </summary>
		public MutationGenomeStorageProps()
		{
			compClass = typeof(MutationGenomeStorage);
		}

		/// <summary>
		///     gets all configuration errors
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef).MakeSafe()) yield return configError;

			if (mutation == null)
				yield return "no mutation set";
		}
	}
}