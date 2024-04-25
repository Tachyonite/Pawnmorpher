// MutationDef.cs modified by Iron Wolf for Pawnmorph on 01/11/2020 8:16 AM
// last updated 01/11/2020  8:16 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.GraphicSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     custom hediff def for mutations
	/// </summary>
	/// <seealso cref="Verse.HediffDef" />
	public class MutationDef : HediffDef, IDebugString
	{
		[Unsaved()]
		private MutationStage[] _cachedMutationStages;

		/// <summary>
		///     list of body parts this mutation can be added to
		/// </summary>
		/// note: this does not affect HediffGiver_AddedMutation, this is for adding mutations without a hediff giver
		[CanBeNull] public List<BodyPartDef> parts = new List<BodyPartDef>();

		/// <summary>the number of parts to add this mutation to</summary>
		[Obsolete] public int countToAffect;

		/// <summary>
		///     the various mutation categories this mutation belongs to
		/// </summary>
		public List<MutationCategoryDef> categories = new List<MutationCategoryDef>();

		/// <summary>
		///     The stage patches that are applied once the object has been deserialized.
		/// </summary>
		public List<MutationStagePatch> stagePatches = new List<MutationStagePatch>();

		/// <summary>
		///     The default chance to add this mutation
		/// </summary>
		public float defaultAddChance = 1f;

		/// <summary>
		///     The default value indicating weather or not this mutation blocks a transformation chain until it is added
		/// </summary>
		public bool defaultBlocks = false;

		/// <summary>
		/// if this mutation can be tagged and stored 
		/// </summary>
		public bool isTaggable = true;

		/// <summary>
		/// Indicates whether there is a reason to run vanilla hediff base logic or not.
		/// </summary>
		public bool RunBaseLogic = false;

		/// <summary>
		/// optional field that will act as an explicit description for the mutations 'genome' object
		/// </summary>
		public string customGenomeDescription;

		/// <summary>
		///     list of other mutations this mutation blocks
		/// </summary>
		public List<BlockEntry> blockList = new List<BlockEntry>();

		/// <summary>
		/// list of body part defs that this mutations blocks other mutations from being added onto 
		/// </summary>
		public List<BodyPartDef> blockSites = new List<BodyPartDef>();


		/// <summary>
		/// The graphics for this mutation 
		/// </summary>
		[CanBeNull]
		public List<MutationGraphicsData> graphics = new List<MutationGraphicsData>();

		/// <summary>
		/// The abstract 'value' of this mutation, can be negative or zero if the mutation is in general negative 
		/// </summary>
		public int value;

		/// <summary>
		///     the rule pack to use when generating mutation logs for this mutation
		/// </summary>
		[CanBeNull] public RulePackDef mutationLogRulePack;


		/// <summary>The mutation tale</summary>
		[CanBeNull] public TaleDef mutationTale;

		/// <summary>
		/// if this mutation should be removed instantly by a reverter
		/// </summary>
		public bool removeInstantly;

		/// <summary>
		///     The class this part gives influence for
		/// </summary>
		/// only should be set if morphInfluence is not set!
		[CanBeNull]
		[UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public AnimalClassBase classInfluence;

		/// <summary>
		/// The class influences if multiple.
		/// </summary>
		[CanBeNull]
		[UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public List<AnimalClassBase> classInfluences;

		/// <summary>The mutation memory</summary>
		[CanBeNull] public ThoughtDef mutationMemory;

		/// <summary>
		///     if true, the mutation will not respect the max mutation thoughts mod setting
		/// </summary>
		public bool memoryIgnoresLimit;

		[Unsaved] private RestrictionLevel? _restrictionLevel;

		[Unsaved] private RemoveFromPartCompProperties _rmComp;

		[Unsaved] private List<ThingDef> _associatedAnimals;
		[Unsaved] private List<AnimalClassBase> _classInfluencesCache;

		/// <summary>
		/// Gets a cached mutation layer from any remover RemoveFromPartCompProperties component. Null if none.
		/// </summary>
		public MutationLayer? Layer { get; private set; } = null;


		/// <summary>
		///     Gets the animals associated with this mutation animals.
		/// </summary>
		/// <value>
		///     The associated animals.
		/// </value>
		public IReadOnlyList<ThingDef> AssociatedAnimals
		{
			get
			{
				return _associatedAnimals
					?? (_associatedAnimals = MorphDef.AllDefs.Where(m => m.IsAnAssociatedMutation(this))
													 .SelectMany(m => m.AllAssociatedAnimals)
													 .ToList());
			}
		}

		/// <summary>
		/// Gets the cached mutation stages. this is the same size as stages but pre cast to <see cref="MutationStage"/> if a particular stage is not
		/// a MutationsStage then the corresponding entry in this list is null 
		/// </summary>
		/// <value>
		/// The cached mutation stages.
		/// </value>
		[NotNull]
		public IReadOnlyList<MutationStage> CachedMutationStages => _cachedMutationStages ??= Array.Empty<MutationStage>();

		/// <summary>
		///     returns a full, detailed, representation of the object in string form
		/// </summary>
		/// <returns></returns>
		public string ToStringFull()
		{
			var builder = new StringBuilder();
			builder.AppendLine($"-{defName}/{label}-");
			if (parts == null)
				builder.AppendLine("full body mutation");
			else
				builder.AppendLine($"parts:[{parts.Join(n => n.defName)}]");

			return builder.ToString();
		}

		/// <summary>
		///     Gets all mutations.
		/// </summary>
		/// <value>
		///     All mutations.
		/// </value>
		[NotNull]
		public static IEnumerable<MutationDef> AllMutations => DefDatabase<MutationDef>.AllDefs;

		/// <summary>
		///     Gets the remover comp.
		/// </summary>
		/// this is the comp used to remove 'overlapping' mutations
		/// <value>
		///     The remove comp.
		/// </value>
		public RemoveFromPartCompProperties RemoveComp => CompProps<RemoveFromPartCompProperties>();

		/// <summary>Gets a value indicating whether this instance is restricted to special PawnKindGroups</summary>
		/// <value>
		///     <c>true</c> if this instance is restricted the mutation can only be given to special PawnKindGroups; otherwise it
		///     can show up in any group, <c>false</c>.
		/// </value>
		public bool IsRestricted
		{
			get { return RestrictionLevel > RestrictionLevel.UnRestricted; }
		}

		/// <summary>
		/// Gets the finalized collection of class influences regardless of how it was defined in the XML.
		/// </summary>
		[NotNull]
		public List<AnimalClassBase> ClassInfluences
		{
			get
			{
				if (_classInfluencesCache == null)
					_classInfluencesCache = classInfluences ?? new List<AnimalClassBase>() { classInfluence ?? AnimalClassDefOf.Animal };

				return _classInfluencesCache;
			}
		}

		/// <summary>
		/// Gets the restriction level of this mutation
		/// </summary>
		/// <value>
		/// The restriction level.
		/// </value>
		public RestrictionLevel RestrictionLevel
		{
			get
			{
				if (_restrictionLevel == null)
				{
					_restrictionLevel = RestrictionLevel.UnRestricted;
					foreach (MutationCategoryDef cat in categories.MakeSafe())
					{
						if (_restrictionLevel == RestrictionLevel.Always) break;
						if (cat.restrictionLevel > _restrictionLevel) _restrictionLevel = cat.restrictionLevel;
					}

				}

				return _restrictionLevel.Value;
			}
		}

		/// <summary>
		///     checks if this mutation blocks the addition of the otherMutation.
		/// </summary>
		/// checks if this mutation on the 'thisPart' blocks the addition of the otherMutation on the 'addPart'
		/// checks if this mutation on the 'thisPart' blocks the addition of the otherMutation on the 'addPart'
		/// <param name="otherMutation">The other mutation.</param>
		/// <param name="thisPart">The part this mutation is already on.</param>
		/// <param name="addPart">The  part the otherMutation will be added to.</param>
		/// <returns></returns>
		public bool BlocksMutation([NotNull] MutationDef otherMutation, [CanBeNull] BodyPartRecord thisPart,
								   [CanBeNull] BodyPartRecord addPart)
		{
			if (blockSites?.Contains(addPart?.def) == true) return true;
			BlockEntry entry = blockList?.FirstOrDefault(e => e.mutation == otherMutation);
			if (entry == null) return false;
			return thisPart == addPart || entry.blockOnAnyPart;
		}


		/// <summary>
		///     Gets all configuration errors
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors()) yield return configError;

			if (!typeof(Hediff_AddedMutation).IsAssignableFrom(hediffClass))
				yield return $"{hediffClass.Name} is not {nameof(Hediff_AddedMutation)} or a subtype of it";


			_rmComp = CompProps<RemoveFromPartCompProperties>();
			if (_rmComp == null)
				yield return "mutation does not have a remover comp!";

			foreach (BlockEntry entry in blockList.MakeSafe())
				if (entry.mutation == null)
					yield return "block entry has missing mutation def!";


			if (classInfluence != null && (classInfluences != null && classInfluences.Count > 0))
			{
				yield return "both classInfluence and classInfluences are set. only 1 should be set in any mutation";
			}

			if (classInfluence == null && (classInfluences == null || classInfluences.Count == 0))
			{
				yield return "No classInfluence has been assigned.";
			}

			if (parts == null || parts.Count == 0)
			{
				yield return "No body parts assigned to mutation.";
			}

			if (Layer == null)
			{
				yield return "No layer assigned to mutation. Make sure it has a RemoveFromPartCompProperties with layer assigned.";
			}
		}


		/// <summary>
		///     checks if this instance gives influence for the given animal class
		/// </summary>
		/// <param name="classDef">The class definition.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">classDef</exception>
		public bool GivesInfluence([NotNull] AnimalClassDef classDef)
		{
			if (classDef == null) throw new ArgumentNullException(nameof(classDef));
			if (ClassInfluences == null) return false;
			return ClassInfluences.Any(x => classDef.Contains(x));
		}

		/// <summary>
		///     checks if this instance gives influence for the given morph
		/// </summary>
		/// <param name="morph">The morph.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">morph</exception>
		public bool GivesInfluence([NotNull] MorphDef morph)
		{
			if (morph == null) throw new ArgumentNullException(nameof(morph));
			if (ClassInfluences == null) return false;
			return ClassInfluences.Any(x => x.Contains(morph));
		}

		/// <summary>
		///     Resolves the references.
		/// </summary>
		public override void ResolveReferences()
		{
			base.ResolveReferences();

			if (mutationMemory == null)
			{
				mutationMemory = DefDatabase<ThoughtDef>.GetNamedSilentFail(defName);
				if (mutationMemory != null)
				{
					//Log.Message($"{defName} has implicitly defined {nameof(mutationMemory)}, this should be assigned explicitly");
				}
			}


			foreach (MutationStagePatch stagePatch in stagePatches)
			{
				stagePatch.Apply(this);
			}


			if (parts != null)
			{
				//get rid of any duplicates 
				_tmpPartLst.Clear();
				_tmpPartLst.AddRange(parts.Distinct());
				parts.Clear();
				parts.AddRange(_tmpPartLst);

			}


			if (stages.NullOrEmpty())
			{
				_cachedMutationStages = Array.Empty<MutationStage>();
			}
			else
			{
				int count = stages.Count;
				_cachedMutationStages = new MutationStage[count];
				for (int i = 0; i < stages.Count; i++)
				{
					_cachedMutationStages[i] = stages[i] as MutationStage;
				}
			}

			if (hediffGivers != null && hediffGivers.Count > 0)
				RunBaseLogic = true;

			Layer = RemoveComp?.layer;
		}

		[NotNull]
		private static readonly List<BodyPartDef> _tmpPartLst = new List<BodyPartDef>();



		/// <summary>
		///     simple class for a single 'block entry'
		/// </summary>
		public class BlockEntry
		{
			/// <summary>
			///     The mutation to block from being added
			/// </summary>
			public MutationDef mutation;

			/// <summary>
			///     if true, the mutation will be block from any part, not just on the same part this mutation is on
			/// </summary>
			public bool blockOnAnyPart;

			/// <summary>
			/// Checks if the given source mutation blocks the given otherMutation being added at the given part
			/// </summary>
			/// <param name="sourceMutation">The source mutation.</param>
			/// <param name="otherMutation">The other mutation.</param>
			/// <param name="addPart">The add part.</param>
			/// <returns></returns>
			public bool Blocks([NotNull] Hediff_AddedMutation sourceMutation, [NotNull] MutationDef otherMutation, [CanBeNull] BodyPartRecord addPart)
			{
				if (sourceMutation == null) throw new ArgumentNullException(nameof(sourceMutation));
				if (otherMutation == null) throw new ArgumentNullException(nameof(otherMutation));

				if (otherMutation != mutation)
				{
					return false;
				}

				return blockOnAnyPart || addPart == sourceMutation.Part;

			}
		}


	}
}