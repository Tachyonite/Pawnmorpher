// MutationSequencerComp.cs created by Iron Wolf for Pawnmorph on 11/14/2020 8:28 AM
// last updated 11/14/2020  8:28 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
using Pawnmorph.UserInterface;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// </summary>
	public class MutationSequencerComp : CompScanner
	{
		private const string MUTATION_GATHERED_LABEL = "PMMutationTagged";

		private ChamberDatabase _genebank;
		private List<MutationDef> _scratchList;
		private PawnKindDef _targetAnimal;
		private bool _animalSequenced;

		Gizmo _advPreview;


		public PawnKindDef TargetAnimal
		{
			get => _targetAnimal;
			set
			{
				_animalSequenced = false;
				_targetAnimal = value;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			_genebank = Find.World.GetComponent<ChamberDatabase>();
			_scratchList = new List<MutationDef>(50);
			_animalSequenced = false;

			_advPreview = new Command_Action()
			{
				action = OpenSequencerInterface,
				icon = PMTextures.AnimalSelectorIcon,
				defaultLabel = "Open"
			};
		}

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

			this.daysWorkingSinceLastFinding += this.lastUserSpeed * PawnmorpherMod.Settings.SequencingMultiplier / TimeMetrics.TICKS_PER_DAY;
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

				if (_animalSequenced)
					return false;

				return parent.Faction == Faction.OfPlayer && _targetAnimal != null && _genebank.CanTag;
			}
		}

		/// <summary>
		///     Comps the get gizmos extra.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra()) 
				yield return gizmo;


			yield return _advPreview;
		}

		private void OpenSequencerInterface()
		{
			Window_Sequencer genebankWindow = new Window_Sequencer(this);
			Find.WindowStack.Add(genebankWindow);
		}


		/// <summary>
		///     Does the find.
		/// </summary>
		/// <param name="worker">The worker.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		protected override void DoFind(Pawn worker)
		{
			if (_animalSequenced)
				return;

			if (_targetAnimal == null)
			{
				Log.Error($"calling DoFind on {parent.ThingID} which does not have a chosen animal!");
				return;
			}

			_scratchList.Clear();
			_scratchList.AddRange(_targetAnimal.GetAllMutationsFrom().Where(m => !_genebank.StoredMutations.Contains(m)));

			if (_scratchList.Count == 0)
			{
				Log.Warning("unable to find mutation to give!");
				ClearTarget();
				return;
			}

			MutationDef mutation = _scratchList.RandomElement();

			if (_genebank.TryAddToDatabase(new MutationGenebankEntry(mutation)))
			{
				TaggedString msg = MUTATION_GATHERED_LABEL.Translate(mutation.Named("mutation"),
																	 _targetAnimal.Named("animal")
																	);
				Messages.Message(msg, MessageTypeDefOf.PositiveEvent);
				if (_scratchList.Count - 1 == 0)
					ClearTarget();
			}
		}

		private void ClearTarget()
		{
			TaggedString msg = "PMNothingTaggable".Translate(_targetAnimal.LabelCap.Named("animal"));
			Messages.Message(msg, MessageTypeDefOf.NeutralEvent);
			_animalSequenced = true;
		}

		/// <inheritdoc/>
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look(ref _targetAnimal, "TargetAnimal");

			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (_targetAnimal != null)
					_animalSequenced = _targetAnimal.GetAllMutationsFrom().Where(m => !_genebank.StoredMutations.Contains(m)).Any() == false;
			}
		}

		/// <inheritdoc/>
		public override string CompInspectStringExtra()
		{
			if (_targetAnimal == null)
				return string.Empty;

			string text = "";
			if (lastScanTick > (float)(Find.TickManager.TicksGame - 30))
			{
				text += "UserSequenceAbility".Translate() + ": " + lastUserSpeed.ToStringPercent() + "\n";
			}

			if (_animalSequenced)
				return text + "SequencingComplete".Translate(_targetAnimal.LabelCap.Named("animal"));

			return text + "SequencingProgress".Translate(_targetAnimal.label.Named("animal")) + ": " + Progress.ToStringPercent();
		}

		/// <summary>
		/// Gets the current sequencing progress.
		/// </summary>
		public float Progress => daysWorkingSinceLastFinding / Props.scanFindGuaranteedDays;
	}
}