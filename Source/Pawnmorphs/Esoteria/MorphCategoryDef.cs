// MorphCategoryDef.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:09 PM
// last updated 09/15/2019  9:09 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph
{
	/// <summary> Def for representing the 'category' a morph can be in. </summary>
	public class MorphCategoryDef : Def
	{
		[Unsaved] private List<MorphDef> _allMorphs;


		/// <summary>
		/// The associated mutation category with this morph category, all mutations directly associated with a morph in this category will be in this category 
		/// </summary>
		[UsedImplicitly, CanBeNull]
		public MutationCategoryDef associatedMutationCategory;

		/// <summary>
		/// if morphs in this category should be considered 'restricted'
		/// </summary>
		public bool restricted;

		/// <summary>Gets all morphs in this category.</summary>
		/// <value>All morphs in categories.</value>
		[NotNull]
		public IEnumerable<MorphDef> AllMorphsInCategories
		{
			get
			{
				return _allMorphs;
			}
		}

		/// <summary>
		/// Resolves the references.
		/// </summary>
		public override void ResolveReferences()
		{
			_allMorphs = new List<MorphDef>();
			foreach (MorphDef morph in MorphDef.AllDefs)
			{
				if (morph.categories?.Contains(this) == true)
				{
					_allMorphs.Add(morph);
					if (associatedMutationCategory == null) continue;
					foreach (MutationDef mutation in MutationDef.AllMutations)
					{
						if (mutation.ClassInfluences.Contains(morph))
						{
							mutation.categories = mutation.categories ?? new List<MutationCategoryDef>();
							if (!mutation.categories.Contains(associatedMutationCategory))
								mutation.categories.Add(associatedMutationCategory);
						}
					}
				}
			}
		}
	}
}