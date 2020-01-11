// MutationCategoryDef.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:00 PM
// last updated 09/15/2019  9:00 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def representing a 'category' of mutations 
    /// </summary>
    public class MutationCategoryDef : Def
    {

        [Unsaved] private List<HediffDef> _allMutations;

        [NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
        private List<HediffDef> mutations = new List<HediffDef>(); 


        /// <summary>if mutations in this category should be restricted to special PawnGroupKinds</summary>
        public bool restricted; 

        /// <summary> An enumerable collection of all mutations within this category. </summary>
        [NotNull]
        public IEnumerable<HediffDef> AllMutationsInCategory
        {
            get
            {
                if (_allMutations == null)
                {
                    _allMutations = new List<HediffDef>(mutations.MakeSafe());

                    foreach (HediffDef mutationDef in DefDatabase<HediffDef>.AllDefs)
                    {
                        List<MutationCategoryDef> categories = (mutationDef as MutationDef)?.categories;
                        if (categories.MakeSafe().Contains(this))
                        {
                            if (_allMutations.Contains(mutationDef))
                                Log.Warning($"hediff {mutationDef.defName} is added to {defName} in both {defName} and with the def extension, this is redundant");
                            else
                                _allMutations.Add(mutationDef);
                        }
                    }
                }

                return _allMutations; 
            }
        }
    }
}