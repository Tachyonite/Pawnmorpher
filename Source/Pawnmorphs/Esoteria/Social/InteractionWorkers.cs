// InteractionWorkers.cs modified by Iron Wolf for Pawnmorph on 08/30/2019 9:14 AM
// last updated 08/30/2019  9:14 AM

using RimWorld;
using Verse;

namespace Pawnmorph.Social
{
    /// <summary>
    /// interaction worker that functions like chitchat worker 
    /// </summary>
    /// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
    public class InteractionWorker_Chitchat : PMInteractionWorkerBase
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return  GetBaseWeight(initiator, recipient); 
        }
    }
    
    
    /// <summary>
    /// interaction worker that functions like InteractionWorker_KindWords
    /// </summary>
    /// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
    public class InteractionWorker_KindWords : PMInteractionWorkerBase
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float weight ;

            weight = initiator.story.traits.HasTrait(TraitDefOf.Kind) ? 0.01f : 0;

            return weight * GetBaseWeight(initiator, recipient); 
        }
    }

    /// <summary>
    /// interaction worker that functions like InteractionWorker_Slight 
    /// </summary>
    /// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
    public class InteractionWorker_Slight : PMInteractionWorkerBase
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0.02f
                 * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient)
                 * GetBaseWeight(initiator, recipient); 
        }
    }

    /// <summary>
    /// interaction worker that works like base InteractionWorker_Insult
    /// </summary>
    /// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
    public class InteractionWorker_Insult : PMInteractionWorkerBase
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0.007f * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient) * GetBaseWeight(initiator, recipient) ;
        }
    }

}