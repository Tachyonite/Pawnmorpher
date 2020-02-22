// DescriptiveStage.cs modified by Iron Wolf for Pawnmorph on 12/14/2019 7:32 PM
// last updated 12/14/2019  7:32 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff stage with an extra description field  
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    public class MutationStage : HediffStage, IDescriptiveStage, IExecutableStage
    {
        /// <summary>
        /// list of all aspect givers in this stage 
        /// </summary>
        [CanBeNull]
        public List<AspectGiver> aspectGivers; 

        /// <summary>
        /// optional description override for a hediff in this stage 
        /// </summary>
        public string description;

        /// <summary>
        /// The label override
        /// </summary>
        public string labelOverride;

        /// <summary>
        /// the base chance that the mutation will stop progressing at this stage  
        /// </summary>
        /// this should be in [0,1]
        public float stopChance;

        /// <summary>
        /// memory to add when this stage is entered 
        /// </summary>
        public ThoughtDef memory; 

        string IDescriptiveStage.DescriptionOverride => description;
        string IDescriptiveStage.LabelOverride => labelOverride;

        /// <summary>called when the given hediff enters this stage</summary>
        /// <param name="hediff">The hediff.</param>
        public void EnteredStage(Hediff hediff)
        {
            if (aspectGivers != null)
            {
                foreach (AspectGiver aspectGiver in aspectGivers)
                {
                    aspectGiver.TryGiveAspects(hediff.pawn); 
                }
            }

            if (memory != null)
            {
                hediff.pawn?.TryGainMemory(memory); 
            }
        }
    }
}