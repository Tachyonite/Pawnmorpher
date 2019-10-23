// MorphGroupDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/09/2019 9:07 AM
// last updated 09/09/2019  9:07 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// Def for morph groups. <br/>
    /// i.e. Packs, Herds, ect.
    /// </summary>
    public class MorphGroupDef : Def
    {
        /// <summary> A list of all morph types that are of this group. </summary>
        [Unsaved] private List<MorphDef> _associatedMorphs;

        ///hediff to give to morphs in this group,
        [CanBeNull]
        [Obsolete("use the new aspects")]
        public HediffDef hediff; 

        /// <summary>The aspect definition to add to all morphs in this group</summary>
        [CanBeNull] public AspectDef aspectDef; 

        /// <summary> An enumerable collection of all morphs in this group.</summary>
        public IEnumerable<MorphDef> MorphsInGroup => _associatedMorphs ?? (_associatedMorphs = DefDatabase<MorphDef>.AllDefs.Where(def => def.@group == this).ToList());
    }
}