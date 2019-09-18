// Worker_HasMutations.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:14 PM
// last updated 09/18/2019  2:14 PM

using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    public class Worker_HasMutations : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var numMutations = p.health.hediffSet.hediffs.Count(h => h is Hediff_AddedMutation);
            if (numMutations == 0) return false;

            var num = Mathf.Min(def.stages.Count - 1, numMutations);
            return ThoughtState.ActiveAtStage(num); 
            
        }
    }
}