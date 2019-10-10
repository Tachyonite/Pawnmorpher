// MutationCategoryDef.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:00 PM
// last updated 09/15/2019  9:00 PM

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def representing a 'category' of mutations 
    /// </summary>
    public class MutationCategoryDef : Def
    {

        [Unsaved] private List<HediffDef> _mutations;

        /// <summary> An enumerable collection of all mutations within this category. </summary>
        public IEnumerable<HediffDef> AllMutationsInCategory
        {
            get
            {
                if (_mutations == null)
                {
                    _mutations = 
                        DefDatabase<HediffDef>.AllDefs
                        .Where(d => d.GetModExtension<MutationHediffExtension>()
                        ?.categories.Contains(this) ?? false)
                        .ToList();
                }

                return _mutations; 
            }
        }
    }
}