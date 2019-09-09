// MorphGroupDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/09/2019 9:07 AM
// last updated 09/09/2019  9:07 AM

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def for 'morph groups' i
    /// ie packs,herds, ect  
    /// </summary>
    public class MorphGroupDef : Def
    {
        [Unsaved]
        private List<MorphDef> _associatedMorphs;

        public HediffDef hediff;

 


        /// <summary>
        /// an enumerable collection of all morphs in this group 
        /// </summary>
        public IEnumerable<MorphDef> MorphsInGroup
        {
            get
            {
                return _associatedMorphs
                    ?? (_associatedMorphs = DefDatabase<MorphDef>.AllDefs.Where(def => def.group == this).ToList());
            }
        }




    }
}