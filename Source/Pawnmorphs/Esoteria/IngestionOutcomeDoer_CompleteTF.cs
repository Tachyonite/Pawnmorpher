using System;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// ingestion outcome doer that forces a full transformation
    /// </summary>
    /// <seealso cref="RimWorld.IngestionOutcomeDoer" />
    public class IngestionOutcomeDoer_CompleteTF : IngestionOutcomeDoer
    {
        /// <summary>
        /// if true then to complete the tf the pawn must be in a 'reeling' state
        /// </summary>
        public bool mustBeReeling; 

        /// <summary>Does the ingestion outcome special.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="ingested">The ingested.</param>
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (pawn.HasSapienceState())
            {
                pawn.health.AddHediff(TfHediffDefOf.FeralPillSapienceDrop);
                return; 
            }

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Loop through all the hediffs on the pawn.
            {
                if(hediff?.def == null) continue;

                if (TryForceTransformation(pawn, hediff)) return;
            }
        }

        private bool TryForceTransformation([NotNull] Pawn pawn, [NotNull] Hediff hediff)
        {
            if (mustBeReeling)
            {
                var tfHediff = hediff as IMutationHediff;
                // TODO this should probably be changed to CurrentStageHasTransformations
                if (tfHediff?.CurrentStageHasMutations != false) return false; 
            }


            foreach (IPawnTransformer pawnTransformer in hediff.def.GetAllTransformers())
                if (pawnTransformer.TransformPawn(pawn, hediff))
                    return true;

            return false;
            
        }
    }
}
