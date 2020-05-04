using System;
//using Multiplayer.API;
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
        /// <summary>Does the ingestion outcome special.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="ingested">The ingested.</param>
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            //if (MP.IsInMultiplayer)
            //{
            //    Rand.PushState(RandUtilities.MPSafeSeed); 
            //}

            try
            {
                if (pawn.HasSapienceState())
                {
                    pawn.health.AddHediff(TfHediffDefOf.FeralPillSapienceDrop);
                    return; 
                }


                foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Loop through all the hediffs on the pawn.
                {
                    if(hediff?.def == null) continue;
                    foreach (IPawnTransformer pawnTransformer in hediff.def.GetAllTransformers())
                    {
                        if (pawnTransformer.TransformPawn(pawn, hediff)) return; //break at the first transformer that works 
                    }
                }

              
            }
            finally
            {
                //if (MP.IsInMultiplayer)
                //{
                //    Rand.PopState();
                //}
            }
        }
    }
}
