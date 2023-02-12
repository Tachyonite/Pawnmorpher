// MorphTf.cs created by Iron Wolf for Pawnmorph on 01/02/2020 1:53 PM
// last updated 01/02/2020  1:53 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     simple implementation of TransformationBase
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.TransformationBase" />
	public class MorphTf : TransformationBase
	{
		private const float BASE_MEAN = 4;
		private float _meanPerDay = BASE_MEAN;



		/// <summary>
		/// Gets a value indicating whether this instance should be removed.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance should be removed; otherwise, <c>false</c>.
		/// </value>
		public override bool ShouldRemove => base.ShouldRemove || MutationStatValue <= 0;

		private float? _statCache;

		/// <summary>
		/// Gets the mutation stat value.
		/// </summary>
		/// <value>
		/// The mutation stat value.
		/// </value>
		protected float MutationStatValue
		{
			get
			{
				if (_statCache == null) //getting the stat value is expensive so save the result 
				{
					_statCache = pawn.GetStatValue(PMStatDefOf.MutagenSensitivity);
				}

				return _statCache.Value;
			}
		}


		/// <summary>
		/// the expected number of mutations to happen in a single day 
		/// </summary>
		public override float MeanMutationsPerDay => CanMutatePawn ? _meanPerDay : 0;

		private List<MutationEntry> _allMutations;

		/// <summary>Gets the available mutations.</summary>
		/// <value>The available mutations.</value>
		public override IEnumerable<MutationEntry> AllAvailableMutations => _allMutations.MakeSafe();

		/// <summary>called after this hediff is added to the pawn</summary>
		/// <param name="dinfo">The dinfo.</param>
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			_allMutations = def.stages.OfType<TransformationStageBase>().SelectMany(s => s.GetEntries(pawn, this)).ToList();
		}

		/// <summary>
		/// Resets the mutation caches.
		/// </summary>
		public void ResetMutationCaches()
		{
			_allMutations = _allMutations ?? new List<MutationEntry>();
			_allMutations.Clear();
			_allMutations.AddRange(def.stages.OfType<TransformationStageBase>().SelectMany(s => s.GetEntries(pawn, this)));
		}

		/// <summary>Ticks this instance.</summary>
		public override void Tick()
		{
			base.Tick();

			if (pawn.IsHashIntervalTick(200))
			{
				_statCache = pawn.GetStatValue(PMStatDefOf.MutagenSensitivity); //recalculate the stat value every so often 
				_meanPerDay = GetBaseMutationRate(CurStage) * _statCache.Value;
			}
		}

		/// <summary>
		/// Gets the base mutation rate for this stage 
		/// </summary>
		/// gets the current mutation rate before the MutagenSensitivity or other stats are taken into account 
		/// <param name="currentStage">The current stage.</param>
		/// <returns></returns>
		protected virtual float GetBaseMutationRate(HediffStage currentStage)
		{
			if (currentStage is TransformationStageBase tfStage)
			{
				return tfStage.meanMutationsPerDay;
			}

			return BASE_MEAN;
		}

		/// <summary>Fills the part check list.</summary>
		/// the check list is a list of all parts in the parents body def in the order mutations should be added
		/// <param name="checkList">The check list.</param>
		protected override void FillPartCheckList(List<BodyPartRecord> checkList)
		{
			pawn.RaceProps.body.RandomizedSpreadOrder(checkList);
		}

		/// <summary>Gets the available the mutations from the given stage.</summary>
		/// <param name="currentStage">The current stage.</param>
		/// <returns></returns>
		protected override IEnumerable<MutationEntry> GetAvailableMutations(HediffStage currentStage)
		{
			if (currentStage is TransformationStageBase stage) return stage.GetEntries(pawn, this);
			return Enumerable.Empty<MutationEntry>();
		}

		/// <summary>
		/// returns true if there are ny mutations in this stage 
		/// </summary>
		/// <param name="stage"></param>
		/// <returns></returns>
		protected override bool AnyMutationsInStage(HediffStage stage)
		{
			if (stage is TransformationStageBase tStage)
			{
				return tStage.GetEntries(pawn, this).Any();
			}

			return false;
		}
	}
}