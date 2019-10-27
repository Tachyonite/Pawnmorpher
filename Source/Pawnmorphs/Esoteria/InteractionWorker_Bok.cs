using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class InteractionWorker_Bok : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float weight = 0f;
            Dictionary<string, float> dicc = new Dictionary<string, float>()
            {
                {"EtherWing",0.5f},
                {"EtherTailfeathers",1f},
                {"EtherEggLayer",2f},
                {"EtherWingTip",1f},
                {"EtherAvianFoot",0.5f},
                {"EtherFeatheredLimb",0.5f},
            };
            HediffSet hs = initiator.health.hediffSet;

            if (initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherBeak")))
            {
                foreach (KeyValuePair<string, float> pair in dicc)
                {
                    if (hs.HasHediff(HediffDef.Named(pair.Key)))
                    {
                        weight += pair.Value;
                    }
                }
                if (hs.HasHediff(HediffDef.Named("EtherEggLayer")))
                {
                    weight += hs.hediffs.Find(x => x.def.defName == "EtherEggLayer").Severity * 3;
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
