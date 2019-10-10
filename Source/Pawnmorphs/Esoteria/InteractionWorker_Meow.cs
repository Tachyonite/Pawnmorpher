using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class InteractionWorker_Meow : InteractionWorker
    {
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
