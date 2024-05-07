using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// An abstract class for hediffs that need to do things on stage changes.
	/// Also implements the IDescriptiveHediff interface
	/// </summary>
	public abstract class Hediff_StageChanges : Hediff_Descriptive
	{
		// Cache the stage index and stage, because CurStage/CurStageIndex both
		// calculate it every time they're called and it can get expensive
		private int cachedStageIndex = -1;
		[Unsaved] private HediffStage cachedStage;
		// If the severity is within these bounds, we are still in the same stage.  Otherwise we've had a stage change.
		[Unsaved] private float minStageSeverity = float.NegativeInfinity;
		[Unsaved] private float maxStageSeverity = float.PositiveInfinity;


		/// <summary>
		/// Whether the base Hediff tick is called.  Should be false for anything that doesn't need the vanilla tick behavior for
		/// performance reasons.
		/// </summary>
		protected bool TickBase = true;

		private List<IStageChangeObserverComp> observerComps;
		private IEnumerable<IStageChangeObserverComp> ObserverComps
		{
			get
			{
				if (observerComps == null)
					observerComps = comps.MakeSafe().OfType<IStageChangeObserverComp>().ToList();
				return observerComps;
			}
		}

		// CurStageIndex is kind of expensive to calculate, so use the cache when possible

		/// <summary>
		/// Gets the index of the current stage.
		/// </summary>
		/// <value>
		/// The index of the current stage.
		/// </value>
		public override int CurStageIndex => cachedStageIndex;

		/// <summary>
		/// Gets the current stage.
		/// </summary>
		/// <value>
		/// The current stage.
		/// </value>
		[CanBeNull]
		public override HediffStage CurStage => cachedStage;

		/// <summary>
		/// Called after the hediff is created, but before it's added to a pawn
		/// </summary>
		public override void PostMake()
		{
			base.PostMake();
			RecacheStage(base.CurStageIndex);
		}

		/// <summary>
		/// Ticks this instance.
		/// </summary>
		public override void Tick()
		{
			if (TickBase)
				base.Tick();
			else
				ageTicks++;

			// As long as we're within these bounds, the stage hasn't changed yet
			float sev = base.severityInt;
			if (sev < minStageSeverity || sev >= maxStageSeverity)
				UpdateStage();
		}

		internal void UpdateStage()
		{
			int newStageIndex = base.CurStageIndex; // Make sure to get the actual index from the base
			var oldStage = cachedStage;
			RecacheStage(newStageIndex);
			OnStageChanged(oldStage, cachedStage);

			foreach (var comp in ObserverComps)
				comp.OnStageChanged(oldStage, cachedStage);
		}

		/// <summary>
		/// Reloads the stage cache
		/// </summary>
		/// <param name="stageIndex">Stage index.</param>
		private void RecacheStage(int stageIndex)
		{
			cachedStageIndex = stageIndex;
			if (def?.stages != null)
			{
				var stages = def.stages;
				cachedStage = stages[cachedStageIndex];
				minStageSeverity = cachedStage?.minSeverity ?? float.NegativeInfinity;

				if (stages.Count > stageIndex + 1)
					maxStageSeverity = stages[stageIndex + 1]?.minSeverity ?? float.PositiveInfinity;
				else
					maxStageSeverity = float.PositiveInfinity;
			}
		}

		/// <summary>
		/// Called when the stage changes
		/// </summary>
		protected abstract void OnStageChanged([NotNull] HediffStage oldStage, [NotNull] HediffStage newStage);

		/// <summary>
		/// Exposes data to be saved/loaded from XML upon saving the game
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref cachedStageIndex, nameof(cachedStageIndex), base.CurStageIndex);

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				RecacheStage(cachedStageIndex);
			}
		}
	}
}
