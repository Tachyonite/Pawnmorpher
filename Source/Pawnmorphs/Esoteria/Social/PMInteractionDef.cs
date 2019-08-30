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
        public class Weights
        {
            public Dictionary<HediffDef, float> mutationWeights = new Dictionary<HediffDef, float>();
            public Dictionary<MorphDef, float> morphWeights = new Dictionary<MorphDef, float>();
            public bool requiresBoth; //if the pawn needs both a mutation and be a morph to get a non zero weight 

            public float GetTotalWeight(Pawn pawn)
            {
                var hediffs = pawn.health.hediffSet.hediffs.Select(h => h.def);

                float total = 0;
                bool hasMutation = false;

                foreach (HediffDef hediffDef in hediffs)
                {
                    if (mutationWeights.TryGetValue(hediffDef, out float v))
                    {
                        hasMutation = true;
                        total += v;
                    }
                }

                bool isMorph = RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef morph);

                if (isMorph)
                {
                    isMorph = morphWeights.TryGetValue(morph, out float w);
                    total += w;
                }

                if (requiresBoth && (!isMorph && !hasMutation))
                {
                    total = 0; 
                }

                return total; 


            }


        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError;
            }

            if (!(Worker is PMInteractionWorkerBase))
                yield return "Worker type is not derived from " + nameof(PMInteractionWorkerBase);


        }

        public Weights initiatorWeights;
        public Weights recipientWeights;
        public bool requiresBoth; //if both the initiator and recipient need to have non zero weights for the resultant weight to be 
                                    //non zero 

    }

    

}