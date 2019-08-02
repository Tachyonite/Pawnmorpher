// CompProperties_MorphInfluence.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:52 PM
// last updated 08/02/2019  2:52 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    public class Comp_MorphInfluence : HediffComp
    {
        public CompProperties_MorphInfluence Props => (CompProperties_MorphInfluence) props; 
    }


    public class CompProperties_MorphInfluence : HediffCompProperties
    {
        public MorphDef morph;
        public float influence = 1; 


        public CompProperties_MorphInfluence()
        {
            compClass = typeof(Comp_MorphInfluence); 
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string configError in base.ConfigErrors(parentDef))
            {
                yield return configError;
            }

            if (morph == null)
            {
                yield return $"{parentDef.defName}'s MorphInfluence component has a null morph!";
            }
        }
    }
}