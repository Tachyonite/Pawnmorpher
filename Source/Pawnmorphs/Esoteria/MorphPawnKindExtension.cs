// MorphPawnKindExtension.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:43 PM
// last updated 09/15/2019  9:43 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
	/// <summary> Mod extension for applying morphs to various PawnKinds. </summary>
	public class MorphPawnKindExtension : DefModExtension, IDebugString
	{
		/// <summary>the min and max number of hediffs this kind can have</summary>
		public IntRange hediffRange;

		/// <summary>The range for the number of aspects that can be added</summary>
		public IntRange aspectRange;

		/// <summary>the chance that a given pawn will be a hybrid race</summary>
		/// percentage, [0,1]
		public float morphChance = 0.6f;

		/// <summary>
		///     if true, then any mutation from <see cref="morphs" /> can be picked
		///     other wise just one morph is picked
		/// </summary>
		public bool pickAnyMutation;

		/// <summary>The aspects that can be added by this extension </summary>
		public List<AspectEntry> aspects = new List<AspectEntry>();


		/// <summary>The morph categories that can be chosen from</summary>
		public List<MorphCategoryDef> morphCategories = new List<MorphCategoryDef>();

		/// <summary>
		///     The mutation categories that can be chosen from, this is in addition to those added by
		///     <seealso cref="morphCategories" />
		/// </summary>
		public List<MutationCategoryDef> mutationCategories = new List<MutationCategoryDef>();

		/// <summary>list of morphs to get mutations from</summary>
		[NotNull] public List<MorphDef> morphs = new List<MorphDef>();


		[Unsaved] private Dictionary<AspectDef, List<int>> _addDict;
		[Unsaved] private List<AspectDef> _allAspectDefs;


		private List<MorphDef> _allMorphs;

		[Unsaved] private List<MutationDef> _mutations;
		[Unsaved] private int _morphTries = 0;

		[NotNull]
		private List<MorphDef> AllMorphs
		{
			get
			{
				if (_allMorphs == null)
				{
					_allMorphs = GetAllMorphs();
				}
				else if (_allMorphs.Count == 0 && _morphTries < MAX_TRIES)
				{
					Log.Warning($"{nameof(MorphPawnKindExtension)} could not get morphs on try {_morphTries}, attempting to correct");
					_allMorphs.AddRange(GetAllMorphs());
				}

				return _allMorphs;
			}
		}

		[NotNull]
		private List<MorphDef> GetAllMorphs()
		{
			List<MorphDef> retList = new List<MorphDef>();
			foreach (MorphDef morphDef in morphCategories.MakeSafe().SelectMany(c => c.AllMorphsInCategories)) //get all morphs in the various categories 
			{
				if (!retList.Contains(morphDef) && morphDef.AllAssociatedMutations.Any())
				{
					retList.Add(morphDef);
				}
			}

			foreach (MorphDef morphDef in morphs) //add any morphs that are in the list 
			{
				if (!retList.Contains(morphDef) && morphDef.AllAssociatedMutations.Any())
				{
					retList.Add(morphDef);
				}
			}

			return retList;

		}

		private const int MAX_TRIES = 10;
		[Unsaved]
		private int _mutationTries;
		[NotNull]
		private IEnumerable<MutationDef> AllMutations
		{
			get
			{
				if (_mutations == null)
				{
					_mutations = new List<MutationDef>(GetAllMutations().Distinct());
				}
				else if (_mutations.Count == 0 && _mutationTries < MAX_TRIES)
				{
					Log.Warning($"could not get mutations for {nameof(MorphPawnKindExtension)} on try {_mutationTries}, attempting to correct");
					_mutations.AddRange(GetAllMutations().Distinct());
					_mutationTries++;
				}

				return _mutations;
			}
		}

		[NotNull]
		IEnumerable<MutationDef> GetAllMutations()
		{
			foreach (MutationDef mutation in AllMorphs.SelectMany(allMorph => allMorph.AllAssociatedMutations))
			{
				yield return mutation;
			}

			foreach (MutationDef mutation in mutationCategories.MakeSafe().SelectMany(m => m.AllMutations))
			{
				yield return mutation;
			}
		}



		/// <summary>Gets all aspect defs that can be added by this instance.</summary>
		/// <returns></returns>
		public IEnumerable<AspectDef> GetAllAspectDefs()
		{
			if (_allAspectDefs == null) _allAspectDefs = aspects.Select(a => a.aspect).Distinct().ToList();

			return _allAspectDefs;
		}

		/// <summary>Gets the available stages that can be added by the given aspect.</summary>
		/// <param name="def">The definition.</param>
		/// <returns></returns>
		public IEnumerable<int> GetAvailableStagesFor(AspectDef def)
		{
			if (_addDict == null)
			{
				_addDict = new Dictionary<AspectDef, List<int>>();
				IEnumerable<IGrouping<AspectDef, int>> grouping = aspects.GroupBy(a => a.aspect, a => a.stage);
				foreach (IGrouping<AspectDef, int> group in grouping) _addDict[group.Key] = group.Distinct().ToList();
			}

			return _addDict.TryGetValue(def) ?? Enumerable.Empty<int>();
		}

		/// <summary>
		/// Gets a random set of mutations to be added 
		/// </summary>
		/// <param name="seed">The seed.</param>
		/// <returns></returns>
		[NotNull]
		public IEnumerable<MutationDef> GetRandomMutations(int? seed = null)
		{
			if (pickAnyMutation) return AllMutations;
			if (seed != null) Rand.PushState(seed.Value);
			try
			{
				var mutations = new List<MutationDef>();
				GetMorphMutations(mutations);
				var distinctMCats = mutationCategories.MakeSafe()
													  .SelectMany(c => c.AllMutations)
													  .Distinct();
				foreach (MutationDef mutation in distinctMCats)
				{
					if (!mutations.Contains(mutation))
						mutations.Add(mutation);
				}

				return mutations;
			}
			finally
			{
				if (seed != null) Rand.PopState(); //make sure to always pop the rand state 
			}
		}

		private void GetMorphMutations([NotNull] List<MutationDef> mutations)
		{
			if (!AllMutations.Any()) return;
			MorphDef rMorph = AllMorphs.RandElement();
			mutations.AddRange(rMorph.AllAssociatedMutations);
		}


		/// <summary>
		/// Converts this object to a full debug string 
		/// </summary>
		/// <returns></returns>
		public string ToStringFull()
		{
			StringBuilder builder = new StringBuilder();
			if (mutationCategories != null)
			{
				builder.AppendLine($"{nameof(mutationCategories)}:[{mutationCategories.Join(d => d.defName)}]");
			}

			if (morphCategories != null)
				builder.AppendLine($"{nameof(morphCategories)}:[{morphCategories.Join(d => d.defName)}]");
			builder.AppendLine($"{nameof(morphs)}:[{morphs.Join(d => d.defName)}]");

			//all morph info 
			builder.AppendLine($"---Full Morph Info, total count{AllMorphs.Count}---");
			builder.AppendLine($"[{AllMorphs.Join(m => m.defName + "/" + m.label)}]");

			//total mutation info 
			builder.AppendLine($"---Full Mutation Info, total count={AllMutations.Count()}, {nameof(pickAnyMutation)}:{pickAnyMutation}---");
			builder.AppendLine($"[{AllMutations.Join(m => m.defName)}]");

			return builder.ToString();
		}

		/// <summary>
		///     class for a single aspect entry in the <see cref="MorphPawnKindExtension" />
		/// </summary>
		public class AspectEntry
		{
			/// <summary>The aspect to add</summary>
			public AspectDef aspect;

			/// <summary>The stage to add the aspect at</summary>
			public int stage = 0;
		}
	}
}