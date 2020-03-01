// ConditionalSapientAnimal.cs created by Iron Wolf for Pawnmorph on 03/01/2020 11:00 AM
// last updated 03/01/2020  11:00 AM

using Verse;
using Verse.AI;

namespace Pawnmorph.ThinkNodes
{
    /// <summary>
    /// conditional think nodes for sapient animals 
    /// </summary>
    /// <seealso cref="Verse.AI.ThinkNode_Conditional" />
    public class ConditionalSapientAnimal : ThinkNode_Conditional
    {
        /// <summary>
        /// if the pawn must be a colonist to 
        /// </summary>
        public bool mustBeColonist = true;
        /// <summary>
        /// checks if this think node is satisfied or not 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.IsSapientFormerHuman()) return false;
            if (mustBeColonist) return pawn.IsColonist;
            return true; 
        }
    }
}