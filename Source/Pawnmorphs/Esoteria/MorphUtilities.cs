// MorphUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 3:48 PM
// last updated 08/02/2019  3:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.RecipeWorkers;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using static Pawnmorph.PMHistoryEventArgsNames;

namespace Pawnmorph
{
	/// <summary>
	///     Static collection of useful morph related functions. <br />
	///     TransformerUtilities was getting a bit crowded.
	/// </summary>
	[StaticConstructorOnStartup]
	public static class MorphUtilities
	{
		[NotNull] private static readonly List<ThoughtDef> _allMorphSleepingThoughts;
		[NotNull] private static readonly Dictionary<PawnKindDef, MorphDef> _morphByAnimalKind;






		/// <summary>
		/// Gets all morph group sleeping thoughts.
		/// </summary>
		/// <value>
		/// All morph sleeping thoughts.
		/// </value>
		[NotNull]
		public static IReadOnlyList<ThoughtDef> AllMorphSleepingThoughts => _allMorphSleepingThoughts;

		static MorphUtilities()
		{
			//check for mod incompatibilities here 
			if (ThingDefOf.Human.race.body != BodyDefOf.Human)
			{
				Log.Error($"human ThingDef is using {ThingDefOf.Human.race.body.defName} not {BodyDefOf.Human.defName}!\nmost likely cause is mod incompatibilities please check mod list");
			}

			IEnumerable<ThoughtDef> MorphGroupThoughtSelectorFunc(MorphGroupDef group)
			{
				if (group?.barrakThoughtReplacement != null) yield return group.barrakThoughtReplacement;
				if (group?.bedroomThoughtReplacement != null) yield return group.bedroomThoughtReplacement;
				if (group?.asceticRoomThought != null) yield return group.asceticRoomThought;
			}

			//get all sleeping thoughts for morph groups 
			_allMorphSleepingThoughts = DefDatabase<MorphGroupDef>.AllDefs.SelectMany(MorphGroupThoughtSelectorFunc).ToList();
			//get the associated races lookup 

		}

		[NotNull]
		private static readonly Dictionary<ThingDef, List<MorphDef>> _associatedAnimalsLookup =
			new Dictionary<ThingDef, List<MorphDef>>();

		public static void Initialize()
		{
			foreach (MorphDef morph in DefDatabase<MorphDef>.AllDefsListForReading)
			{
				if (morph.race == null)
				{
					Log.Error($"{morph.defName} has null animal assigned.");
					continue;
				}

				//first check the race 
				if (!_associatedAnimalsLookup.TryGetValue(morph.race, out var lst))
				{
					lst = new List<MorphDef>();
					_associatedAnimalsLookup[morph.race] = lst;
				}

				lst.Add(morph);

				//now the associated animals 
				foreach (ThingDef aAnimal in morph.associatedAnimals.MakeSafe())
				{
					if (aAnimal == null)
					{
						Log.Error($"{morph.defName} has associated null animal.");
						continue;
					}

					if (!_associatedAnimalsLookup.TryGetValue(aAnimal, out lst))
					{
						lst = new List<MorphDef>();
						_associatedAnimalsLookup[aAnimal] = lst;
					}

					lst.Add(morph);
				}
			}
		}

		/// <summary>
		/// Gets the maximum influence for the given body def.
		/// </summary>
		/// <param name="morph">The morph.</param>
		/// <param name="bodyDef">The body definition.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// morph
		/// or
		/// bodyDef
		/// </exception>
		public static float GetMaxInfluenceForBody([NotNull] this MorphDef morph, [NotNull] BodyDef bodyDef)
		{
			if (morph == null) throw new ArgumentNullException(nameof(morph));
			if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));

			if (!_maxInfluenceLookup.TryGetValue(morph, out var sDic))
			{
				sDic = new Dictionary<BodyDef, float>();
				_maxInfluenceLookup[morph] = sDic;
			}

			if (sDic.TryGetValue(bodyDef, out float max)) return max;

			max = morph.AllAssociatedMutations
					   .SelectMany(s => s.GetAllMutationSites(bodyDef)).Count();
			sDic[bodyDef] = max;

			return max;

		}

		private readonly
			static
			Dictionary<MorphDef, Dictionary<BodyDef, float>> _maxInfluenceLookup =
				new Dictionary<MorphDef, Dictionary<BodyDef, float>>();

		/// <summary>
		///     scalar used to make it easier for pawns to become hybrids
		/// </summary>
		[Obsolete] public const float HUMAN_CHANGE_FACTOR = 0.65f;

		/// <summary>the percent influence needed for a single morph to be selected to turn the pawn into, rather then a chimera</summary>
		public const float CHIMERA_THRESHOLD = 0.35f;

		/// <summary>
		///     the percent human influence below which a pawn is 'no longer considered human'
		/// </summary>
		public const float MORPH_TF_THRESHOLD = 2f / 3f;


		private static Dictionary<HediffDef, List<MorphDef>> _morphAssociationCache =
			new Dictionary<HediffDef, List<MorphDef>>(); // So we don't calculate the associations more then we have to.

		private static float? _maxHumanInfluence;

		static string GetRecordInfo([NotNull] BodyPartRecord record)
		{
			string modStr;
			if (record.def?.modContentPack != null)
				modStr = (record.def.modContentPack.Name ?? "") + "-";
			else
				modStr = "";
			return $"{modStr}{record.def?.defName}/{record.Label}";
		}

		[NotNull]
		private static readonly Dictionary<BodyDef, float> _maxInfluenceDict = new Dictionary<BodyDef, float>();

		private const int MAX_TRIES = 10;
		private static int _tries = 0;
		/// <summary> The maximum possible human influence. </summary>
		[Obsolete("use " + nameof(GetMaxInfluenceOfRace) + " instead")]
		public static float MaxHumanInfluence
		{
			get
			{
				if (_maxHumanInfluence == null)
				{
					_maxHumanInfluence = CalculateMaxBodyInfluence(BodyDefOf.Human);
				}
				else if (Math.Abs(_maxHumanInfluence.Value) < 0.01f && _tries < MAX_TRIES)
				{
					Log.Warning($"could not resolve Max Human Influence on try {_tries}");
					_tries++;
					_maxHumanInfluence = CalculateMaxBodyInfluence(BodyDefOf.Human);
				}

				return _maxHumanInfluence.Value;
			}
		}

		/// <summary>
		/// Gets the maximum influence of race.
		/// </summary>
		/// <param name="raceDef">The race definition.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">raceDef</exception>
		/// <exception cref="ArgumentException"></exception>
		public static float GetMaxInfluenceOfRace([NotNull] ThingDef raceDef)
		{
			if (raceDef == null) throw new ArgumentNullException(nameof(raceDef));
			if (raceDef.race?.body == null)
				throw new ArgumentException($"{nameof(raceDef)}:{raceDef.defName} does not have a body def");
			if (_maxInfluenceDict.TryGetValue(raceDef.race.body, out float val))
			{
				return val;
			}

			val = CalculateMaxBodyInfluence(raceDef.race.body);
			_maxInfluenceDict[raceDef.race.body] = val;
			return val;
		}

		private static float CalculateMaxBodyInfluence(BodyDef bodyDef)
		{
			var set = new HashSet<MutationSite>();
			var sites = MutationDef.AllMutations.SelectMany(m => m.GetAllMutationSites(bodyDef));
			set.AddRange(sites);

			if (set.Count == 0)
			{
				var allCheckedParts = bodyDef.AllParts.Join(GetRecordInfo);

				Log.Error($"unable to find any mutation sites on {bodyDef.defName} after checking {MutationDef.AllMutations.Count()} mutations!\nchecked parts:[{allCheckedParts}]");
			}

			return set.Count;
		}


		/// <summary>
		/// Gets the morph of the given animal.
		/// </summary>
		/// <param name="animalDef">The animal definition.</param>
		/// <returns></returns>
		[NotNull, Pure]
		public static IEnumerable<MorphDef> GetMorphOfAnimal([NotNull] ThingDef animalDef)
		{
			foreach (MorphDef morphDef in MorphDef.AllDefs)
			{
				if (morphDef.race == animalDef) yield return morphDef;
			}
		}

		/// <summary>
		/// Determines whether this instance is a chimera.
		/// </summary>
		/// <param name="morph">The morph.</param>
		/// <returns>
		///   <c>true</c> if the specified morph is a chimera; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">morph</exception>
		[Pure]
		public static bool IsChimera([NotNull] this MorphDef morph)
		{
			if (morph == null) throw new ArgumentNullException(nameof(morph));
			return morph.categories?.Contains(MorphCategoryDefOf.Chimera) == true;
		}

		/// <summary>
		/// Determines whether this instance is a chimera.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if the specified pawn is a chimera; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsChaomorph([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return pawn.def?.GetModExtension<ChaomorphExtension>() != null;
		}

		/// <summary>
		/// Checks the race of this pawn. If the pawn is mutated enough it's race is changed to one of the hybrids
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="addMissingMutations">if true, any missing mutations from the highest morph influence will be added</param>
		/// <param name="displayNotifications">if set to <c>true</c> display race shift notifications.</param>
		/// <param name="sendEvents">if set to <c>true</c> send events.</param>
		/// <exception cref="ArgumentNullException">pawn</exception>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		public static void CheckRace([NotNull] this Pawn pawn, bool addMissingMutations = true, bool displayNotifications = true,
									 bool sendEvents = true)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (!HybridsAreEnabledFor(pawn.def)) return;
			if (pawn.DevelopmentalStage != DevelopmentalStage.Adult)
				return; // If pawn is not an adult, then don't shift race.
						// Would need to handle development stages across multiple races and potentially between different HAR races.

			if (pawn.ShouldBeConsideredHuman())
			{
				if (pawn.def != ThingDefOf.Human)
				{
					MorphDef morph = pawn.def.GetMorphOfRace();
					ThoughtDef reversionMemory = morph?.transformSettings?.revertedMemory;
					if (reversionMemory != null) pawn.TryGainMemory(reversionMemory);
					RaceShiftUtilities.ChangePawnRace(pawn, ThingDefOf.Human);
					if (morph != null)
						PMHistoryEventDefOf.DeMorphed.SendEvent(pawn.Named(HistoryEventArgsNames.Doer), morph.Named(OLD_MORPH));
					else
						Log.Warning($"reverting a non morph pawn {pawn.Name} to human");
				}

				return;
			}

			MutationTracker mutTracker = pawn.GetMutationTracker();

			AnimalClassBase hInfluence = mutTracker.HighestInfluence;

			if (hInfluence == null) return;
			float morphInfluence = mutTracker.GetDirectNormalizedInfluence(hInfluence);
			int morphInfluenceCount = mutTracker.Count();
			bool isBelowChimeraThreshold = morphInfluence < CHIMERA_THRESHOLD && morphInfluenceCount > 1;

			MorphDef setMorph = GetMorphForPawn(pawn, isBelowChimeraThreshold, hInfluence, out MorphDef curMorph);
			if (setMorph?.raceSettings?.PawnCanBecomeHybrid(pawn) == false) return;
			if (curMorph != setMorph && setMorph != null)
			{
				RaceShiftUtilities.ChangePawnToMorph(pawn, setMorph, addMissingMutations, displayNotifications);
				if (curMorph != null)
					PMHistoryEventDefOf.Morphed.SendEvent(pawn.Named(HistoryEventArgsNames.Doer),
															  curMorph.Named(OLD_MORPH), setMorph.Named(NEW_MORPH));
				else
					PMHistoryEventDefOf.Morphed.SendEvent(pawn.Named(HistoryEventArgsNames.Doer),
															  setMorph.Named(NEW_MORPH));

			}
		}

		[CanBeNull]
		private static MorphDef GetMorphForPawn(Pawn pawn, bool isBelowChimeraThreshold, AnimalClassBase hInfluence, out MorphDef curMorph)
		{
			MorphDef setMorph;
			curMorph = pawn.def.GetMorphOfRace();
			try
			{
				Rand.PushState(pawn.thingIDNumber); // make sure this is deterministic for each pawn 

				if (isBelowChimeraThreshold) //if they'er below turn them into a chimera 
				{
					setMorph = GetChimeraRace(hInfluence);
				}
				else
				{
					setMorph = hInfluence as MorphDef;

				}
			}
			finally
			{
				Rand.PopState();
			}

			return setMorph;
		}

		/// <summary>Gets all morphs.</summary>
		/// <value>All morphs.</value>
		[NotNull]
		public static IEnumerable<MorphDef> AllMorphs => DefDatabase<MorphDef>.AllDefs;


		/// <summary>
		/// Tries the get best morph of the specified animal.
		/// </summary>
		/// tries to get the best morph def of the given animal, checking first for a morph who's
		/// race is the given race then checks morph's associated animals 
		/// <param name="race">The race.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">race</exception>
		[CanBeNull]
		public static MorphDef TryGetBestMorphOfAnimal([NotNull] this ThingDef race)
		{
			return _associatedAnimalsLookup.TryGetValue(race)?.FirstOrDefault();

		}



		/// <summary> Gets all morphDefs associated with the given transformation. </summary>
		/// <param name="transformationDef"> The transformation definition. </param>
		[NotNull]
		public static IEnumerable<MorphDef> GetAssociatedMorph(HediffDef transformationDef)
		{
			if (_morphAssociationCache.TryGetValue(transformationDef, out List<MorphDef> lst)) return lst;

			lst = new List<MorphDef>();

			var enumerable = transformationDef.GetAllHediffGivers().Select(g => g.hediff).OfType<MutationDef>();

			foreach (MutationDef mutationDef in enumerable)
			{
				foreach (AnimalClassBase animalClass in mutationDef.ClassInfluences)
				{
					if (animalClass is MorphDef morph)
					{
						if (!lst.Contains(morph))
						{
							lst.Add(morph);
							break;
						}
					}
				}
			}
			_morphAssociationCache[transformationDef] = lst;
			return lst;
		}

		/// <summary>
		/// Determines whether this instance is a morph.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if the specified pawn is morph; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static bool IsMorph([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return pawn.def.GetMorphOfRace() != null;
		}

		/// <summary> Gets the amount of influence a pawn has that's still human.</summary>
		/// <param name="pawn">the pawn</param>
		/// <param name="normalize"> Whether or not the resulting influence should be normalized between [0,1] </param>
		/// <returns></returns>
		public static float GetHumanInfluence([NotNull] this Pawn pawn, bool normalize = false)
		{
			MutationTracker mTracker = pawn.GetMutationTracker();
			var mxInfluence = GetMaxInfluenceOfRace(pawn.def);
			if (mTracker == null)
			{
				return mxInfluence; //always have non mutatable pawns be considered 'human' so the hybrid system never triggers for them 
			}

			var hInfluence = mxInfluence - mTracker.TotalInfluence;
			if (normalize) hInfluence /= mxInfluence;
			return hInfluence;
		}


		/// <summary> Gets the type of the transformation. </summary>
		/// <param name="inst"> The instance. </param>
		/// <returns> The type of the transformation. </returns>
		/// <exception cref="ArgumentNullException"> inst is null </exception>
		public static MorphTransformationTypes GetTransformationType([NotNull] this HediffDef inst)
		{
			if (inst == null) throw new ArgumentNullException(nameof(inst));
			if (!(inst.hediffClass is IMutagenicHediff)) return 0;

			var comp = inst.CompProps<HediffCompProperties_Single>();
			return comp == null ? MorphTransformationTypes.Full : MorphTransformationTypes.Partial;
		}


		/// <summary>checks if the hybrid system is enabled for the given race def.</summary>
		/// <param name="raceDef">The race definition.</param>
		/// <returns></returns>
		public static bool HybridsAreEnabledFor(this ThingDef raceDef)
		{
			if (raceDef == ThingDefOf.Human) return true;
			return raceDef.IsHybridRace();
		}

		/// <summary>
		///     Determines whether this instance is a chimera morph.
		/// </summary>
		/// <param name="morphDef">The morph definition.</param>
		/// <returns>
		///     <c>true</c> if this instance is a chimera morph; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsChimeraMorph([NotNull] this MorphDef morphDef)
		{
			return MorphCategoryDefOf.Chimera.AllMorphsInCategories.Contains(morphDef);
		}


		/// <summary>
		///     Determines whether this pawn is a hybrid race.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if this pawn is a hybrid race ; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static bool IsHybridRace([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefs)
				if (pawn.def == morphDef.hybridRaceDef)
					return true;

			return false;
		}

		/// <summary>
		///     Determines whether this instance is hybrid race.
		/// </summary>
		/// <param name="raceDef">The race definition.</param>
		/// <returns>
		///     <c>true</c> if this instance is a hybrid race; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsHybridRace([NotNull] this ThingDef raceDef)
		{
			return raceDef.GetMorphOfRace() != null;
		}

		/// <summary> Get whether or not the given pawn should still be considered 'human'. </summary>
		public static bool ShouldBeConsideredHuman([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (pawn.health?.hediffSet?.hediffs == null) return true;

			MutationTracker tracker = pawn.GetMutationTracker();
			if (tracker == null) return true;


			float humanInfluence = GetHumanInfluence(pawn, true);

			return humanInfluence > MORPH_TF_THRESHOLD;
		}



		private static MorphDef GetChimeraRace([CanBeNull] AnimalClassBase hInfluence)
		{
			if (hInfluence != null)
			{
				var morph = hInfluence as MorphDef;
				//if the highest influence isn't a morph pick a random morph from the animal class
				morph = morph ?? ((AnimalClassDef)hInfluence).GetAllMorphsInClass().RandomElementWithFallback();

				// Morph can be null here if the AnimalClassDef doesn't have any associated morphs
				if (morph != null)
				{
					if (AnimalClassDefOf.Canid.Contains(morph.classification)) //TODO Generalize this or just pick randomly, 
						return MorphDefOfs.ChaofoxMorph;

					if (AnimalClassDefOf.Reptile.Contains(morph.classification))
						return MorphDefOfs.ChaodinoMorph;

					if (AnimalClassDefOf.Cervid.Contains(morph.classification))
						return MorphDefOfs.ChaoboomMorph;

					if (AnimalClassDefOf.Bovid.Contains(morph.classification))
						return MorphDefOfs.ChaocowMorph;
				}
			}
			return MorphCategoryDefOf.Chimera.AllMorphsInCategories.RandomElement();
		}

		/// <summary>
		///     get the largest influence on this pawn
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		[CanBeNull]
		public static MorphDef GetHighestInfluence([NotNull] this Pawn pawn)
		{
			MutationTracker comp = pawn.GetMutationTracker();
			if (comp == null) return null;

			MorphDef highest = null;
			float max = float.NegativeInfinity;
			foreach (KeyValuePair<AnimalClassBase, float> keyValuePair in comp)
			{
				if (!(keyValuePair.Key is MorphDef morph)) continue;
				if (max < keyValuePair.Value)
				{
					max = keyValuePair.Value;
					highest = morph;
				}
			}

			return highest;
		}
	}
}