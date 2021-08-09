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
        /// <summary>gets the random selection weight.</summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return GetBaseWeight(initiator, recipient); 
        }
    }

    /// <summary>
    /// interaction worker that functions like InteractionWorker_DeepTalk
    /// </summary>
    /// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
    public class InteractionWorker_DeepTalk : PMInteractionWorkerBase
    {
        private SimpleCurve CompatibilityFactorCurve = new SimpleCurve {
            {
                new CurvePoint (-1.5f, 0f),
                true
            },
            {
                new CurvePoint (-0.5f, 0.1f),
                true
            },
            {
                new CurvePoint (0.5f, 1f),
                true
            },
            {
                new CurvePoint (1f, 1.8f),
                true
            },
            {
                new CurvePoint (2f, 3f),
                true
            }
        };

        /// <summary>gets the selection weight.</summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0.075f
                * CompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient))
                * GetBaseWeight(initiator, recipient);
        }
    }

    /// <summary>
    /// interaction worker that functions like InteractionWorker_KindWords
    /// </summary>
    /// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
    public class InteractionWorker_KindWords : PMInteractionWorkerBase
    {
        /// <summary>gets the selection weight.</summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float weight = initiator.story.traits.HasTrait(TraitDefOf.Kind) ? 0.01f : 0;
            return weight * GetBaseWeight(initiator, recipient); 
        }
    }

    /// <summary>
    /// interaction worker that functions like InteractionWorker_Slight 
    /// </summary>
    /// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
    public class InteractionWorker_Slight : PMInteractionWorkerBase
    {
        /// <summary>gets the selection weight.</summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
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
        /// <summary>gets the selection weight.</summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0.007f
                * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient)
                * GetBaseWeight(initiator, recipient) ;
        }
    }

}