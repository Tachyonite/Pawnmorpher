// TaggingInjector.cs created by Iron Wolf for Pawnmorph on 02/26/2021 9:21 AM
// last updated 02/26/2021  9:21 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// </summary>
	/// <seealso cref="Verse.HediffWithComps" />
	public class TaggingInjector : HediffWithComps
	{
		/// <summary>
		/// </summary>
		public enum Outcome
		{
			/// <summary>
			///     give partial mutations
			/// </summary>
			Partials,

			/// <summary>
			///     give a morph chain
			/// </summary>
			MorphChain,

			/// <summary>
			///     give some buildup
			/// </summary>
			Buildup,

			/// <summary>
			///     instant chaomorph tf
			/// </summary>
			ChaomorphTf
		}

		private const float CHAO_CHANCE_INCREASE = 0.1f;
		[NotNull] private static readonly float[] _entries;

		[NotNull] private static readonly Outcome[] _outcomes;


		private static readonly FloatRange _buildupRange = new FloatRange(0.1f, 0.6f);

		private static IntRange _partialCountRange = new IntRange(1, 6);

		[NotNull] private static readonly List<MorphDef> _validMorphs;


		private bool _shouldRemove;

		private List<float> _weights;

		static TaggingInjector()
		{
			_validMorphs = MorphDef.AllDefs.Where(m => !m.IsChimera()
													&& !m.Restricted
													&& m.fullTransformation != null
													&& m.partialTransformation != null)
								   .ToList();


			_outcomes = Enum.GetValues(typeof(Outcome)).Cast<Outcome>().ToArray();
			_entries = new float[_outcomes.Length];

			for (var i = 0; i < _entries.Length; i++) _entries[i] = 0.01f; //default 

			_entries[(int)Outcome.Partials] = 0.25f;
			_entries[(int)Outcome.Buildup] = 0.25f;
			_entries[(int)Outcome.MorphChain] = 0.20f;
			_entries[(int)Outcome.ChaomorphTf] = 0.05f;

			//normalize 
			float sum = 0;
			for (var i = 0; i < _entries.Length; i++) sum += _entries[i];

			for (var i = 0; i < _entries.Length; i++) _entries[i] /= sum;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TaggingInjector"/> class.
		/// </summary>
		public TaggingInjector()
		{
			_weights = _entries.ToList();
		}

		/// <summary>
		///     Gets a value indicating whether [should remove].
		/// </summary>
		/// <value>
		///     <c>true</c> if [should remove]; otherwise, <c>false</c>.
		/// </value>
		public override bool ShouldRemove => base.ShouldRemove || _shouldRemove;

		/// <summary>
		///     Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _shouldRemove, nameof(ShouldRemove));
			Scribe_Collections.Look(ref _weights, "weights", LookMode.Value);

			if (Scribe.mode == LoadSaveMode.PostLoadInit && _weights == null) _weights = _entries.ToList();
		}

		/// <summary>
		///     Posts the add.
		/// </summary>
		/// <param name="dinfo">The dinfo.</param>
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			DoOutcome();
		}

		/// <summary>
		/// Tries the merge with.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public override bool TryMergeWith(Hediff other)
		{
			if (base.TryMergeWith(other))
			{
				_weights[(int)Outcome.ChaomorphTf] += CHAO_CHANCE_INCREASE;
				DoOutcome();
				return true;
			}

			return false;
		}

		private void DoOutcome()
		{
			Outcome outcome = PickOutcome();


			switch (outcome)
			{
				case Outcome.Partials:
					GivePartial();
					break;
				case Outcome.MorphChain:
					GiveMorph();
					break;
				case Outcome.Buildup:
					GiveBuildup();
					break;
				case Outcome.ChaomorphTf:
					ChaoTf();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ChaoTf()
		{
			HediffDef chaoTfDeff = MorphTransformationDefOf.FullRandomTFAnyOutcome;
			IPawnTransformer tf = chaoTfDeff.stages.OfType<IPawnTransformer>().FirstOrDefault();
			if (tf == null)
			{
				tf = chaoTfDeff.stages.SelectMany(s => s.hediffGivers.MakeSafe()).OfType<IPawnTransformer>().FirstOrDefault();
			}
			if (tf != null)
			{
				_shouldRemove = true;
				tf.TransformPawn(pawn, this);
			}
			else
			{
				Log.Warning("could not find ");
			}
		}

		private void GiveBuildup()
		{
			MutagenicBuildupUtilities.AdjustMutagenicBuildup(def, pawn, _buildupRange.RandomInRange);
		}

		private void GiveMorph()
		{
			MorphDef morph = PickMorph();
			Hediff hDiff = HediffMaker.MakeHediff(morph.fullTransformation, pawn);
			pawn.health?.AddHediff(hDiff);
		}

		private void GivePartial()
		{
			MorphDef morph = PickMorph();
			Hediff partialH = HediffMaker.MakeHediff(morph.partialTransformation, pawn);
			var pCount = partialH.TryGetComp<HediffComp_Single>();
			if (pCount != null) pCount.stacks = _partialCountRange.RandomInRange;

			pawn.health?.AddHediff(partialH);
		}

		private static MorphDef PickMorph()
		{
			return _validMorphs.RandElement();
		}


		private Outcome PickOutcome()
		{
			return _outcomes.RandomElementByWeight(w => _weights[(int)w]);
		}
	}
}