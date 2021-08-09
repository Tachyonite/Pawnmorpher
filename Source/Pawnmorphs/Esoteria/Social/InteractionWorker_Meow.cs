using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// interaction worker for the meow interaction 
    /// </summary>
    /// <seealso cref="RimWorld.InteractionWorker" />
    public class InteractionWorker_Meow : InteractionWorker
    {
        /// <summary>gets the random selection weight.</summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float weight = 0f;
            Dictionary<string, float> dicc = new Dictionary<string, float>()
            {
                {"EtherCatEar",0.5f},
                {"EtherCatTail",1f},
                {"EtherCatMuzzle",2f},
                {"EtherPawHand",1f},
                {"EtherDigitigradeLeg",0.5f},
                {"EtherPawFoot",0.5f},
                {"EtherFurredLimb",0.2f},
                {"EtherCatEye",0.2f},
            };
            HediffSet hs = initiator.health.hediffSet;

            if (initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherCatMuzzle")))
            {
                foreach (KeyValuePair<string, float> pair in dicc)
                {
                    if (hs.HasHediff(HediffDef.Named(pair.Key)))
                    {
                        weight += pair.Value;
                    }
                }
                return weight;
            }
            else
            {
                return 0f;
            }
        }
    }
}
