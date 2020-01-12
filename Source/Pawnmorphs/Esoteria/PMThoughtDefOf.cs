// PMThoughtDefOf.cs modified by Iron Wolf for Pawnmorph on 09/28/2019 8:35 AM
// last updated 09/28/2019  8:35 AM

using System;
using RimWorld;
#pragma warning disable 1591
namespace Pawnmorph
{
    /// <summary> DefOf class for commonly referenced ThoughtDefs. </summary>
    [DefOf]
    public static class PMThoughtDefOf
    {
        static PMThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDef)); 
        }

        /// <summary> Get the correct default memory for a pawn that was a morph that returns to being human. </summary>
        /// <param name="mutationOutlook">the mutation outlook of the pawn</param>
        /// <returns> The ThoughtDef of the thought to use based on their traits. </returns>
        public static ThoughtDef GetDefaultMorphRevertThought(MutationOutlook mutationOutlook)
        {

            switch (mutationOutlook)
            {
                case MutationOutlook.Neutral:
                    return DefaultMorphRevertsToHuman; 
                case MutationOutlook.Furry:
                    return DefaultMorphRevertsToHumanFurry;
                case MutationOutlook.BodyPurist:
                    return DefaultMorphRevertsToHumanBP;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mutationOutlook), mutationOutlook, null);
            }
        }

        public static ThoughtDef DefaultMorphTfMemory;

        /// <summary> Default thought for pawns that were a morph that reverts back to a human. </summary>
        public static ThoughtDef DefaultMorphRevertsToHuman;

        /// <summary> Default thought for pawns that have the MutationAffinity Trait and were a morph that reverts back to human. </summary>
        public static ThoughtDef DefaultMorphRevertsToHumanFurry;

        /// <summary> Default thought for pawns that have the BodyPurist Trait and were a morph that reverts back to human. </summary>
        public static ThoughtDef DefaultMorphRevertsToHumanBP;

        /// <summary>
        /// The former human taming success thought
        /// </summary>
        public static ThoughtDef FormerHumanTameThought; 

        /// <summary>
        /// default thought for when a sapient animal sleeps on the ground 
        /// </summary>
        public static ThoughtDef SapientAnimalSleptOnGround;
        /// <summary>
        /// default thought for when a sapient animal is milked 
        /// </summary>
        public static ThoughtDef SapientAnimalMilked;

        /// <summary>
        /// The sapient animal hunting memory
        /// </summary>
        /// this is for hunting out of necessity not for the hunting mental break 
        public static ThoughtDef SapientAnimalHuntingMemory; 

    }
}