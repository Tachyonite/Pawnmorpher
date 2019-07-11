using RimWorld;
using Verse;
using System.Collections.Generic;

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

    public class InteractionWorker_Bark : InteractionWorker
    {
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

    public class InteractionWorker_Yip : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float weight = 0f;
            Dictionary<string, float> dicc = new Dictionary<string, float>()
            {
                {"EtherFoxEar",0.5f},
                {"EtherFluffyTail",1f},
                {"EtherFoxMuzzle",2f},
                {"EtherPawHand",1f},
                {"EtherDigitigradeLeg",0.5f},
                {"EtherPawFoot",0.5f},
                {"EtherFurredLimb",0.2f},
                {"EtherFoxEye",0.2f},
            };
            HediffSet hs = initiator.health.hediffSet;

            if (initiator.health.hediffSet.HasHediff(HediffDef.Named("EtherFoxMuzzle")))
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
