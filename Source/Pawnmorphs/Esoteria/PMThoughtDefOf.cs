// PMThoughtDefOf.cs modified by Iron Wolf for Pawnmorph on 09/28/2019 8:35 AM
// last updated 09/28/2019  8:35 AM

using System;
using RimWorld;

namespace Pawnmorph
{
    /// <summary>
    /// DefOf class for commonly referenced ThoughtDefs 
    /// </summary>
    [DefOf]
    public static class PMThoughtDefOf
    {
        static PMThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDef)); 
        }


        /// <summary>
        /// get the correct default memory for a pawn that was a morph that returns to being human 
        /// </summary>
        /// <param name="mutationOutlook">the mutation outlook of the pawn</param>
        /// <returns></returns>
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

        /// <summary>
        /// default thought for pawns that were a morph that reverts back to a human 
        /// </summary>
        public static ThoughtDef DefaultMorphRevertsToHuman;

        /// <summary>
        /// default thought for pawns that have the MutationAffinity Trait and were a morph that reverts back to human 
        /// </summary>
        public static ThoughtDef DefaultMorphRevertsToHumanFurry;

        /// <summary>
        /// default thought for pawns that have the MutationAffinity Trait and were a morph that reverts back to human 
        /// </summary>
        public static ThoughtDef DefaultMorphRevertsToHumanBP;
    }
}