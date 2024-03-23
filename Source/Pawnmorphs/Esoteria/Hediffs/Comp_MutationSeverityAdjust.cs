// Comp_MutationSeverityAdjust.cs modified by Iron Wolf for Pawnmorph on 12/01/2019 8:55 AM
// last updated 12/01/2019  8:55 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Hediff comp that acts like severity per day but if affected by the 'Mutation Adaptability Stat'
	/// Replicates the behavior of <see cref="HediffComp_SeverityPerDay"/> instead of inheriting from it for performance reasons
	/// </summary>
	/// <seealso cref="Pawnmorph.Utilities.HediffCompBase{T}" />
	[UsedImplicitly]
	public class Comp_MutationSeverityAdjust : HediffComp, IStageChangeObserverComp
	{
		private const float EPSILON = 0.001f;
		private bool _halted;

		private float _offset;

		/// <summary>
		/// An additional offset on the maximum severity, for when things such as adaption cream increase it
		/// </summary>
		public float SeverityOffset
		{
			get => _offset;
			set => _offset = Mathf.Clamp(value, -1, 1);
		}

		private const string ADAPTED = "PMMutationAdaptedGreater";

		private static string _cachedTranslation;

		static string CachedTranslationStr
		{
			get
			{
				if (_cachedTranslation == null) _cachedTranslation = ADAPTED.Translate();
				return _cachedTranslation;
			}
		}

		/// <summary>
		/// Gets the comp label in brackets extra.
		/// </summary>
		/// <value>
		/// The comp label in brackets extra.
		/// </value>
		public override string CompLabelInBracketsExtra => _offset > 0.1f ? CachedTranslationStr : "";

		/// <summary>
		///     Gets the natural severity limit.
		/// </summary>
		/// this value is the value the attached hediff should reach if the pawn has had the mutation for an 'infinite' amount of time
		/// <value>
		///     The natural severity limit.
		/// </value>
		public float NaturalSeverityLimit => Props.GetNaturalSeverityLimitFor(Pawn);

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="Comp_MutationSeverityAdjust" /> is halted.
		/// </summary>
		/// <value>
		///     <c>true</c> if halted; otherwise, <c>false</c>.
		/// </value>
		public bool Halted
		{
			get => _halted;
			set
			{
				if (_halted != value)
				{
					foreach (Hediff_AddedMutation allSimilarMutation in GetAllSimilarMutations())
					{
						Comp_MutationSeverityAdjust sevComp = allSimilarMutation.SeverityAdjust;
						if (sevComp != null)
						{
							sevComp._halted =
								value; //use the private variable so we don't trigger an infinite loop of comps setting each other
							allSimilarMutation.Severity = parent.Severity; //make sure they have the same severity to 
						}
					}

					_halted = value;
				}
			}
		}

		

		/// <summary>
		///     Gets a value indicating whether the parent hediff should be removed.
		/// </summary>
		/// <value>
		///     <c>true</c> if [comp should remove]; otherwise, <c>false</c>.
		/// </value>
		public override bool CompShouldRemove => ShouldRemove.GetValue(300);

		/// <summary>
		///     Gets the change per day.
		/// </summary>
		/// <value>
		///     The change per day.
		/// </value>
		public float ChangePerDay => SeverityChangePerDay();

		[NotNull]
		private CompProperties_MutationSeverityAdjust Props
		{
			get
			{
				try
				{
					var p = (CompProperties_MutationSeverityAdjust)props;
					if (p == null)
						throw new InvalidOperationException($"props on {parent.def.defName} on {Pawn.Name} has not props");
					return p;
				}
				catch (InvalidCastException e) //just incase the properties get miss assigned somehow 
				{
					throw new
						InvalidCastException($"could not convert hediff comp props of type {props.GetType().Name} to {nameof(CompProperties_MutationSeverityAdjust)}",
											 e);
				}
			}
		}

		// These values only get recalculated every so often because of how expensive they are to calculate
		//TODO make some sort of unified caching system for stat lookups, easy way to cause lots of lag 
		private readonly TimedCache<bool> IsReverting;
		private readonly TimedCache<bool> ShouldRemove;

		// The speed at which this particular mutation will revert, in severity-per-day.
		// Only generated once so that it doesn't fluctuate during reversion
		private readonly Lazy<float> RandReversionSpeed;

		/// <summary>
		/// creates a new instance 
		/// </summary>
		public Comp_MutationSeverityAdjust()
		{
			IsReverting = new TimedCache<bool>(() => Pawn.health?.hediffSet?.HasHediff(MorphTransformationDefOf.PM_Reverting) == true);
			ShouldRemove = new TimedCache<bool>(() => IsReverting.GetValue(300) && parent.Severity <= 0);

			RandReversionSpeed = new Lazy<float>(GenerateRandomReversionSpeed);
		}

		/// <summary>
		///     called to save/load data for this comp.
		/// </summary>
		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref _halted, nameof(_halted));
			Scribe_Values.Look(ref _offset, nameof(SeverityOffset));
		}

		/// <summary>
		///     called when the parent is merged with a new hediff of the same type
		/// </summary>
		/// <param name="other">The other.</param>
		public override void CompPostMerged(Hediff other)
		{
			base.CompPostMerged(other);
			Restart(); //restart adjusting if halted
		}

		public void UpdateComp()
		{
			parent.Severity += SeverityChangePerDay() / 300f; // 60000 / 200
		}

		/// <summary>
		/// Called when the stage changes on the parent hediff
		/// </summary>
		public void OnStageChanged(HediffStage oldStage, HediffStage newStage)
		{
			CheckIfHalted();
		}


		/// <summary>
		///     manually purges all cached data to force the adjustment speed to be recalculated the next time it's called
		/// </summary>
		public void RecalcAdjustSpeed()
		{
			IsReverting.QueueUpdate();
			ShouldRemove.QueueUpdate();
		}

		/// <summary>
		///     restarts adjustment for this mutation if it was halted
		/// </summary>
		public void Restart()
		{
			Halted = false;
			RecalcAdjustSpeed();
		}

		/// <summary>
		///     get the change in severity per day
		/// </summary>
		/// <returns></returns>
		public virtual float SeverityChangePerDay()
		{
			if (IsReverting.GetValue(300))
			{
				return RandReversionSpeed.Value;
			}

			if (Halted) return 0;
			float maxSeverity = EffectiveMax;
			float minSeverity = Mathf.Min(maxSeverity, 0); //have the stat influence how high or low the severity can be
			float sMult = Props.statEffectMult * (maxSeverity + 1);
			float sevPerDay = Props.severityPerDay * sMult;
			//make sure the severity can only stay between the max and min 
			if (parent.Severity > maxSeverity) sevPerDay = Mathf.Min(0, sevPerDay);
			if (parent.Severity < minSeverity) sevPerDay = Mathf.Max(0, sevPerDay);


			return sevPerDay * Mathf.Max(StatsUtility.GetStat(Pawn, PMStatDefOf.MutagenSensitivity, 500) ?? 1, 0); //take the mutagen sensitivity stat into account 
		}


		internal float EffectiveMax => Mathf.Max(StatsUtility.GetStat(Pawn, PMStatDefOf.MutationAdaptability, 300) ?? 0, 1) + SeverityOffset;

		private void CheckIfHalted()
		{
			if (parent.CurStage is MutationStage mStage)
			{
				float stopChance = mStage.stopChance * GetChanceMult() * StatsUtility.GetStat(Pawn, PMStatDefOf.MutationHaltChance, 300) ?? 0;

				if (stopChance < 0.01f) return;
				if (Rand.Value < stopChance) Halted = true;
			}
		}

		[NotNull]
		private IEnumerable<Hediff_AddedMutation> GetAllSimilarMutations()
		{
			List<Hediff> hediffSet = Pawn.health?.hediffSet?.hediffs;
			foreach (Hediff_AddedMutation hediff in hediffSet.MakeSafe().OfType<Hediff_AddedMutation>())
				if (hediff != parent && hediff.def == parent.def)
					yield return hediff;
		}

		private float GetChanceMult()
		{
			float sVal = StatsUtility.GetStat(Pawn, PMStatDefOf.MutationAdaptability, 300) ?? 0;

			float r = MutationUtilities.MaxMutationAdaptabilityValue - MutationUtilities.MinMutationAdaptabilityValue;
			sVal = Mathf.Abs(MutationUtilities.AverageMutationAdaptabilityValue - sVal)
				 / r
				 * 2; //converts the raw stat value to a value representing the distance the state 
					  //is from the default value, normalized for the total range of the stat 
			sVal -= r / 2; //shift the range of the modified stat value from [0,r/2] to [-r/2,0]
			sVal *= -2 / r; //shift the range again to [0,1], where a default stat value is 0, and either min or max is 0 
			sVal = Mathf.Max(sVal, 0); //make sure it doesn't go below zero, can happen if the default is not the center point of min and max
			return sVal;
		}

		/// <summary>
		/// Returns a debug string added to the debug tooltip for hediffs with this comp
		/// </summary>
		/// <returns>The debug string.</returns>
		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.CompDebugString());
			if (!Pawn.Dead)
			{
				stringBuilder.Append($"severity/day: {SeverityChangePerDay().ToString("F3")}, effective max: {EffectiveMax:F3}");

				if (_halted)
					stringBuilder.Append(" (halted)");
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		/// <summary>
		/// Generates the random reversion speed.  Each mutation will have a different reversion speed so that they don't
		/// all disappear at exactly the same time.  The distribution is weighted so that many mutations have similar speeds,
		/// but a significant number of outliers are slower.  This means morphs will start losing mutations quickly, but the
		/// remaining ones will stick around for a bit and take longer to fully revert.
		/// (just for flavor, they'll still be reverted eventually).
		/// 
		/// Reversion speed must be at least 0.8f, or there's a possibility mutations could stick around even after
		/// a full dose of reverter serum.
		/// </summary>
		/// <returns>The random reversion speed.</returns>
		private static float GenerateRandomReversionSpeed()
		{
			// Clamp between 0.81 and 1.3 to ensure mutations aren't disappearing too quickly or too slowly
			// The absolute minimum is 0.8, but making it slightly higher here to avoid mutations surviving reversion due to rounding errors
			return -Mathf.Clamp(RandUtilities.generateSkewNormalRandom(1.2f, 0.12f, -4f), 0.81f, 1.3f);
		}
	}

	/// <summary>
	///     comp properties for mutation adjust hediff comp
	/// </summary>
	/// <seealso cref="Pawnmorph.Utilities.HediffCompPropertiesBase{T}" />
	public class CompProperties_MutationSeverityAdjust : HediffCompProperties
	{
		private const float EPSILON = 0.01f;

		/// <summary>
		/// The severity change per day.
		/// </summary>
		public float severityPerDay;

		/// <summary>
		///     The stat effect multiplier
		/// </summary>
		/// values less then 1 will make the mutation adaptability stat have less of an effect
		/// values greater then 1 will increase it's effect
		public float statEffectMult = 1;

		/// <summary>
		///     Initializes a new instance of the <see cref="CompProperties_MutationSeverityAdjust" /> class.
		/// </summary>
		public CompProperties_MutationSeverityAdjust()
		{
			compClass = typeof(Comp_MutationSeverityAdjust);
		}

		/// <summary>
		///     Gets the natural severity limit for the given pawn
		/// </summary>
		/// this value is the value the attached hediff should reach if the pawn has had the mutation for an 'infinite' amount of time
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public float GetNaturalSeverityLimitFor([NotNull] Pawn pawn)
		{
			float sVal = pawn.GetStatValue(PMStatDefOf.MutationAdaptability);
			float sMul = (sVal + 1) * statEffectMult;
			if (sMul < -EPSILON) return sVal;
			if (sMul > EPSILON) return sVal + 1;
			return 0;
		}
	}
}