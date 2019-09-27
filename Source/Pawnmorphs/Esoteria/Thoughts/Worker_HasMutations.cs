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
            
            var mutTracker = p.GetMutationTracker();
            if (mutTracker == null) return false;
            if (!mutTracker.AllMutations.Any()) return false;

            var humanInfluence = 1 - mutTracker.TotalNormalizedInfluence;

            //now get the stage number 


            var num = Mathf.FloorToInt(humanInfluence * def.stages.Count);
            num = Mathf.Clamp(num, 0, def.stages.Count - 1); 
            return ThoughtState.ActiveAtStage(num); 
            
        }
    }
}