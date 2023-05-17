// AnimalGenomeStorageComp.cs created by Iron Wolf for Pawnmorph on 08/29/2020 2:31 PM
// last updated 08/29/2020  2:31 PM

using System.Collections.Generic;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Chambers
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class AnimalGenomeStorageComp : ThingComp
	{
		private AnimalGenomeStorageCompProps Props => (AnimalGenomeStorageCompProps)props;

		/// <summary>
		/// Gets a value indicating whether this instance is consumed on use.
		/// </summary>
		/// <value>
		///   <c>true</c> if [consumed on use]; otherwise, <c>false</c>.
		/// </value>
		public bool ConsumedOnUse => Props?.consumedOnUse ?? true;

		/// <summary>
		/// Gets the animal this holds the genome for.
		/// </summary>
		/// <value>
		/// The animal.
		/// </value>
		public PawnKindDef Animal
		{
			get
			{
				if (parent is Genome_Animal genome)
					return genome.AnimalKind;

				return Props.animal;
			}
		}

		bool CanAdd
		{
			get
			{
				var db = Find.World.GetComponent<ChamberDatabase>();
				return db.CanAddToDatabase(new AnimalGenebankEntry(Animal));
			}
		}

		/// <summary>
		///     gets float menu options for this comp .
		/// </summary>
		/// <param name="selPawn">The sel pawn.</param>
		/// <returns></returns>
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			JobFailReason.Clear();

			string scanFailReason = (props as AnimalGenomeStorageCompProps).scanFailReason;
			if (string.IsNullOrWhiteSpace(scanFailReason) == false)
			{
				yield return new
					FloatMenuOption("CannotGenericWorkCustom".Translate(WORK_APPLY_GENOME.Translate(parent.Label)) + ": " + scanFailReason.CapitalizeFirst(), null);
				yield break;
			}

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
			else if (!wComp.CanAddToDatabase(new AnimalGenebankEntry(Animal), out string reason))
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

		private const string NO_MUTATION_BENCH_AVAILABLE = "PMNoMutationBenchAvailable";

		private const string WORK_APPLY_GENOME = "PMApplyGenome";

		private const string CANNOT_STORE_ANIMAL = "PMNoStorageAvailableForGenome";


	}

	/// <summary>
	/// props for <see cref="AnimalGenomeStorageComp"/>
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class AnimalGenomeStorageCompProps : CompProperties
	{
		/// <summary>
		/// if this thing is consumed on use
		/// </summary>
		public bool consumedOnUse = true;

		/// <summary>
		/// Special reason that block scanning if set.
		/// </summary>
		public string scanFailReason = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimalGenomeStorageCompProps"/> class.
		/// </summary>
		public AnimalGenomeStorageCompProps()
		{
			compClass = typeof(AnimalGenomeStorageComp);
		}


		/// <summary>
		/// The pawn kind
		/// </summary>
		public PawnKindDef animal;

	}

}