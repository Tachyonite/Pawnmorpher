// Def_PMInteraction.cs modified by Iron Wolf for Pawnmorph on 08/30/2019 8:46 AM
// last updated 08/30/2019  8:46 AM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Hybrids;
using RimWorld;
using Verse;

namespace Pawnmorph.Social
{
    /// <summary>
    /// def for pawnmorph specific interactions 
    /// </summary>
    /// <seealso cref="RimWorld.InteractionDef" />
    public class PMInteractionDef : InteractionDef
    {
        /// <summary>
        /// class that represents a single weight entry 
        /// </summary>
        public class Weights
        {
            /// <summary>The mutation weights</summary>
            public Dictionary<HediffDef, float> mutationWeights = new Dictionary<HediffDef, float>();
            /// <summary>The morph weights</summary>
            public Dictionary<MorphDef, float> morphWeights = new Dictionary<MorphDef, float>();
            /// <summary>if the pawn needs both a mutation and be a morph to get a non zero weight </summary>
            public bool requiresBoth;

            /// <summary>Gets the total weight.</summary>
            /// the higher the weight the more likely this interaction is going to be picked 
            /// <param name="pawn">The pawn.</param>
            /// <returns></returns>
            public float GetTotalWeight(Pawn pawn)
            {
                IEnumerable<HediffDef> hediffs = pawn.health.hediffSet.hediffs.Select(h => h.def);

                float total = 0;
                var hasMutation = false;

                foreach (HediffDef hediffDef in hediffs)
                    if (mutationWeights.TryGetValue(hediffDef, out float v))
                    {
                        hasMutation = true;
                        total += v;
                    }

                bool isMorph = RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef morph);

                if (isMorph)
                {
                    isMorph = morphWeights.TryGetValue(morph, out float w);
                    total += w;
                }

                if (requiresBoth && !isMorph && !hasMutation) total = 0;

                return total;
            }
        }

        /// <summary>Get all Configuration Errors with this instance</summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError;
            }

            if (!(Worker is PMInteractionWorkerBase))
                yield return "Worker type is not derived from " + nameof(PMInteractionWorkerBase);


        }

        /// <summary>The initiator weights</summary>
        public Weights initiatorWeights;

        /// <summary>The recipient weights</summary>
        public Weights recipientWeights;

        /// <summary>if both the initiator and recipient need to have non zero weights for the resultant weight to be non zero </summary>
        public bool requiresBoth;

    }

    

}