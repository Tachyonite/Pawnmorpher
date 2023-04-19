// SapientAnimalMentalBreaker.cs modified by Iron Wolf for Pawnmorph on 12/05/2019 7:39 PM
// last updated 12/05/2019  7:39 PM

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     class for giving sapient animals mental breaks
	/// </summary>
	[StaticConstructorOnStartup]
	public class SapientAnimalMentalBreaker : IExposable
	{
		//various constants copied from rimworld 
		private const float ExtremeBreakMTBDays = 0.6f;

		private const float MajorBreakMTBDays = 1f;

		private const float MinorBreakMTBDays = 5f;

		private const int MinTicksBelowToBreak = 2000;
		private const int MENTAL_BREAK_HASH_INTERVAL = 150;

		private const int MinTicksSinceRecoveryToBreak = 15000;
		private const float MAJOR_BREAK_SCALAR = 0.5714286f;
		private const float EXTREME_BREAK_SCALAR = 0.1428571f;
		[NotNull] private static readonly List<Thought> _scratchList = new List<Thought>();

		//these three just look like debug helpers 
		private int _ticksBelowExtreme;
		private int _ticksBelowMajor;
		private int _ticksBelowMinor;

		private int _ticksUntilCanDoMentalBreak;

		[NotNull] private static readonly List<MentalBreakDef> _allSapientAnimalBreaks;

		static SapientAnimalMentalBreaker()
		{
			_allSapientAnimalBreaks = DefDatabase<MentalBreakDef>
										  .AllDefsListForReading
										  .Where(d => d.GetModExtension<FormerHumanRestriction>()?.mustBeFormerHuman == true)
										  .ToList(); //grab all mental breaks marked for sapient animals and store them for later 



		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SapientAnimalMentalBreaker" /> class.
		/// </summary>
		// ReSharper disable once NotNullMemberIsNotInitialized
		public SapientAnimalMentalBreaker()
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SapientAnimalMentalBreaker" /> class.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		public SapientAnimalMentalBreaker([NotNull] Pawn pawn)
		{
			Pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
		}

		void IExposable.ExposeData()
		{
			Scribe_Values.Look(ref _ticksBelowMinor, nameof(_ticksBelowMinor));
			Scribe_Values.Look(ref _ticksBelowMajor, nameof(_ticksBelowMajor));
			Scribe_Values.Look(ref _ticksBelowExtreme, nameof(_ticksBelowExtreme));
			Scribe_Values.Look(ref _ticksUntilCanDoMentalBreak, nameof(_ticksUntilCanDoMentalBreak));
		}

		/// <summary>
		///     Gets a value indicating whether an extreme break is imminent.
		/// </summary>
		/// <value>
		///     <c>true</c> if an extreme break is imminent; otherwise, <c>false</c>.
		/// </value>
		public bool BreakExtremeIsImminent => Pawn.MentalStateDef == null && CurMood < BreakThresholdExtreme;

		/// <summary>
		///     Gets a value indicating whether  a major break is imminent.
		/// </summary>
		/// <value>
		///     <c>true</c> if a major break is imminent; otherwise, <c>false</c>.
		/// </value>
		public bool BreakMajorIsImminent =>
			Pawn.MentalStateDef == null && !BreakExtremeIsImminent && CurMood < BreakThresholdMajor;

		/// <summary>
		///     Gets a value indicating whether a minor break is imminent.
		/// </summary>
		/// <value>
		///     <c>true</c> if a minor break is imminent; otherwise, <c>false</c>.
		/// </value>
		public bool BreakMinorIsImminent => Pawn.MentalStateDef == null
										 && !BreakExtremeIsImminent
										 && !BreakMajorIsImminent
										 && CurMood < BreakThresholdMinor;

		/// <summary>
		///     Gets a value indicating whether an extreme break is approaching.
		/// </summary>
		/// <value>
		///     <c>true</c> if an extreme break is approaching; otherwise, <c>false</c>.
		/// </value>
		public bool BreakExtremeIsApproaching =>
			Pawn.MentalStateDef == null && !BreakExtremeIsImminent && CurMood < BreakThresholdExtreme + 0.1f;

		/// <summary>
		///     Gets the current mood.
		/// </summary>
		/// <value>
		///     The current mood.
		/// </value>
		public float CurMood
		{
			get
			{
				if (Pawn.needs.mood == null) return 0.5f;
				return Pawn.needs.mood.CurLevel;
			}
		}


		/// <summary>
		///     Gets the break threshold extreme.
		/// </summary>
		/// <value>
		///     The break threshold extreme.
		/// </value>
		public float BreakThresholdExtreme => Pawn.GetStatValue(StatDefOf.MentalBreakThreshold) * EXTREME_BREAK_SCALAR;

		/// <summary>
		///     Gets the break threshold major.
		/// </summary>
		/// <value>
		///     The break threshold major.
		/// </value>
		public float BreakThresholdMajor => Pawn.GetStatValue(StatDefOf.MentalBreakThreshold) * MAJOR_BREAK_SCALAR;

		/// <summary>
		///     Gets the break threshold minor.
		/// </summary>
		/// <value>
		///     The break threshold minor.
		/// </value>
		public float BreakThresholdMinor =>
			Pawn.GetStatValue(StatDefOf.MentalBreakThreshold);


		/// <summary>
		///     Gets all sapient animal mental breaks.
		/// </summary>
		/// <value>
		///     All sapient animal mental breaks.
		/// </value>
		[NotNull]
		public static IEnumerable<MentalBreakDef> AllSapientAnimalMentalBreaks => _allSapientAnimalBreaks;

		/// <summary>
		///     Gets the pawn this thing is attached to.
		/// </summary>
		/// <value>
		///     The pawn.
		/// </value>
		[NotNull]
		public Pawn Pawn { get; }

		/// <summary>
		///     Gets a value indicating whether this instance can do random mental breaks.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can do random mental breaks; otherwise, <c>false</c>.
		/// </value>
		private bool CanDoRandomMentalBreaks => (Pawn.Spawned || Pawn.IsCaravanMember()) && Pawn.GetQuantizedSapienceLevel() != SapienceLevel.PermanentlyFeral;

		private MentalBreakIntensity CurrentDesiredMoodBreakIntensity
		{
			get
			{
				if (_ticksBelowExtreme >= MinTicksBelowToBreak) return MentalBreakIntensity.Extreme;
				if (_ticksBelowMajor >= MinTicksBelowToBreak) return MentalBreakIntensity.Major;
				if (_ticksBelowMinor >= MinTicksBelowToBreak) return MentalBreakIntensity.Minor;
				return MentalBreakIntensity.None;
			}
		}

		/// <summary>
		/// Gets the current possible mood breaks.
		/// </summary>
		/// <value>
		/// The current possible mood breaks.
		/// </value>
		[NotNull]
		public IEnumerable<MentalBreakDef> CurrentPossibleMoodBreaks
		{
			get
			{
				MentalBreakIntensity intensity;

				for (intensity = CurrentDesiredMoodBreakIntensity; intensity > MentalBreakIntensity.None; intensity--)
				{
					IEnumerable<MentalBreakDef> breaks =
						AllSapientAnimalMentalBreaks.Where(d => d.intensity == intensity && d.Worker.BreakCanOccur(Pawn));
					var counter = 0;
					foreach (MentalBreakDef mentalBreakDef in breaks)
					{
						yield return mentalBreakDef;
						counter++;
					}

					if (counter > 0) break; //only return the highest intensity ones 
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="intensity">The intensity.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public float GetMentalBreakThresholdFor(MentalBreakIntensity intensity)
		{
			switch (intensity)
			{
				case MentalBreakIntensity.Extreme:
					return BreakThresholdExtreme;
				case MentalBreakIntensity.Major:
					return BreakThresholdMajor;
				case MentalBreakIntensity.Minor:
					return BreakThresholdMinor;
				case MentalBreakIntensity.None:
				default:
					throw new InvalidEnumArgumentException(nameof(intensity), (int)intensity, typeof(MentalBreakIntensity));
			}
		}

		/// <summary>
		///     Notifies this instance that the pawn recovered from mental break.
		/// </summary>
		public void NotifyRecoveredFromMentalBreak()
		{
			_ticksUntilCanDoMentalBreak = MinTicksSinceRecoveryToBreak;
		}

		/// <summary>
		///     Ticks this instance.
		/// </summary>
		public void Tick()
		{
			if (_ticksUntilCanDoMentalBreak > 0 && Pawn.Awake()) _ticksUntilCanDoMentalBreak--;

			if (CanDoRandomMentalBreaks && Pawn.MentalStateDef == null && Pawn.IsHashIntervalTick(MENTAL_BREAK_HASH_INTERVAL))
			{

				if (!DebugSettings.enableRandomMentalStates) return;

				if (CurMood < BreakThresholdExtreme)
					_ticksBelowExtreme += 150;
				else
					_ticksBelowExtreme = 0;
				if (CurMood < BreakThresholdMajor)
					_ticksBelowMajor += 150;
				else
					_ticksBelowMajor = 0;
				if (CurMood < BreakThresholdMinor)
					_ticksBelowMinor += 150;
				else
					_ticksBelowMinor = 0;
				if (TestMoodMentalBreak() && TryDoRandomMoodCausedMentalBreak()) return;

				if (Pawn.story?.traits?.allTraits != null)
				{
					List<Trait> allTraits = Pawn.story.traits.allTraits;
					foreach (Trait trait in allTraits)
						if (trait.CurrentData.MentalStateGiver.CheckGive(Pawn, 150))
							return;
				}
			}
		}

		/// <summary>
		///     Tries the do random mood caused mental break.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool TryDoRandomMoodCausedMentalBreak()
		{

			if (!CanDoRandomMentalBreaks || Pawn.Downed || !Pawn.Awake() || Pawn.InMentalState) return false;
			if (Pawn.Faction != Faction.OfPlayer && CurrentDesiredMoodBreakIntensity != MentalBreakIntensity.Extreme)
				return false;
			MentalBreakDef mentalBreakDef;
			if (!CurrentPossibleMoodBreaks.TryRandomElementByWeight(d => d.Worker.CommonalityFor(Pawn), out mentalBreakDef)
			) return false;
			Thought thought = RandomFinalStraw();
			string text = "MentalStateReason_Mood".Translate();
			if (thought != null) text = text + "\n\n" + "FinalStraw".Translate(thought.LabelCap);
			return mentalBreakDef.Worker.TryStart(Pawn, text, true);
		}

		private int GetTicksBelowBreak(MentalBreakIntensity intensity)
		{
			switch (intensity)
			{
				case MentalBreakIntensity.None:
					return -1; //none is invalid, so return an invalid result 
				case MentalBreakIntensity.Minor:
					return _ticksBelowMinor;
				case MentalBreakIntensity.Major:
					return _ticksBelowMajor;
				case MentalBreakIntensity.Extreme:
					return _ticksBelowExtreme;
				default:
					throw new ArgumentOutOfRangeException(nameof(intensity), intensity, null);
			}
		}

		private Thought RandomFinalStraw()
		{
			Pawn.needs.mood.thoughts.GetAllMoodThoughts(_scratchList);
			var num = 0f;
			for (var i = 0; i < _scratchList.Count; i++)
			{
				float num2 = _scratchList[i].MoodOffset();
				if (num2 < num) num = num2;
			}

			float maxMoodOffset = num * 0.5f;
			Thought result = null;
			(from x in _scratchList
			 where x.MoodOffset() <= maxMoodOffset
			 select x).TryRandomElementByWeight(x => -x.MoodOffset(), out result);
			_scratchList.Clear();
			return result;
		}

		private bool TestMoodMentalBreak()
		{
			if (_ticksUntilCanDoMentalBreak > 0) return false;
			if (_ticksBelowExtreme > 2000) return Rand.MTBEventOccurs(ExtremeBreakMTBDays, 60000f, 150f);
			if (_ticksBelowMajor > 2000) return Rand.MTBEventOccurs(MajorBreakMTBDays, 60000f, 150f);
			return _ticksBelowMinor > 2000 && Rand.MTBEventOccurs(MinorBreakMTBDays, 60000f, 150f);
		}
	}
}