// MorphDef.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:32 PM
// last updated 08/02/2019  2:32 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary> Def class for a morph. Used to generate the morph's implicit race. </summary>
	public class MorphDef : AnimalClassBase
	{
		/// <summary>
		///     The categories that the morph belongs to. <br />
		///     For example, a Pigmorph belongs to the Farm and Production morph groups.
		/// </summary>
		[CanBeNull]
		public List<MorphCategoryDef> categories = new List<MorphCategoryDef>();

		/// <summary>
		/// The adjective for this morph. ex. wolf morph should be "wolfish" or "lupine"
		/// </summary>
		public string adjective;

		/// <summary>
		///     The creature this race is a morph of.<br />
		///     For example, a Wargmorph's race should be Warg.
		/// </summary>
		[CanBeNull]
		public ThingDef race;

		/// <summary>
		/// The associated animals
		/// </summary>
		/// these are a list of animals that are associated with this morph but who don't influence the hybrid race
		/// transformation targets 
		/// setting this is useful for getting mutations for animals that don't have morphs yet
		[CanBeNull]
		public List<ThingDef> associatedAnimals = new List<ThingDef>();

		/// <summary>
		/// Gets the explicit hybrid race.
		/// </summary>
		/// <value>
		/// The explicit hybrid race.
		/// </value>
		[CanBeNull] public ThingDef ExplicitHybridRace => raceSettings?.explicitHybridRace;


		/// <summary>
		/// if true, then all restricted mutations (not just those that are directly tied to this morph) will be added to <see cref="AllAssociatedMutations"/>
		/// </summary>
		public bool allowAllRestrictedParts;


		/// <summary>
		///     The genus of this morph
		///     this should be a class like 'canis'
		/// </summary>
		public AnimalClassDef classification;

		/// <summary>
		///     The group the morph belongs to. <br />
		///     For example, a Huskymorph belongs to the pack, while a Cowmorph is a member of the herd.
		/// </summary>
		public MorphGroupDef group;

		/// <summary> Various settings for the morph's implied race.</summary>
		public HybridRaceSettings raceSettings = new HybridRaceSettings();

		/// <summary> Various settings determining what happens when a pawn is transformed or reverted.</summary>
		public TransformSettings transformSettings = new TransformSettings();

		/// <summary> Aspects that a morph of this race get.</summary>
		public List<AddedAspect> addedAspects = new List<AddedAspect>();

		/// <summary>
		///     The full transformation chain
		/// </summary>
		[CanBeNull] public HediffDef fullTransformation;

		/// <summary>
		/// properties for the generated full tf hediff 
		/// </summary>
		[CanBeNull] public MorphHediffProperties fullTfHediffProps;


		/// <summary>
		///     The partial transformation chain
		/// </summary>
		[CanBeNull] public HediffDef partialTransformation;

		/// <summary>
		/// properties for the generated partial tf hediff 
		/// </summary>
		[CanBeNull] public MorphHediffProperties partialTfHediffProps;

		/// <summary>
		/// The injector definition
		/// </summary>
		[CanBeNull] public ThingDef injectorDef;


		/// <summary>
		/// if this morph should have no injector or hediff specific for it 
		/// </summary>
		/// Note: this is for suppressing warnings about missing injectors 
		public bool noInjector;

		/// <summary>
		/// The properties for the generated injector def 
		/// </summary>
		[CanBeNull] public MorphInjectorProperties injectorProperties;

		/// <summary> The morph's implicit race.</summary>
		[Unsaved] public ThingDef hybridRaceDef;


		[Unsaved] private readonly Dictionary<BodyDef, float> _maxInfluenceCached = new Dictionary<BodyDef, float>();

		[Unsaved] private Dictionary<BodyPartDef, List<MutationDef>> _mutationsByParts;

		[Unsaved] private List<MutationDef> _allAssociatedMutations;

		[Unsaved] private List<PawnKindDef> _primaryPawnKindDefs;
		[Unsaved] private List<PawnKindDef> _secondaryPawnKindDefs;


		/// <summary>
		/// Gets the animal pawnkinds associated with this morph.
		/// </summary>
		[NotNull]
		public IEnumerable<PawnKindDef> FeralPawnKinds
		{
			get
			{
				if (_primaryPawnKindDefs == null)
					return Array.Empty<PawnKindDef>();

				return _primaryPawnKindDefs.Concat(_secondaryPawnKindDefs);
			}
		}

		/// <summary>
		///     Gets the children.
		/// </summary>
		/// <value>
		///     The children.
		/// </value>
		public override IEnumerable<AnimalClassBase> Children =>
			Enumerable.Empty<AnimalClassBase>(); //morphs can't have class children

		/// <summary>
		///     Gets the label.
		/// </summary>
		/// <value>
		///     The label.
		/// </value>
		public override string Label => label;

		/// <summary>
		///     Gets the parent class.
		/// </summary>
		/// <value>
		///     The parent class.
		/// </value>
		public override AnimalClassDef ParentClass => classification;

		/// <summary> Gets an enumerable collection of all the morph type's defs.</summary>
		[NotNull]
		public static IEnumerable<MorphDef> AllDefs => DefDatabase<MorphDef>.AllDefs;

		private bool _checkAssociatedAnimals;

		/// <summary>
		/// Gets all associated animals.
		/// </summary>
		/// <value>
		/// All associated animals.
		/// </value>
		[NotNull]
		public IReadOnlyList<ThingDef> AllAssociatedAnimals
		{
			get
			{
				if (associatedAnimals == null)
				{
					associatedAnimals = new List<ThingDef>();
				}

				if (!_checkAssociatedAnimals)
				{
					_checkAssociatedAnimals = true;
					if (race != null && !associatedAnimals.Contains(race)) associatedAnimals.Add(race);
				}

				return associatedAnimals;
			}
		}

		/// <summary>
		/// Gets the categories.
		/// </summary>
		/// <value>
		/// The categories.
		/// </value>
		[NotNull]
		public IReadOnlyList<MorphCategoryDef> Categories
		{
			get
			{
				if (categories == null) return Array.Empty<MorphCategoryDef>();
				return categories;
			}
		}


		/// <summary>Gets the collection of all mutations associated with this morph def</summary>
		/// <value>All associated mutations.</value>
		[NotNull]
		public IReadOnlyList<MutationDef> AllAssociatedMutations
		{
			get
			{
				if (_allAssociatedMutations == null)
				{
					_allAssociatedMutations = GetAllAssociatedMutations().Distinct().ToList();
				}

				return _allAssociatedMutations;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="MorphDef"/> is restricted.
		/// </summary>
		/// <value>
		///   <c>true</c> if restricted; otherwise, <c>false</c>.
		/// </value>
		public bool Restricted => categories?.Any(c => c.restricted) == true;


		[NotNull]
		IEnumerable<MutationDef> GetAllAssociatedMutations()
		{
			var restrictionSet = new HashSet<MutationDef>();

			var set = new HashSet<(BodyPartDef bodyPart, MutationLayer layer)>();
			AnimalClassBase curNode = this;
			List<MutationDef> tmpList = new List<MutationDef>();
			var tmpSiteLst = new List<(BodyPartDef bodyPart, MutationLayer layer)>();

			while (curNode != null)
			{
				restrictionSet.AddRange(curNode.MutationExclusionList);

				tmpList.Clear();
				foreach (MutationDef mutation in MutationDef.AllMutations) //grab all mutations that give the current influence directly 
				{
					if (restrictionSet.Contains(mutation))
						continue;

					if (mutation.ClassInfluences.Contains(curNode) == false)
						continue;

					if (curNode != this && allowAllRestrictedParts == false && mutation.IsRestricted)
					{
						if (categories == null)
							continue;

						List<MutationCategoryDef> rCategories = mutation
															   .categories.Where(c => c.restrictionLevel
																					>= RestrictionLevel.CategoryOnly)
															   .ToList();

						bool allowed = rCategories.All(cat => categories.Any(c => c.associatedMutationCategory == cat) == true)
									== true; //make sure all restricted mutation categories are from one of the associated morph categories this morph is a part of 

						if (!allowed)
							continue; //do not allow restricted parts for higher up in the hierarchy to show up unless allowAllRestrictedParts is set to true
					}

					tmpList.Add(mutation);
				}

				foreach (MutationDef mutationDef in tmpList)
				{
					tmpSiteLst.Clear();
					tmpSiteLst.AddRange(mutationDef.GetAllDefMutationSites());
					bool shouldReject = true;
					foreach (var entry in tmpSiteLst)
					{
						if (!set.Contains(entry))
						{
							shouldReject = false;
							break;
						}
					}

					if (!shouldReject) //if there are any free slots yield the mutation 
					{
						set.AddRange(tmpSiteLst);
						yield return mutationDef;
					}

				}


				curNode = curNode.ParentClass; //move up one in the hierarchy 
			}
		}

		/// <summary>
		///     get all configuration errors with this instance
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors()) yield return configError;


			if (race == null)
				yield return "No race def found!";
			else if (race.race == null) yield return $"Race {race.defName} has no race properties! Are you sure this is a race?";

			if (classification == null) yield return $"No classification set!";
		}

		/// <summary>
		///     Determines whether this instance contains the object.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified other]; otherwise, <c>false</c>.
		/// </returns>
		public override bool Contains(AnimalClassBase other)
		{
			return other == this;
		}


		/// <summary>Gets the mutation that affect the given part from this morph def</summary>
		/// <param name="partDef">The part definition.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">partDef</exception>
		[NotNull]
		public IEnumerable<MutationDef> GetMutationForPart([NotNull] BodyPartDef partDef)
		{
			if (partDef == null) throw new ArgumentNullException(nameof(partDef));
			if (_mutationsByParts == null)
			{
				_mutationsByParts = new Dictionary<BodyPartDef, List<MutationDef>>();
				foreach (MutationDef mutation in AllAssociatedMutations) //build the lookup dict here 
					foreach (BodyPartDef part in mutation.parts.MakeSafe()) //gets a list of all parts this mutation affects 
					{
						List<MutationDef> lst;
						if (_mutationsByParts.TryGetValue(part, out lst))
						{
							lst.Add(mutation);
						}
						else
						{
							lst = new List<MutationDef> { mutation };
							_mutationsByParts[part] = lst;
						}
					}
			}

			return _mutationsByParts.TryGetValue(partDef) ?? Enumerable.Empty<MutationDef>();
		}


		/// <summary>
		///     obsolete, does nothing
		/// </summary>
		/// <param name="food"></param>
		/// <returns></returns>
		[Obsolete("This is no longer used")]
		public FoodPreferability? GetOverride(ThingDef food) //note, RawTasty is 5, RawBad is 4 
		{
			if (food?.ingestible == null) return null;
			foreach (HybridRaceSettings.FoodCategoryOverride foodOverride in raceSettings.foodSettings.foodOverrides)
				if ((food.ingestible.foodType & foodOverride.foodFlags) != 0)
					return foodOverride.preferability;

			return null;
		}

		/// <summary>
		///     Determines whether the specified hediff definition is an associated mutation .
		/// </summary>
		/// <param name="hediffDef">The hediff definition.</param>
		/// <returns>
		///     <c>true</c> if the specified hediff definition is an associated mutation; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">hediffDef</exception>
		[Obsolete]
		public bool IsAnAssociatedMutation([NotNull] HediffDef hediffDef)
		{
			if (hediffDef == null) throw new ArgumentNullException(nameof(hediffDef));
			if (hediffDef is MutationDef mDef) return AllAssociatedMutations.Contains(mDef);

			return false;
		}

		/// <summary>
		///     Determines whether the specified hediff definition is an associated mutation .
		/// </summary>
		/// <param name="mutationDef">The hediff definition.</param>
		/// <returns>
		///     <c>true</c> if the specified hediff definition is an associated mutation; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">hediffDef</exception>
		public bool IsAnAssociatedMutation([NotNull] MutationDef mutationDef)
		{
			if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));

			return AllAssociatedMutations.Contains(mutationDef);
		}


		/// <summary>
		///     Determines whether the given hediff is an associated mutation.
		/// </summary>
		/// <param name="hediff">The hediff.</param>
		/// <returns>
		///     <c>true</c> if the specified hediff is an associated mutation ; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">hediff</exception>
		public bool IsAnAssociatedMutation([NotNull] Hediff hediff)
		{
			if (hediff?.def == null) throw new ArgumentNullException(nameof(hediff));
			if (hediff is Hediff_AddedMutation mutation) return AllAssociatedMutations.Contains(mutation.def as MutationDef);

			return false;
		}

		/// <summary>
		///     resolves all references after DefOfs are loaded
		/// </summary>
		public override void ResolveReferences()
		{
			if (ExplicitHybridRace != null)
			{
				hybridRaceDef = ExplicitHybridRace;

			}

			if (_allAssociatedMutations.NullOrEmpty())
			{
				_allAssociatedMutations = GetAllAssociatedMutations().Distinct().ToList();
			}

			if (associatedAnimals == null)
			{

			}



			_primaryPawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(p => p.race == race).ToList();
			_secondaryPawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(p => AllAssociatedAnimals.Contains(p.race)).ToList();
			injectorProperties?.ResolveReferences(race.label);
		}

		/// <summary> Settings to control what happens when a pawn changes race to this morph type.</summary>
		public class TransformSettings
		{
			/// <summary> The TaleDef that should be used in art that occurs whenever a pawn shifts to this morph.</summary>
			[CanBeNull] public TaleDef transformTale;

			/// <summary> The content of the message that should be spawned when a pawn shifts to this morph.</summary>
			[CanBeNull] public string transformationMessage;

			/// <summary> The message type that should be used when a pawn shifts to this morph.</summary>
			[CanBeNull] public MessageTypeDef messageDef;

			/// <summary> Memory added when a pawn shifts to this morph.</summary>
			[CanBeNull] public ThoughtDef transformationMemory;

			/// <summary>
			///     Memory added when the pawn reverts from this morph back to human if they have neither the body purist or
			///     furry traits.
			/// </summary>
			[CanBeNull] public ThoughtDef revertedMemory;

		}

		/// <summary> Aspects to add when a pawn changes race to this morph type and settings asociated with them.</summary>
		public class AddedAspect
		{
			/// <summary> The Def of the aspect to add.</summary>
			public AspectDef def;

			/// <summary> Whether or not the aspect should be kept even if the pawn switches race.</summary>
			public bool keepOnReversion;
		}
	}
}