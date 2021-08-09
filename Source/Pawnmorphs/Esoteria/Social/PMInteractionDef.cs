// Def_PMInteraction.cs modified by Iron Wolf for Pawnmorph on 08/30/2019 8:46 AM
// last updated 08/30/2019  8:46 AM

using System.Collections.Generic;
using RimWorld;

namespace Pawnmorph.Social
{
    /// <summary>
    /// def for pawnmorph specific interactions 
    /// </summary>
    /// <seealso cref="RimWorld.InteractionDef" />
    public class PMInteractionDef : InteractionDef
    {
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

        /// <summary>The weights applied based on the initator's mutations</summary>
        public PMInteractionWeightsDef initiatorWeights;

        /// <summary>The weights applied based on the recipients's mutations</summary>
        public PMInteractionWeightsDef recipientWeights;

        /// <summary>if both the initiator and recipient need to have non-zero weights for the resultant weight to be non zero </summary>
        public bool requiresBoth;

    }
}