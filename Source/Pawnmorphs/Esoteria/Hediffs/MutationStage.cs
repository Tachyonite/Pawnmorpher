// DescriptiveStage.cs modified by Iron Wolf for Pawnmorph on 12/14/2019 7:32 PM
// last updated 12/14/2019  7:32 PM

using System;
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

        /// <summary>
        /// The skip aspects
        /// </summary>
        public List<AspectEntry> skipAspects;

        /// <summary>
        /// Gets the skip aspects.
        /// </summary>
        /// <value>
        /// The skip aspects.
        /// </value>
        [NotNull]
        public IReadOnlyList<AspectEntry> SkipAspects => ((IReadOnlyList<AspectEntry>) skipAspects) ?? Array.Empty<AspectEntry>();


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
                hediff.pawn.TryAddMutationThought(memory); 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class AspectEntry
        {
            /// <summary>
            /// The aspect the pawn must have 
            /// </summary>
            public AspectDef aspect;
            /// <summary>
            /// The stage the aspect must be in to satisfy this entry, if null any stage will do 
            /// </summary>
            public int? stage;

            /// <summary>
            /// checks if the given pawn satisfies this entry 
            /// </summary>
            /// <param name="pawn">The pawn.</param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">pawn</exception>
            public bool Satisfied([NotNull] Pawn pawn)
            {
                if (pawn == null) throw new ArgumentNullException(nameof(pawn));
                var asTracker = pawn.GetAspectTracker();
                var pAspect = asTracker?.GetAspect(this.aspect);
                if (pAspect == null) return false;
                return stage == null || stage == pAspect.StageIndex; 
            }
        }
    }
}