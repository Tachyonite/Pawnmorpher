// FormerHumanUtilities.cs modified by Iron Wolf for Pawnmorph on 12/08/2019 7:56 AM
// last updated 12/08/2019  7:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.FormerHumans;
using Pawnmorph.Hediffs;
using Pawnmorph.HPatches;
using Pawnmorph.Hybrids;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Pawnmorph.Utilities;
using Pawnmorph.Work;
using Prepatcher;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
	/// <summary>
	///     static class containing various former human utilities
	/// </summary>
	[StaticConstructorOnStartup]
	public static class FormerHumanUtilities
	{
		/// <summary>
		///     manhunter chances below this means that manhunter tf is disabled
		/// </summary>
		public const float MANHUNTER_EPSILON = 0.01f;

		/// <summary>
		/// The minimum biological age for a former human's human form
		/// TODO - Support children if biotech is installed
		/// </summary>
		public const float MIN_FORMER_HUMAN_AGE = 17f;


		private const float
			FORMER_HUMAN_FILTH_ADJ =
				1f; //at 0  former humans make the same filth as regular animals at 1 they make the same filth as humans 

		private const float ANIMALISTIC_FILTH_AMOUNT = 2; //animalistic humanoids make the same amount of filth as dogs 
		private const float MAX_SAPIENCE_REDUCE_AMOUNT = 0.2f;

		/// <summary>
		///     these are the minimum sapience levels needed to fall withing a given enum level
		/// </summary>
		[NotNull] private static readonly float[] _sapienceThresholds;

		[NotNull] private static readonly List<(SapienceLevel sapienceLevel, float threshold)> _cachedThresholds;

		[NotNull] private static readonly List<MutationDef> _mScratchList = new List<MutationDef>();

		[NotNull] private static readonly List<RulePackDef> _randomNameGenerators;
		private static readonly float _humanFilthAmount;

		[NotNull] private static readonly List<BodyPartRecord> _brScratchList = new List<BodyPartRecord>();


		[NotNull] private static readonly List<PawnKindDef> _allRegularFormerHumanPawnKinds;
		[NotNull] private static readonly List<PawnKindDef> _allResrictedFormerHumanPawnKinds;
		[NotNull] private static readonly List<PawnKindDef> _allFormerHumanPawnKinds;

		[NotNull] private static readonly Dictionary<int, Intelligence> _intelligenceCache = new Dictionary<int, Intelligence>(100);



		static FormerHumanUtilities()
		{
			var values = new[]
			{
				SapienceLevel.Sapient,
				SapienceLevel.MostlySapient,
				SapienceLevel.Conflicted
			};

			float delta = 1f / (values.Length + 1);
			float counter = 1;
			_sapienceThresholds = new float[Enum.GetValues(typeof(SapienceLevel)).Length];

			foreach (SapienceLevel sapienceLevel in values
			) //split up the level thresholds evenly between 1,0 starting at sapient 
			{
				counter -= delta;
				_sapienceThresholds[(int)sapienceLevel] = counter;
			}

			_sapienceThresholds[values.Length] = (_sapienceThresholds[values.Length - 1] + 0) / 12f;

			_sapienceThresholds[values.Length + 1] = 0;


			AllSapienceLevels = Enum.GetValues(typeof(SapienceLevel)).OfType<SapienceLevel>().ToList();

			MutationTraits = new[] //TODO mod extension on traits to specify which ones can carry over? 
            {
				TraitDefOf.BodyPurist,
				PMTraitDefOf.MutationAffinity,
				TraitDefOf.Nudist
			};


			_cachedThresholds = new List<(SapienceLevel sapienceLevel, float threshold)>();
			for (var index = 0; index < _sapienceThresholds.Length; index++)
			{
				float sapienceThreshold = _sapienceThresholds[index];
				_cachedThresholds.Add(((SapienceLevel)index, sapienceThreshold));
			}


			//grab random names from factions 
			_randomNameGenerators = new List<RulePackDef>();
			foreach (FactionDef factionDef in DefDatabase<FactionDef>.AllDefs)
			{
				if (!factionDef.humanlikeFaction || factionDef.hidden || factionDef.factionNameMaker == null) continue;
				_randomNameGenerators.Add(factionDef.factionNameMaker);
			}


			_humanFilthAmount = ThingDefOf.Human.statBases?.FirstOrDefault(s => s.stat == StatDefOf.FilthRate)?.value
							 ?? StatDefOf.FilthRate.defaultBaseValue;

			Giver_RecruitSapientAnimal.ResetStaticData();

			_allRegularFormerHumanPawnKinds = new List<PawnKindDef>();
			_allResrictedFormerHumanPawnKinds = new List<PawnKindDef>();
			_allFormerHumanPawnKinds = new List<PawnKindDef>();

			if (PawnmorpherMod.Settings.animalBlacklist == null)
				PawnmorpherMod.Settings.animalBlacklist = GetDefaultBlockList();

			CacheValidFormerHumans();

			PawnmorphGameComp.OnClear += OnClear;
		}

		private static void OnClear(PawnmorphGameComp obj)
		{
			_intelligenceCache.Clear();
		}

		/// <summary>
		/// Determines whether [is valid former human] [the specified forced].
		/// </summary>
		/// <param name="pawnKind">Kind of the pawn.</param>
		/// <param name="allowDisabled">Include disabled animals.</param>
		/// <param name="allowRestricted">Include restricted animals.</param>
		/// <returns>
		///   <c>true</c> if [is valid former human] [the specified forced]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsValidFormerHuman(this ThingDef pawnKind, bool allowRestricted = true, bool allowDisabled = false)
		{
			if (pawnKind.race == null)
				return false;

			// Must be an animal
			if (pawnKind.race.Animal == false)
				return false;

			// Dryads are not controllable 
			if (pawnKind.race.Dryad)
				return false;

			// XML Patched as not valid former human.
			if (pawnKind.GetModExtension<FormerHumanSettings>()?.neverFormerHuman == true)
				return false;

			// If restricted is not allowed, then filter away special chaomorphs.
			ChaomorphExtension chaoExtension = pawnKind.GetModExtension<ChaomorphExtension>();
			if (chaoExtension != null && allowRestricted == false)
			{
				// Only ordinary chaomorphs are allowed normally.
				if (chaoExtension.chaoType != ChaomorphType.Chaomorph)
					return false;

				// Chamorphs with a selection weight of less than 0 is not allowed.
				if (chaoExtension.selectionWeight < 0)
					return false;
			}

			// Check if manually blocked
			if (PawnmorpherMod.Settings.animalBlacklist.TryGetValue(pawnKind.defName, out FormerHumanRestrictions restriction))
			{
				if (allowRestricted == false && restriction == FormerHumanRestrictions.Restricted)
					return false;


				if (restriction == FormerHumanRestrictions.Disabled)
				{
					// If animal is disabled but has a related morph, then count as restricted.
					// They could have disabled the animal and then added a sub-mod which needs it for a morph.
					if (allowRestricted && pawnKind.GetMorphOfRace() != null)
						return true;

					if (allowDisabled == false)
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Caches the valid former humans. See <see cref="AllRegularFormerHumanPawnkindDefs"/>
		/// </summary>
		public static void CacheValidFormerHumans()
		{
			_allRegularFormerHumanPawnKinds.Clear();
			_allRegularFormerHumanPawnKinds.AddRange(DefDatabase<PawnKindDef>.AllDefsListForReading.Where(p => p.race.IsValidFormerHuman(allowRestricted: false)));


			_allResrictedFormerHumanPawnKinds.Clear();
			_allResrictedFormerHumanPawnKinds.AddRange(DefDatabase<PawnKindDef>.AllDefsListForReading.Where(p => p.race.IsValidFormerHuman(allowRestricted: true) && _allRegularFormerHumanPawnKinds.Contains(p) == false));

			_allFormerHumanPawnKinds.Clear();
			_allFormerHumanPawnKinds.AddRange(_allRegularFormerHumanPawnKinds);
			_allFormerHumanPawnKinds.AddRange(_allResrictedFormerHumanPawnKinds);
		}


		/// <summary>
		/// a list of all pawnkind defs that can be former humans .
		/// </summary>
		/// <value>
		/// All regular former human pawnkind defs.
		/// </value>
		[NotNull]
		public static IReadOnlyList<PawnKindDef> AllRegularFormerHumanPawnkindDefs => _allRegularFormerHumanPawnKinds;

		/// <summary>
		/// a list of all restricted pawnkind defs that can be former humans .
		/// </summary>
		/// <value>
		/// All restricted former human pawnkind defs.
		/// </value>
		[NotNull]
		public static IReadOnlyList<PawnKindDef> AllRestrictedFormerHumanPawnkindDefs => _allResrictedFormerHumanPawnKinds;

		/// <summary>
		/// a list of all pawnkind defs that can be former humans including both restricted and normal.
		/// </summary>
		/// <value>
		/// All former human pawnkind defs.
		/// </value>
		[NotNull]
		public static IReadOnlyList<PawnKindDef> AllFormerHumanPawnkindDefs => _allFormerHumanPawnKinds;

		/// <summary>
		///     the base chance for a neutral or hostile pawn to go manhunter when transformed
		/// </summary>
		public static float BaseManhunterTfChance =>
			LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().manhunterTfChance;

		/// <summary>
		///     if manhunter transformation is enabled
		/// </summary>
		public static bool ManhunterTfEnabled => BaseManhunterTfChance > MANHUNTER_EPSILON;

		/// <summary>
		///     the chance for a friendly pawn to go manhunter when transformed
		/// </summary>
		public static float BaseFriendlyManhunterTfChance => ManhunterTfEnabled
																 ? LoadedModManager
																  .GetMod<PawnmorpherMod>()
																  .GetSettings<PawnmorpherSettings>()
																  .friendlyManhunterTfChance
																 : 0;


		/// <summary>
		///     Gets all sapience levels.
		/// </summary>
		/// <value>
		///     All sapience levels.
		/// </value>
		public static IReadOnlyList<SapienceLevel> AllSapienceLevels { get; }

		/// <summary>
		///     Gets the sapience level thresholds.
		/// </summary>
		/// <value>
		///     The sapience level thresholds.
		/// </value>
		[NotNull]
		public static IEnumerable<(SapienceLevel sapienceLevel, float threshold)> SapienceLevelThresholds => _cachedThresholds;

		/// <summary>
		///     Gets all former humans on all maps
		/// </summary>
		/// <value>
		///     All maps player former humans.
		/// </value>
		[NotNull]
		public static IEnumerable<Pawn> AllMaps_FormerHumans
		{
			get
			{
				foreach (Pawn allMap in PawnsFinder.AllMaps)
					if (allMap.IsFormerHuman())
						yield return allMap;
			}
		}

		/// <summary>
		///     Gets all former humans on all maps, caravans and traveling transport pods that are alive
		/// </summary>
		/// <value>
		///     all former humans on all maps, caravans and traveling transport pods that are alive
		/// </value>
		[NotNull]
		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive
		{
			get
			{
				foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
					if (pawn.IsFormerHuman())
						yield return pawn;
			}
		}

		/// <summary>
		///     Gets all former humans belonging to the player
		/// </summary>
		/// <value>
		///     All player former humans.
		/// </value>
		[NotNull]
		public static IEnumerable<Pawn> AllPlayerFormerHumans
		{
			get
			{
				foreach (Pawn pawn in AllMapsCaravansAndTravelingTransportPods_Alive)
					if (pawn.Faction == Faction.OfPlayer)
						yield return pawn;
			}
		}


		/// <summary>
		///     Gets all sapient animals that are at risk of a minor break .
		/// </summary>
		/// <value>
		///     All sapient animals minor break risk.
		/// </value>
		[NotNull]
		public static IEnumerable<Pawn> AllSapientAnimalsMinorBreakRisk
		{
			get
			{
				foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
				{
					Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
					if (saComp?.MentalBreaker?.BreakMinorIsImminent == true) yield return allPlayerFormerHuman;
				}
			}
		}

		/// <summary>
		///     Gets all sapient animals that are at risk of a major break .
		/// </summary>
		/// <value>
		///     All sapient animals that are at risk of a major break .
		/// </value>
		[NotNull]
		public static IEnumerable<Pawn> AllSapientAnimalsMajorBreakRisk
		{
			get
			{
				foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
				{
					Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
					if (saComp?.MentalBreaker?.BreakMajorIsImminent == true) yield return allPlayerFormerHuman;
				}
			}
		}


		/// <summary>
		///     Gets all sapient animals at risk of an extreme break.
		/// </summary>
		/// <value>
		///     All sapient animals at risk of an extreme break.
		/// </value>
		[NotNull]
		public static IEnumerable<Pawn> AllSapientAnimalsExtremeBreakRisk
		{
			get
			{
				foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
				{
					Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
					if (saComp?.MentalBreaker?.BreakExtremeIsImminent == true) yield return allPlayerFormerHuman;
				}
			}
		}

		/// <summary>
		///     Gets the break alert label for sapient animals
		/// </summary>
		/// <value>
		///     The break alert label.
		/// </value>
		[NotNull]
		public static string BreakAlertLabel
		{
			get
			{
				int num = AllSapientAnimalsExtremeBreakRisk.Count();
				string text;
				if (num > 0)
				{
					text = "BreakRiskExtreme".Translate();
				}
				else
				{
					num = AllSapientAnimalsMajorBreakRisk.Count();
					if (num > 0)
					{
						text = "BreakRiskMajor".Translate();
					}
					else
					{
						num = AllSapientAnimalsMinorBreakRisk.Count();
						text = "BreakRiskMinor".Translate();
					}
				}

				if (num > 1) text = text + " x" + num.ToStringCached();
				return text;
			}
		}


		/// <summary>
		///     Gets the break alert explanation for sapient animals .
		/// </summary>
		/// <value>
		///     The break alert explanation.
		/// </value>
		[NotNull]
		public static string BreakAlertExplanation
		{
			get
			{
				var stringBuilder = new StringBuilder();
				if (AllSapientAnimalsExtremeBreakRisk.Any())
				{
					var stringBuilder2 = new StringBuilder();
					foreach (Pawn current in AllSapientAnimalsExtremeBreakRisk)
						stringBuilder2.AppendLine("    " + current.LabelShort);
					stringBuilder.Append("BreakRiskExtremeDesc".Translate(stringBuilder2));
				}

				if (AllSapientAnimalsMajorBreakRisk.Any())
				{
					if (stringBuilder.Length != 0) stringBuilder.AppendLine();
					var stringBuilder3 = new StringBuilder();
					foreach (Pawn current2 in AllSapientAnimalsMajorBreakRisk)
						stringBuilder3.AppendLine("    " + current2.LabelShort);
					stringBuilder.Append("BreakRiskMajorDesc".Translate(stringBuilder3));
				}

				if (AllSapientAnimalsMinorBreakRisk.Any())
				{
					if (stringBuilder.Length != 0) stringBuilder.AppendLine();
					var stringBuilder4 = new StringBuilder();
					foreach (Pawn current3 in AllSapientAnimalsMinorBreakRisk)
						stringBuilder4.AppendLine("    " + current3.LabelShort);
					stringBuilder.Append("BreakRiskMinorDesc".Translate(stringBuilder4));
				}

				stringBuilder.AppendLine();
				stringBuilder.Append("BreakRiskDescEnding".Translate());
				return stringBuilder.ToString();
			}
		}


		/// <summary>
		///     Gets the traits that transfer between original pawn and transformed pawn
		/// </summary>
		/// <value>
		///     The mutation traits.
		/// </value>
		[NotNull]
		public static IReadOnlyList<TraitDef> MutationTraits { get; }

		/// <summary>
		///     Determines whether this pawn can pass through fences
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if this this pawn can pass through fences otherwise, <c>false</c>.
		/// </returns>
		public static bool CanPassFences([NotNull] this Pawn pawn)
		{
			return !pawn.IsFenceBlocked();
		}

		/// <summary>
		///     Creates the merged animal request.
		/// </summary>
		/// <param name="kind">The kind.</param>
		/// <param name="originals">The originals.</param>
		/// <param name="faction">The faction.</param>
		/// <param name="context">The context.</param>
		/// <param name="fixedGender">The fixed gender.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     kind
		///     or
		///     originals
		/// </exception>
		public static PawnGenerationRequest CreateMergedAnimalRequest([NotNull] PawnKindDef kind,
																	  [NotNull] IEnumerable<Pawn> originals,
																	  Faction faction = null,
																	  PawnGenerationContext context =
																		  PawnGenerationContext.NonPlayer,
																	  Gender? fixedGender = null)
		{
			if (kind == null) throw new ArgumentNullException(nameof(kind));
			if (originals == null) throw new ArgumentNullException(nameof(originals));
			float avgOriginalAge = 0, avgOriginalLifeExpectancy = 0, maxChronoAge = 0;
			var counter = 0;
			foreach (Pawn oPawn in originals)
			{
				counter++;
				avgOriginalAge += oPawn.ageTracker.AgeBiologicalYears;
				avgOriginalLifeExpectancy += oPawn.RaceProps.lifeExpectancy;

				if (maxChronoAge < oPawn.ageTracker.AgeChronologicalYears)
					maxChronoAge = oPawn.ageTracker.AgeChronologicalYears;
			}

			avgOriginalLifeExpectancy /= counter;
			avgOriginalAge /= counter;

			float newAge =
				TransformerUtility.ConvertAge(avgOriginalAge, avgOriginalLifeExpectancy, kind.RaceProps.lifeExpectancy);
			return new PawnGenerationRequest(kind, faction, context, fixedGender: fixedGender, fixedBiologicalAge: newAge,
											 fixedChronologicalAge: maxChronoAge);
		}

		/// <summary>
		///     Creates the sapient animal generation request.
		/// </summary>
		/// <param name="kind">The kind.</param>
		/// <param name="original">The original.</param>
		/// <param name="faction">The faction.</param>
		/// <param name="context">The context.</param>
		/// <param name="fixedGender">The fixed gender.</param>
		/// <returns></returns>
		public static PawnGenerationRequest CreateSapientAnimalRequest([NotNull] PawnKindDef kind, [NotNull] Pawn original,
																	   Faction faction = null,
																	   PawnGenerationContext context =
																		   PawnGenerationContext.NonPlayer,
																	   Gender? fixedGender = null)
		{
			float age = TransformerUtility.ConvertAge(original.RaceProps, kind.RaceProps, original.ageTracker.AgeBiologicalYears);
			return new PawnGenerationRequest(kind, faction, context, fixedBiologicalAge: age,
											 fixedChronologicalAge: original.ageTracker.AgeChronologicalYears,
											 fixedGender: fixedGender);
		}

		/// <summary>
		///     Finds the random prey for the given predator
		/// </summary>
		/// <param name="predator">The predator.</param>
		/// <returns></returns>
		public static Pawn FindRandomPreyFor(Pawn predator)
		{
			var resultsList = new List<Pawn>();
			if (predator.meleeVerbs.TryGetMeleeVerb(null) == null) return null;
			var flag = false;
			float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
			if (summaryHealthPercent < 0.25f) flag = true;
			resultsList.Clear();

			resultsList.AddRange(predator.Map.mapPawns.AllPawnsSpawned);


			Pawn pawn = null;
			var num = 0f;
			bool tutorialMode = TutorSystem.TutorialMode;
			foreach (Pawn pawn2 in resultsList)
			{
				if (predator.GetRoom() != pawn2.GetRoom()) continue;
				if (predator == pawn2) continue;
				if (flag && !pawn2.Downed) continue;
				if (!FoodUtility.IsAcceptablePreyFor(predator, pawn2)) continue;
				if (!predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly)) continue;
				if (pawn2.IsForbidden(predator)) continue;
				if (tutorialMode && pawn2.Faction == Faction.OfPlayer) continue;
				float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2);
				if (!(preyScoreFor > num) && pawn != null) continue;
				num = preyScoreFor;
				pawn = pawn2;
			}

			resultsList.Clear();
			return pawn;
		}

		/// <summary>
		///     Generates the random human form of the given animal
		/// </summary>
		/// <param name="animal">The animal.</param>
		/// <param name="fixedFirstName">First name of the fixed.</param>
		/// <param name="fixedLastName">Last name of the fixed.</param>
		/// <param name="fixedOriginalGender">The fixed original gender.</param>
		/// <returns></returns>
		[Obsolete("Use " + nameof(FormerHumanPawnGenerator) + " instead")]
		public static Pawn GenerateRandomHumanForm(Pawn animal, string fixedFirstName = null, string fixedLastName = null,
												   Gender? fixedOriginalGender = null)
		{
			FHGenerationSettings settings = new FHGenerationSettings
			{
				FirstName = fixedFirstName,
				LastName = fixedLastName,
				Gender = fixedOriginalGender
			};
			return FormerHumanPawnGenerator.GenerateRandomHumanForm(animal, settings);
		}

		/// <summary>
		///     Generates the random unmerged humans for the given merged animal
		/// </summary>
		/// <param name="mergedAnimal">The animal.</param>
		/// <returns></returns>
		[Obsolete("Use " + nameof(FormerHumanPawnGenerator) + " instead")]
		public static (Pawn p1, Pawn p2) GenerateRandomUnmergedHuman(Pawn mergedAnimal)
		{
			return FormerHumanPawnGenerator.GenerateRandomUnmergedHumans(mergedAnimal);
		}

		/// <summary>
		///     Gets the filth stat, taking sapience into account.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public static float GetFilthStat([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			float baseStatValue = pawn.GetStatValue(StatDefOf.FilthRate);
			bool isFormerHuman = pawn.IsFormerHuman();
			if (!isFormerHuman && pawn.RaceProps.Animal) return baseStatValue;

			SapienceLevel sapienceLevel = pawn.GetQuantizedSapienceLevel() ?? SapienceLevel.Sapient;

			int inSapLevel = Mathf.Min(4, (int)sapienceLevel);
			float x, edge0, edge1;

			if (isFormerHuman)
			{
				x = (4 - inSapLevel) / 4f;
				edge0 = baseStatValue;
				edge1 = Mathf.Lerp(baseStatValue, _humanFilthAmount,
								   FORMER_HUMAN_FILTH_ADJ); //at FORMER_HUMAN_FILTH_ADJ = 0.5 this is the average between human and animal filth rates  
				x = MathUtilities.SmoothStep(0, 1, x);
			}
			else
			{
				x = inSapLevel / 4f;
				x = MathUtilities.SmoothStep(0, 1, x);
				edge0 = _humanFilthAmount;
				edge1 = ANIMALISTIC_FILTH_AMOUNT;
			}

			float statValue = Mathf.Lerp(edge0, edge1, x);

			return statValue;
		}

		
		private static Intelligence _placeholder = Intelligence.Animal;
		[PrepatcherField]
		[Prepatcher.DefaultValue((Intelligence)99)]
		public static ref Intelligence GetCachedIntelligence(this Pawn pawn)
		{
			if (_intelligenceCache.TryGetValue(pawn.thingIDNumber, out Intelligence value))
				_placeholder = value;
			else
				_placeholder = (Intelligence)99;

			return ref _placeholder;
		}


		/// <summary>
		///     Gets the intelligence of this pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		/// https://github.com/Zetrith/Prepatcher/wiki/Adding-fields
		public static Intelligence GetIntelligence(this Pawn pawn)
		{
			Intelligence intelligence = GetCachedIntelligence(pawn);
			if ((int)intelligence == 99)
				intelligence = pawn.RaceProps.intelligence;

			return intelligence;
		}

		/// <summary>
		/// Invalidates the cached intelligence level of the given pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		public static void InvalidateIntelligence([NotNull] this Pawn pawn)
		{
			Intelligence intelligence;
			SapienceTracker sTracker = pawn.GetSapienceTracker();
			if (sTracker != null)
				intelligence = sTracker.CurrentIntelligence;
			else
				intelligence = pawn.RaceProps.intelligence;

			GetCachedIntelligence(pawn) = intelligence;
			_intelligenceCache[pawn.thingIDNumber] = intelligence;
		}

		/// <summary>
		///     Gets the mid level.
		/// </summary>
		/// <param name="sapienceLevel">The sapience level.</param>
		/// <returns></returns>
		public static float GetMidLevel(this SapienceLevel sapienceLevel)
		{
			return (GetThreshold(sapienceLevel) + GetUpperThreshold(sapienceLevel)) / 2;
		}

		/// <summary>
		///     Gets the original pawn of the given former human.
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		/// <returns>the original pawn if it exists, otherwise null</returns>
		[CanBeNull]
		public static Pawn GetOriginalPawnOfFormerHuman([NotNull] Pawn formerHuman)
		{
			foreach (TransformedPawn tfPawn in Find.World.GetComponent<PawnmorphGameComp>().TransformedPawns)
				if (tfPawn.TransformedPawns.Contains(formerHuman))
					return tfPawn.OriginalPawns.FirstOrDefault();

			return null;
		}

		/// <summary>Gets the quantized sapience level.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>returns null if the pawn isn't a former human</returns>
		public static SapienceLevel? GetQuantizedSapienceLevel([NotNull] this Pawn pawn)
		{
			SapienceTracker tracker = pawn.GetSapienceTracker();
			if (tracker?.CurrentState == null) return null;
			return tracker.SapienceLevel;
		}

		/// <summary>Gets the quantized sapience level.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>returns null if the pawn isn't a former human</returns>
		public static float? GetSapienceLevel([NotNull] this Pawn pawn)
		{
			SapienceTracker tracker = pawn.GetSapienceTracker();
			if (tracker?.CurrentState == null) return null;
			return tracker.Sapience;
		}



		/// <summary>
		///     Gets the quantized sapience level.
		/// </summary>
		/// <param name="sapienceLevel">The raw sapience level.</param>
		/// <returns></returns>
		public static SapienceLevel GetQuantizedSapienceLevel(float sapienceLevel)
		{
			for (var index = 0; index < _sapienceThresholds.Length; index++)
			{
				float sapienceThreshold = _sapienceThresholds[index];
				if (sapienceLevel > sapienceThreshold) return (SapienceLevel)index;
			}

			return SapienceLevel.Feral;
		}

		/// <summary>
		///     Gets the current sapience state the pawn is in
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		[CanBeNull]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SapienceState GetSapienceState([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return pawn.GetSapienceTracker()?.CurrentState;
		}

		/// <summary>
		///     Gets the former human tracker.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		[CanBeNull]
		[PrepatcherField]
		[InjectComponent]
		public static SapienceTracker GetSapienceTracker([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));

			SapienceTracker tComp = CompCacher<SapienceTracker>.GetCompCached(pawn);
			return tComp;
		}


		/// <summary>
		///     Gets the sapience will debuff.
		/// </summary>
		/// <param name="sapienceLevel">The q sapience.</param>
		/// <returns></returns>
		public static float GetSapienceWillDebuff(SapienceLevel sapienceLevel)
		{
			return GetSapienceWillDebuff(_sapienceThresholds[(int)sapienceLevel]);
		}

		/// <summary>
		/// Gets the sapience will debuff.
		/// </summary>
		/// <param name="sapienceLevel">The sapience level.</param>
		/// <returns></returns>
		public static float GetSapienceWillDebuff(float sapienceLevel)
		{
			sapienceLevel -= _sapienceThresholds[(int)SapienceLevel.MostlyFeral];
			sapienceLevel /= (1.0f
							- _sapienceThresholds[(int)SapienceLevel.MostlyFeral]);

			sapienceLevel = Mathf.Clamp01(sapienceLevel); //[0,1] 0 is mostly feral 1 is sapient 

			return MathUtilities.SmoothStep(0.0f, 0.7f, 1 - sapienceLevel) * MAX_SAPIENCE_REDUCE_AMOUNT; //invert 1 is mostly feral 0 is sapient
		}

		/// <summary>
		///     Gets the sapient animal comp.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		[CanBeNull]
		public static Comp_SapientAnimal GetSapientAnimalComp([NotNull] this Pawn pawn)
		{
			return pawn.TryGetComp<Comp_SapientAnimal>();
		}


		/// <summary>
		///     Gets the threshold.
		/// </summary>
		/// <param name="sapienceLevel">The sapience level.</param>
		/// <returns></returns>
		public static float GetThreshold(this SapienceLevel sapienceLevel)
		{
			if (sapienceLevel == SapienceLevel.PermanentlyFeral || sapienceLevel == SapienceLevel.Feral) return 0;
			return _sapienceThresholds[(int)sapienceLevel];
		}

		/// <summary>
		///     Gets the upper threshold.
		/// </summary>
		/// <param name="sapienceLevel">The sapience level.</param>
		/// <returns></returns>
		public static float GetUpperThreshold(this SapienceLevel sapienceLevel)
		{
			if (sapienceLevel == SapienceLevel.Sapient) return 1;
			return _sapienceThresholds[(int)sapienceLevel - 1];
		}

		/// <summary>
		///     Gives the sapient animal the hunting thought.
		/// </summary>
		/// note: this always gives the default thoughts, the caller should
		/// first check if ideology is active and if the pawns ideo should override these 
		/// <param name="sapientAnimal">The sapient animal.</param>
		/// <param name="prey">The prey.</param>
		public static void GiveSapientAnimalHuntingThought([NotNull] Pawn sapientAnimal, [NotNull] Pawn prey)
		{
			ThoughtDef thoughtDef;




			if (sapientAnimal.GetMutationOutlook() == MutationOutlook.PrimalWish)
				thoughtDef = PMThoughtDefOf.SapientAnimalHuntingMemoryPrimalWish;
			else
				thoughtDef = PMThoughtDefOf.SapientAnimalHuntingMemory;

			sapientAnimal.TryGainMemory(thoughtDef);
		}

		/// <summary>
		///     Determines this pawn's sapience is in a special state like FormerHuman or Animalistic
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if this pawn's sapience is in a special state like FormerHuman or Animalistic; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasSapienceState([NotNull] this Pawn pawn)
		{
			return GetSapienceState(pawn) != null;
		}

		/// <summary>
		///     Initializes the transformed pawn with the given original pawn and sapience level
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="animal">The animal.</param>
		/// <param name="sapienceLevel">The sapience level.</param>
		/// <param name="backstoryOverride">The backstory override.</param>
		/// <exception cref="ArgumentNullException">
		///     original
		///     or
		///     animal
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		///     original
		///     or
		///     animal
		/// </exception>
		public static void InitializeTransformedPawn([NotNull] Pawn original, [NotNull] Pawn animal, float sapienceLevel,
													 BackstoryDef backstoryOverride = null)
		{
			if (original == null) throw new ArgumentNullException(nameof(original));
			if (animal == null) throw new ArgumentNullException(nameof(animal));

			if (original.Faction == Faction.OfPlayer && animal.Faction != original.Faction)
				animal.SetFaction(original.Faction);

			TransferEverything(original, animal, passionTransferMode: PawnTransferUtilities.SkillPassionTransferMode.Set);

			animal?.workSettings?.EnableAndInitializeIfNotAlreadyInitialized();

			TryAssignBackstoryToTransformedPawn(animal, original, backstoryOverride);
			var nC = animal.needs.TryGetNeed<Need_Control>();

			if (nC == null)
			{
				Log.Error(nameof(nC));
				return;
			}

			nC.SetInitialLevel(sapienceLevel);
			animal.InvalidateIntelligence();

			//nC.CurLevelPercentage = sapienceLevel;
			ResetTraining(animal);

			PawnComponentsUtility.AddAndRemoveDynamicComponents(animal);

			if (animal.needs == null)
			{
				Log.Error(nameof(animal.needs));
				return;
			}

			animal.needs.AddOrRemoveNeedsAsAppropriate();
		}

		/// <summary>
		/// Resets the training levels of the provided pawn to max if pawn has training component.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		public static void ResetTraining(Pawn pawn)
		{
			if (pawn.training == null)
				return;

			foreach (TrainableDef training in DefDatabase<TrainableDef>.AllDefs)
			{
				if (!pawn.training.CanBeTrained(training))
					continue;

				pawn.training.Train(training, null, true);
			}
		}

		/// <summary>
		///     Initializes the work settings for the given sapient animal
		/// </summary>
		/// <param name="sapientAnimal">The sapient animal.</param>
		/// <param name="workSettings">The pawn work settings.</param>
		/// <exception cref="ArgumentNullException">
		///     sapientAnimal
		///     or
		///     workSettings
		/// </exception>
		public static void InitializeWorkSettingsFor([NotNull] Pawn sapientAnimal, [NotNull] Pawn_WorkSettings workSettings)
		{
			if (sapientAnimal == null) throw new ArgumentNullException(nameof(sapientAnimal));
			if (workSettings == null) throw new ArgumentNullException(nameof(workSettings));
			var formerHumanExt = sapientAnimal.def.GetModExtension<FormerHumanSettings>();
			BackstoryDef backstoryDef = formerHumanExt?.backstory ?? BackstoryDefOf.FormerHumanNormal;
			foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
				if (backstoryDef.DisabledWorkTypes.Contains(workTypeDef))
					workSettings.SetPriority(workTypeDef, 0);
				else
					workSettings.SetPriority(workTypeDef, 3);
		}

		/// <summary>
		///     Determines whether this instance is an animal.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if the specified pawn is an animal; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAnimal([NotNull] this Pawn pawn)
		{
			return pawn.GetIntelligence() == Intelligence.Animal;
		}

		/// <summary>
		///     Determines whether this pawn is a colonist former human
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if this pawn is a colonist former human; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsColonistAnimal([NotNull] this Pawn pawn)
		{
			SapienceLevel? fhStatus = pawn.GetQuantizedSapienceLevel();
			if (fhStatus == null) return false;
			return IsColonistAnimal(fhStatus.Value);
		}

		/// <summary>
		///     Determines whether this status is for a colonist animal.
		/// </summary>
		/// <param name="fhStatus">The fh status.</param>
		/// <returns>
		///     <c>true</c> if this status is for a colonist animal; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static bool IsColonistAnimal(this SapienceLevel fhStatus)
		{
			switch (fhStatus)
			{
				case SapienceLevel.Sapient:
				case SapienceLevel.MostlySapient:
					return true;
				case SapienceLevel.Conflicted:
				case SapienceLevel.MostlyFeral:
				case SapienceLevel.Feral:
				case SapienceLevel.PermanentlyFeral:
					return false;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		/// <summary>
		///     Determines whether this pawn is blocked by fences
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if this pawn is blocked by fences; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFenceBlocked([NotNull] this Pawn pawn)
		{
			return pawn.RaceProps.FenceBlocked && !pawn.IsHumanlike();
		}

		/// <summary>
		/// Determines whether the given pawn is a former human.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="countPermanentlyFeral">if set to <c>true</c> permanently feral pawns count as former humans.</param>
		/// <returns>
		///   <c>true</c> if the given pawn is former human; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFormerHuman([NotNull] this Pawn pawn, bool countPermanentlyFeral = true)
		{
			SapienceState sState = pawn.GetSapienceState();
			return (sState?.IsFormerHuman == true) && (countPermanentlyFeral || sState.Tracker?.IsPermanentlyFeral == false);
		}


		/// <summary>
		///     Determines whether this instance is humanlike.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if the specified pawn is humanlike; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsHumanlike([NotNull] this Pawn pawn)
		{
			return pawn.GetIntelligence() == Intelligence.Humanlike;
		}


		/// <summary>
		///     Determines whether the specified pawn is a manhunter.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if the specified pawn is a manhunter; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static bool IsManhunter([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			MentalStateDef ms = pawn.MentalStateDef;
			return ms == MentalStateDefOf.ManhunterPermanent || ms == MentalStateDefOf.Manhunter;
		}

		/// <summary>
		///     If this pawn is a roamer or not.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsRoamer([NotNull] this Pawn pawn)
		{
			return pawn.RaceProps.Roamer && !pawn.IsHumanlike();
		}

		/// <summary>
		///     Determines whether this pawn is a sapient former human.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if this pawn is a sapient former human; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static bool IsSapientFormerHuman([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			SapienceState sapienceState = pawn.GetSapienceState();
			if (sapienceState?.IsFormerHuman != true) return false;
			return sapienceState.CurrentIntelligence == Intelligence.Humanlike;
		}


		/// <summary>
		///     Determines whether the given pawn is a tool user.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if the given pawn is a tool user ; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsToolUser([NotNull] this Pawn pawn)
		{
			return pawn.GetIntelligence() >= Intelligence.ToolUser;
		}

		/// <summary>Makes the animal sapient. including adding necessary comps, need, training, etc  </summary>
		/// <param name="original">The original.</param>
		/// <param name="animal">The animal.</param>
		/// <param name="sapienceLevel">The sapience level.</param>
		public static void MakeAnimalSapient([NotNull] Pawn original, [NotNull] Pawn animal, float sapienceLevel = 1)
		{
			if (animal.IsFormerHuman())
			{
				Log.Warning($"trying to make {original.Name} a former human twice!");
				return;
			}

			SapienceTracker sTracker = animal.GetSapienceTracker();

			if (sTracker == null)
			{
				Log.Error($"{animal.Name},{animal.def.defName} does not have a {nameof(SapienceTracker)} comp!");
				return;
			}

			sTracker.EnterState(SapienceStateDefOf.FormerHuman, sapienceLevel);

			InitializeTransformedPawn(original, animal, sapienceLevel);
		}


		/// <summary>
		///     Makes the animal sapient.
		/// </summary>
		/// <param name="animal">The animal.</param>
		/// <param name="sapienceLevel">The sapience level.</param>
		/// <param name="joinIfRelated">
		///     if set to <c>true</c> and the resulting pawn is related to a colonist have the animal join
		///     the colony.
		/// </param>
		/// <param name="backstoryOverride">The backstory override.</param>
		/// <param name="fixedFirstName">First name of the fixed.</param>
		/// <param name="fixedLastName">Last name of the fixed.</param>
		/// <param name="fixedOriginalGender">The fixed original gender.</param>
		public static void MakeAnimalSapient([NotNull] Pawn animal, float sapienceLevel = 1, bool joinIfRelated = true,
											 BackstoryDef backstoryOverride = null, string fixedFirstName = null,
											 string fixedLastName = null, Gender? fixedOriginalGender = null)
		{
			if (animal.IsFormerHuman())
			{
				Log.Warning($"trying to make {animal.Name} a former human twice!");
				return;
			}

			SapienceTracker sTracker = animal.GetSapienceTracker();

			if (sTracker == null)
			{
				Log.Error($"{animal.Name},{animal.def.defName} does not have a {nameof(SapienceTracker)} comp!");
				return;
			}

			FHGenerationSettings settings = new FHGenerationSettings()
			{
				FirstName = fixedFirstName,
				LastName = fixedLastName,
				Gender = fixedOriginalGender,
				ChronoAge = animal.ageTracker.AgeChronologicalYearsFloat
			};
			Pawn lPawn = FormerHumanPawnGenerator.GenerateRandomHumanForm(animal, settings);

			MorphDef morph = animal.def.TryGetBestMorphOfAnimal();

			if (morph != null)
			{
				if (morph.IsChimera())
					AddRandomMutationToPawn(lPawn);
				else
					MutationUtilities.AddAllMorphMutations(lPawn, morph, MutationUtilities.AncillaryMutationEffects.None)
									 .SetAllToNaturalMax();
			}

			MutationTracker mTracker = lPawn.GetMutationTracker();
			if (mTracker != null)
			{
				mTracker.RecalculateMutationInfluences();
				lPawn.CheckRace(false, false);
			}

			CleanupPawn(lPawn);
			sTracker.EnterState(SapienceStateDefOf.FormerHuman, sapienceLevel);
			PawnComponentsUtility.AddAndRemoveDynamicComponents(animal);

			TryAssignBackstoryToTransformedPawn(animal, lPawn, backstoryOverride);
			TransferEverything(lPawn, animal, passionTransferMode: PawnTransferUtilities.SkillPassionTransferMode.Set);
			animal?.workSettings?.EnableAndInitializeIfNotAlreadyInitialized();
			var inst = new TransformedPawnSingle
			{
				original = lPawn,
				animal = animal,
				mutagenDef = MutagenDefOf.defaultMutagen
			};


			var gameComp = Find.World.GetComponent<PawnmorphGameComp>();
			gameComp.AddTransformedPawn(inst);

			if (animal.needs == null)
			{
				Log.Error(nameof(animal.needs));
				return;
			}

			//now give the animal a name 
			SapienceLevel sapienceQLevel = GetQuantizedSapienceLevel(sapienceLevel);
			if (sapienceQLevel == SapienceLevel.Sapient || sapienceQLevel == SapienceLevel.MostlySapient)
				animal.Name = lPawn.Name;
			else
				animal.Name = new NameSingle(animal.LabelShort);

			animal.needs.AddOrRemoveNeedsAsAppropriate();
			var nC = animal.needs.TryGetNeed<Need_Control>();

			if (nC == null)
			{
				if (sapienceLevel > 0)
					Log.Error($"unable to set sapient level on {animal.Name} while trying to make the sapient because they have no control need");
				return;
			}

			nC.SetInitialLevel(sapienceLevel);

			// Offer to join the colony if they are wild and related to a colonist
			if (animal.Faction == null && animal.GetCorrectMap() != null)
				if (joinIfRelated)
				{
					RelatedFormerHumanUtilities.OfferJoinColonyIfRelated(animal);
				}

			if (animal.training == null) return;

			foreach (TrainableDef training in DefDatabase<TrainableDef>.AllDefs)
			{
				if (!animal.training.CanBeTrained(training)) continue;

				animal.training.Train(training, null, true);
			}
		}

		/// <summary>
		///     Makes the pawn permanently feral.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="NotImplementedException"></exception>
		public static void MakePermanentlyFeral([NotNull] Pawn pawn)
		{
			var comp = pawn.GetSapienceTracker();
			if (comp == null) return;
			if (comp.CurrentState?.StateDef.canGoPermanentlyFeral != true || comp.IsPermanentlyFeral) return;
			comp.MakePermanentlyFeral();
		}


		/// <summary>
		///     checks if Tameness the can decay on the given pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static bool TamenessCanDecay([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			SapienceLevel? sapienceLv = pawn.GetQuantizedSapienceLevel();
			if (sapienceLv == null || sapienceLv > SapienceLevel.Conflicted)
				return TrainableUtility.TamenessCanDecay(pawn.def);
			return false;
		}

		/// <summary>
		///     transfers all relevant information from the original pawn to the transformed pawn
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="transformedPawn">The pawn the original pawn was transformed into.</param>
		/// <param name="transferMode">The skill transfer mode.</param>
		/// <param name="passionTransferMode">The passion transfer mode.</param>
		public static void TransferEverything(Pawn original, Pawn transformedPawn,
											  PawnTransferUtilities.SkillTransferMode transferMode =
												  PawnTransferUtilities.SkillTransferMode.Max,
											  PawnTransferUtilities.SkillPassionTransferMode passionTransferMode =
												  PawnTransferUtilities.SkillPassionTransferMode.Ignore)
		{
			PawnTransferUtilities.TransferRelations(original, transformedPawn);
			PawnTransferUtilities.TransferAspects(original, transformedPawn);
			PawnTransferUtilities.TransferSkills(original, transformedPawn, transferMode, passionTransferMode);
			PawnTransferUtilities.TransferTraits(original, transformedPawn,
												 t => t.GetModExtension<TFTransferable>()?.CanTransfer(transformedPawn) == true);
			PawnTransferUtilities.TransferHediffs(original, transformedPawn,
												  h => h.def.GetModExtension<TFTransferable>()?.CanTransfer(transformedPawn)
													== true);
			PawnTransferUtilities.TransferThoughts(original, transformedPawn);
			PawnTransferUtilities.TransferInteractions(original, transformedPawn);
			PawnTransferUtilities.TransferQuestRelations(original, transformedPawn);
			if (ModLister.RoyaltyInstalled) PawnTransferUtilities.TransferFavor(original, transformedPawn);
			if (ModLister.IdeologyInstalled) PawnTransferUtilities.TransferIdeo(original, transformedPawn);
			PawnTransferUtilities.TransferAbilities(original, transformedPawn,
												 a => a.def.GetModExtension<TFTransferable>()?.CanTransfer(transformedPawn) == true);
		}

		/// <summary>
		///     Transfers the hediffs from the original pawn onto the transformed pawn by checking for the TFTransferable
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="transformedPawn">The transformed pawn.</param>
		public static void TransferHediffs(Pawn original, Pawn transformedPawn)
		{
			PawnTransferUtilities.TransferHediffs(original, transformedPawn,
												  h => h.def.GetModExtension<TFTransferable>()?.CanTransfer(transformedPawn)
													== true);
		}


		/// <summary>
		///     Transfers the relations back to the original pawn after they've been transformed into the transformedPawn
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <exception cref="ArgumentNullException">
		///     original
		///     or
		///     transformedPawn
		/// </exception>
		public static void TransferRelationsToOriginal([NotNull] Pawn original, [NotNull] Pawn transformedPawn)
		{
			if (original == null) throw new ArgumentNullException(nameof(original));
			if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
			Pawn_RelationsTracker rTracker = original.relations;
			if (rTracker == null) return;
			TransferRelationsToOriginal(original, transformedPawn, rTracker);
		}


		/// <summary>
		///     Tries the assign the correct backstory to transformed pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="originalPawn">The original pawn.</param>
		/// <param name="backstoryOverride">The backstory override.</param>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static void TryAssignBackstoryToTransformedPawn([NotNull] Pawn pawn, [CanBeNull] Pawn originalPawn,
															   BackstoryDef backstoryOverride = null)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (!pawn.IsFormerHuman()) return;
			if (pawn.story == null) return;

			if (originalPawn != null)
				pawn.Name = originalPawn.Name;
			else if (pawn.Name == null) pawn.Name = new NameSingle(pawn.LabelShort);


			BackstoryDef backstoryDef = backstoryOverride;
			if (backstoryDef != null)
			{
				pawn.story.Adulthood = backstoryDef;
				return;
			}

			var ext = pawn.def.GetModExtension<FormerHumanSettings>();

			backstoryDef = ext?.backstory ?? BackstoryDefOf.FormerHumanNormal;

			pawn.story.Adulthood = backstoryDef;
		}

		/// <summary>
		///     Tries the get definition variant for the given formerHuman.
		/// </summary>
		/// this also checks if the variant is valid for the formerHuman, and keeps looking until a valid variant is found or all variants are exhausted
		/// <typeparam name="T"></typeparam>
		/// <param name="sourceDef">The source definition.</param>
		/// <param name="formerHuman">The former human.</param>
		/// <param name="variant">The variant.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     sourceDef
		///     or
		///     formerHuman
		/// </exception>
		public static bool TryGetSapientDefVariant<T>([NotNull] this Def sourceDef, [NotNull] Pawn formerHuman, out T variant)
			where T : Def
		{
			if (sourceDef == null) throw new ArgumentNullException(nameof(sourceDef));
			if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));
			SapienceLevel? sapientLevel = formerHuman.GetQuantizedSapienceLevel();
			if (sapientLevel == null)
			{
				variant = default;
				return false;
			}

			foreach (DefModExtension sourceDefModExtension in sourceDef.modExtensions.MakeSafe())
				if (sourceDefModExtension is ISapientVariantHolder<T> variantHolder) //can't use GetModExtension for this 
				{
					T tmp = variantHolder[sapientLevel.Value];
					if (tmp == null || !tmp.IsValidFor(formerHuman)) continue; //keep looking if the variant is not valid 

					variant = tmp;
					return true;
				}

			variant = default;
			return false;
		}

		/// <summary>
		///     Tries the get sapient variant of the specified type for the given level
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sourceDef">The source definition.</param>
		/// <param name="level">The level.</param>
		/// <param name="variant">The variant.</param>
		/// <returns></returns>
		public static bool TryGetSapientVariant<T>([NotNull] this Def sourceDef, SapienceLevel level, out T variant)
		{
			foreach (DefModExtension sourceDefModExtension in sourceDef.modExtensions.MakeSafe())
				if (sourceDefModExtension is ISapientVariantHolder<T> variantHolder) //can't use GetModExtension for this 
				{
					variant = variantHolder[level];
					return variant != null;
				}

			variant = default;
			return false;
		}

		/// <summary>
		///     Tries the get a variant for the given formerHuman.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sourceDef">The source definition.</param>
		/// <param name="formerHuman">The former human.</param>
		/// <param name="variant">The variant.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     sourceDef
		///     or
		///     formerHuman
		/// </exception>
		public static bool TryGetSapientVariant<T>([NotNull] this Def sourceDef, [NotNull] Pawn formerHuman, out T variant)
		{
			if (sourceDef == null) throw new ArgumentNullException(nameof(sourceDef));
			if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));
			SapienceLevel? sapientLevel = formerHuman.GetQuantizedSapienceLevel();
			if (sapientLevel == null)
			{
				variant = default;
				return false;
			}

			foreach (DefModExtension sourceDefModExtension in sourceDef.modExtensions.MakeSafe())
				if (sourceDefModExtension is ISapientVariantHolder<T> variantHolder) //can't use GetModExtension for this 
				{
					variant = variantHolder[sapientLevel.Value];
					return variant != null;
				}

			variant = default;
			return false;
		}

		private static void AddRandomMutationToPawn(Pawn lPawn)
		{
			//give at least as many mutations as there are slots, plus some more to make it a bit more chaotic 
			int mutationsToAdd = Mathf.CeilToInt(MorphUtilities.GetMaxInfluenceOfRace(lPawn.def)) + 10;
			_mScratchList.Clear();
			_mScratchList.AddRange(MutationUtilities.AllNonRestrictedMutations);
			_mScratchList.RemoveAll(m => AnimalClassDefOf
										.Powerful.GetAllMutationIn()
										.Contains(m)); // Chimeras should not recieve powerful mutations.
			var addList = new List<BodyPartRecord>();
			var addedList = new List<BodyPartRecord>();
			_brScratchList.Clear();
			_brScratchList.AddRange(lPawn.health.hediffSet.GetAllNonMissingWithoutProsthetics());


			var i = 0;
			MutationUtilities.AncillaryMutationEffects aEffects = MutationUtilities.AncillaryMutationEffects.None;
			while (i < mutationsToAdd)
			{
				addList.Clear();
				MutationDef rM = _mScratchList.RandomElementWithFallback();
				if (rM == null) break;
				_mScratchList.Remove(rM);

				//handle whole body mutations first 
				if (rM.parts == null)
				{
					if (MutationUtilities.AddMutation(lPawn, rM, ancillaryEffects: aEffects)) i++;
					continue;
				}

				//get the body parts to add to 

				//how many parts to grab 
				//+3 is to make it more likely that all parts will be added 
				int countToAdd = Rand.Range(1, rM.parts.Count + 3);
				countToAdd = Mathf.Min(countToAdd, rM.parts.Count); //make sure it's less then or equal to 
				foreach (BodyPartRecord record in _brScratchList)
				{
					if (!rM.parts.Any(p => p == record.def)) continue;

					if (addedList.Contains(record)) continue;

					addedList.Add(record);
					addList.Add(record);

					if (addList.Count >= countToAdd) break;
				}


				MutationResult res = MutationUtilities.AddMutation(lPawn, rM, addList, aEffects);
				if (res) i++; //only increment if we actually added any mutations 
			}
		}

		private static void CleanupPawn(Pawn lPawn)
		{
			lPawn.equipment
				?.DestroyAllEquipment(); //make sure all equipment and apparel is removed so they don't spawn with it if reverted
			lPawn.apparel?.DestroyAll();
		}

		private static void TransferRelationsToOriginal([NotNull] Pawn pawn, [CanBeNull] Pawn oPawn,
														[NotNull] Pawn_RelationsTracker aRelations)
		{
			List<DirectPawnRelation> dRelations = aRelations.DirectRelations.MakeSafe().Where(r => !r.def.implied).ToList();

			foreach (DirectPawnRelation directPawnRelation in dRelations)
			{
				aRelations.RemoveDirectRelation(directPawnRelation);
				if (oPawn?.relations != null)
				{
					if (directPawnRelation.def == PawnRelationDefOf.Bond) continue; //don't transfer bond 
					oPawn.relations.AddDirectRelation(directPawnRelation.def, directPawnRelation.otherPawn);
				}
			}

			List<Pawn> pRelated = aRelations.PotentiallyRelatedPawns.MakeSafe().ToList();

			foreach (Pawn pawn1 in pRelated) //move relations from potentially related pawns 
			{
				if (pawn1.relations == null) continue;
				List<DirectPawnRelation> relations = pawn1.relations.DirectRelations.MakeSafe()
														  .Where(r => !r.def.implied && r.otherPawn == pawn)
														  .ToList(); //get all relations from potentially related pawns 


				foreach (DirectPawnRelation directPawnRelation in relations)
				{
					if (directPawnRelation.def == PawnRelationDefOf.Bond) continue; //don't transfer bond
					pawn1.relations.RemoveDirectRelation(directPawnRelation);
					if (oPawn?.relations != null) pawn1.relations.AddDirectRelation(directPawnRelation.def, oPawn);
				}
			}
		}

		internal static Dictionary<string, FormerHumanRestrictions> GetDefaultBlockList()
		{

			var defaultConfig = new Dictionary<string, FormerHumanRestrictions>()
			{
				// Genetic Rim
				["GR_ArchotechCentipede"] = FormerHumanRestrictions.Disabled,

				// Alpha Animals
				["AA_Gallatross"] = FormerHumanRestrictions.Restricted,

				// Insectoids
				["VFEI_Insectoid_Queen"] = FormerHumanRestrictions.Disabled,
			};

			foreach (var animal in AnimalClassDefOf.Powerful.GetAssociatedAnimals())
				defaultConfig.Add(animal.defName, FormerHumanRestrictions.Restricted);


			return defaultConfig;
		}
	}
}