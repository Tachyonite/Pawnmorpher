// Def_MorphThought.cs modified by Iron Wolf for Pawnmorph on 09/26/2019 8:52 PM
// last updated 09/26/2019  8:52 PM

using System.Collections.Generic;
using RimWorld;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// def for a thought that has a morph dependency 
    /// </summary>
    public class Def_MorphThought : ThoughtDef
    {
        public MorphDef morph;
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError;
            }

            if (morph == null) yield return "no morphDef set"; 
        }
    }
}