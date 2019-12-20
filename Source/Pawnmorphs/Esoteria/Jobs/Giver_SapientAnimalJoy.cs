// Giver_SapientAnimalJoy.cs modified by Iron Wolf for Pawnmorph on 12/19/2019 7:57 PM
// last updated 12/19/2019  7:57 PM

using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    /// <summary>
    /// job giver to give sapient animals joy jobs 
    /// </summary>
    /// <seealso cref="RimWorld.JobGiver_GetJoy" />
    public class Giver_SapientAnimalJoy : JobGiver_GetJoy
    {
        /// <summary>
        /// Tries to give the pawn a job from joy giver definition
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        protected override Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
        {
            if (!def.IsValidFor(pawn)) return null; 
            return base.TryGiveJobFromJoyGiverDefDirect(def, pawn);
        }
    }
}