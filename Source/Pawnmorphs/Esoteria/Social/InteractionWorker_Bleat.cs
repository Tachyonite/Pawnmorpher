using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// interaction worker for the bleat interaction 
    /// </summary>
    /// <seealso cref="RimWorld.InteractionWorker" />
    public class InteractionWorker_Bleat : InteractionWorker
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
                {"EtherSheepEar",0.5f},
                {"EtherSheepTail",1f},
                {"EtherSheepSnout",2f},
                {"EtherHoofHand",1f},
                {"EtherDigitigradeLeg",0.5f},
                {"EtherClovenHoofFoot",0.5f},
                {"EtherWoolySheep",1f},
                {"EtherSheepEye",0.2f},
            };
            HediffSet hs = initiator.health.hediffSet;

            if (initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherSheepSnout")))
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
