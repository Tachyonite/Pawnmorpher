// MorphPawnKindExtension.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:43 PM
// last updated 09/15/2019  9:43 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
    /// <summary> Mod extension for applying morphs to various PawnKinds. </summary>
    public class MorphPawnKindExtension : DefModExtension
    {
        /// <summary>The maximum hediffs to add </summary>
        public int maxHediffs = 5;
        /// <summary>The maximum aspects to add</summary>
        public int maxAspects = 1;
        /// <summary>The morph change</summary>
        /// percentage, [0,1]
        public float morphChange = 0.6f;

        /// <summary>The morph categories that can be chosen from</summary>
        public List<MorphCategoryDef> morphCategories = new List<MorphCategoryDef>();
        /// <summary>The mutation categories that can be chosen from</summary>
        public List<MutationCategoryDef> mutationCategories = new List<MutationCategoryDef>(); 
    }
}