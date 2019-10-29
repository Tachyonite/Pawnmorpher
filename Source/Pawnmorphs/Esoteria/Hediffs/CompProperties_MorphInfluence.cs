// CompProperties_MorphInfluence.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:52 PM
// last updated 08/02/2019  2:52 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff comp that adds influence of a certain morph to the pawn 
    /// </summary>
    public class Comp_MorphInfluence : HediffComp
    {
        /// <summary>
        /// the props for this comp 
        /// </summary>
        public CompProperties_MorphInfluence Props => (CompProperties_MorphInfluence) props;

        /// <summary>
        /// the morph this comp adds influence for 
        /// </summary>
        public MorphDef Morph => Props.morph;
        /// <summary>
        /// the amount of influence this adds 
        /// </summary>
        public float Influence => Props.influence;
    }

    /// <summary>
    /// the props for the morph influence comp 
    /// </summary>
    public class CompProperties_MorphInfluence : HediffCompProperties
    {
        /// <summary>
        /// the morph to add influence for 
        /// </summary>
        public MorphDef morph;
            /// <summary>
            /// the amount of influence to add 
            /// </summary>
        public float influence = 1; 


            /// <summary>
            /// create a new instance of this class
            /// </summary>
        public CompProperties_MorphInfluence()
        {
            compClass = typeof(Comp_MorphInfluence); 
        }

            /// <summary>
            /// return all configuration errors with this instance 
            /// </summary>
            /// <param name="parentDef"></param>
            /// <returns></returns>
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