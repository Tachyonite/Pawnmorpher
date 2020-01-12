// MutationCategoryDef.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:00 PM
// last updated 09/15/2019  9:00 PM

using System;
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


        [NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
        private List<MutationDef> mutations = new List<MutationDef>(); 


        /// <summary>if mutations in this category should be restricted to special PawnGroupKinds</summary>
        public bool restricted;

        [Unsaved] private List<MutationDef> _allMutations;

        /// <summary>
        /// Gets all mutations in this category 
        /// </summary>
        /// <value>
        /// All mutations.
        /// </value>
        public IEnumerable<MutationDef> AllMutations
        {
            get
            {
                if (_allMutations == null)
                {
                    _allMutations = new List<MutationDef>(mutations);

                    foreach (MutationDef mutation in MutationDef.AllMutations)
                    {
                        if (!_allMutations.Contains(mutation))
                            _allMutations.Add(mutation); 
                    }

                }

                return _allMutations; 
            }
        }

    }
}