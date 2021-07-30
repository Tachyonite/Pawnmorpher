// ApplyHaltingCream.cs created by Iron Wolf for Pawnmorph on 07/30/2021 7:52 AM
// last updated 07/30/2021  7:52 AM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.RecipeWorkers
{

    /// <summary>
    /// recipe worker for applying halting cream 
    /// </summary>
    /// <seealso cref="Pawnmorph.RecipeWorkers.ApplyToMutatedPart" />
    public class ApplyHaltingCream : ApplyToMutatedPart
    {
        /// <summary>
        /// applies the effect onto the given mutation. can be called multiple times on the same pawn 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="billDoer">The bill doer.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="ingredients">The ingredients.</param>
        protected override void ApplyOnMutation(Pawn pawn, Pawn billDoer, Hediff_AddedMutation mutation, IReadOnlyList<Thing> ingredients)
        {
            var medSkill = billDoer?.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? 0;
            
            var haltComp = mutation.SeverityAdjust;
            if (haltComp == null) return;
            if(!haltComp.Halted)
                haltComp.Halted = Rand.Value < 0.3f; //TODO use medskill to give a better chance of halting



        }
    }
}