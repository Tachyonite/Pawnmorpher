// MorphCategoryDef.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:09 PM
// last updated 09/15/2019  9:09 PM

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph
{
    /// <summary> Def for representing the 'category' a morph can be in. </summary>
    public class MorphCategoryDef : Def
    {
        [Unsaved] private List<MorphDef> _allMorphs;

        public IEnumerable<MorphDef> AllMorphsInCategories
        {
            get
            {
                if (_allMorphs == null)
                {
                    _allMorphs = DefDatabase<MorphDef>.AllDefs.Where(m => m.categories.Contains(this)).ToList();
                }

                return _allMorphs; 
            }
        }
    }
}