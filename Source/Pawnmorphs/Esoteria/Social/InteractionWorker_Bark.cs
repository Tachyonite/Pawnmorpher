using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// interaction worker for the bark interaction
    /// </summary>
    /// <seealso cref="RimWorld.InteractionWorker" />
    public class InteractionWorker_Bark : InteractionWorker
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
                {"EtherHuskyMuzzle",0.5f},
                {"EtherWargMuzzle",0.5f},
                {"EtherWolfMuzzle",0.5f},
                {"EtherWolfEar",0.5f},
                {"EtherWargEar",0.5f},
                {"EtherHuskyEar",0.5f},
                {"EtherWolfTail",1f},
                {"EtherWargTail",1f},
                {"EtherHuskyTail",1f},
                {"EtherPawFoot",0.5f},
                {"EtherThickFurLimb",0.5f},
                {"EtherFurredLimb",0.5f},
            };
            HediffSet hs = initiator.health.hediffSet;

            if (initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherHuskyMuzzle")) ||
                initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherWargMuzzle")) ||
                initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherWolfMuzzle")))
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
