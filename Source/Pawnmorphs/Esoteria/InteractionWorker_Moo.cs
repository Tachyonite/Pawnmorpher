using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class InteractionWorker_Moo : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float weight = 0f;
            Dictionary<string, float> dicc = new Dictionary<string, float>()
            {
                {"EtherCowEar",0.5f},
                {"EtherCowTail",1f},
                {"EtherUdder",2f},
                {"EtherHoofHand",1f},
                {"EtherClovenHoofFoot",0.5f},
                {"EtherHorns",0.5f},
            };
            HediffSet hs = initiator.health.hediffSet;

            if (initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherCowSnout")))
            {
                foreach(KeyValuePair<string,float> pair in dicc)
                {
                    if (hs.HasHediff(HediffDef.Named(pair.Key))){
                        weight += pair.Value;
                    }
                }
                if (hs.HasHediff(HediffDef.Named("EtherUdder")))
                {
                    weight += hs.hediffs.Find(x => x.def.defName == "EtherUdder").Severity * 3;
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
