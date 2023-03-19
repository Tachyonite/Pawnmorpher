// MutationSequencerComp.cs created by Iron Wolf for Pawnmorph on 11/14/2020 8:28 AM
// last updated 11/14/2020  8:28 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// </summary>
	public class MutationSequencerComp : CompScanner
	{
		private const string MUTATION_GATHERED_LABEL = "PMMutationTagged";

		[NotNull] private readonly List<MutationDef> _scratchList = new List<MutationDef>();

		private AnimalSelectorComp _animalSelector;

		private ChamberDatabase _db;


		/// <summary>
		/// Useds the specified worker.
		/// </summary>
		/// <param name="worker">The worker.</param>
		public new void Used(Pawn worker)
		{
			if (!this.CanUseNow)
				Log.Error("Used while CanUseNow is false.");
			this.lastScanTick = (float)Find.TickManager.TicksGame;
			this.lastUserSpeed = 1f;
			if (this.Props.scanSpeedStat != null)
				this.lastUserSpeed = worker.GetStatValue(this.Props.scanSpeedStat, true);
			this.daysWorkingSinceLastFinding += this.lastUserSpeed / 60000f;
			if (!this.TickDoesFind(this.lastUserSpeed))
				return;
			this.DoFind(worker);
			this.daysWorkingSinceLastFinding = 0.0f;
		}

		/// <summary>
		///     Gets a value indicating whether this instance can use now.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can use now; otherwise, <c>false</c>.
		/// </value>
		public new bool CanUseNow
		{
			get
			{
				if (parent?.Spawned != true)
				{
					return false;
				}
				if (powerComp != null && !powerComp.PowerOn)
				{
					return false;
				}

				if (forbiddable != null && forbiddable.Forbidden)
				{
					return false;
				}
				return parent.Faction == Faction.OfPlayer && AnimalSelector.ChosenKind != null && DB.CanTag;
			}
		}

		[NotNull]
		private AnimalSelectorComp AnimalSelector
		{
			get
			{
				if (_animalSelector == null)
				{
					_animalSelector = parent.GetComp<AnimalSelectorComp>();
					_animalSelector.SpeciesFilter = (x) => x.GetAllMutationsFrom().Any(m => !DB.StoredMutations.Contains(m));
				}

				return _animalSelector;
			}
		}

		[NotNull]
		private ChamberDatabase DB
		{
			get
			{
				if (_db == null) _db = Find.World.GetComponent<ChamberDatabase>();

				return _db;
			}
		}

		/// <summary>
		///     Comps the get gizmos extra.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra()) yield return gizmo;
		}

		/// <summary>
		///     Does the find.
		/// </summary>
		/// <param name="worker">The worker.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		protected override void DoFind(Pawn worker)
		{
			PawnKindDef chosenKind = AnimalSelector.ChosenKind;
			if (chosenKind == null)
			{
				Log.Error($"calling DoFind on {parent.ThingID} which does not have a chosen animal!");
				return;
			}

			_scratchList.Clear();
			_scratchList.AddRange(chosenKind.GetAllMutationsFrom().Where(m => !DB.StoredMutations.Contains(m)));

			if (_scratchList.Count == 0)
			{
				Log.Warning("unable to find mutation to give!");
				AnimalSelector.ResetSelection();
				return;
			}

			MutationDef mutation = _scratchList.RandomElement();

			DB.TryAddToDatabase(new MutationGenebankEntry(mutation));

			TaggedString msg = MUTATION_GATHERED_LABEL.Translate(mutation.Named("mutation"),
																 chosenKind.Named("animal")
																);
			Messages.Message(msg, MessageTypeDefOf.PositiveEvent);
			if (_scratchList.Count - 1 == 0)
				AnimalSelector.ResetSelection();
		}
	}
}