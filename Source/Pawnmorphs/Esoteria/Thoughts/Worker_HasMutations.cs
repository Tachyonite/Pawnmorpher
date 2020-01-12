// Worker_HasMutations.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:14 PM
// last updated 09/18/2019  2:14 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// thought worker who's state depends on how many mutations a pawn has 
    /// </summary>
    public class Worker_HasMutations : ThoughtWorker
    {

        private List<Thought> _scratchList = new List<Thought>(); 



        /// <summary>
        /// return the thought state for the given pawn 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!def.IsValidFor(p)) return false;
            MutationTracker mutTracker = p.GetMutationTracker();
            if (mutTracker == null) return false;
            
            if (!mutTracker.AllMutations.Any()) return false;
            
            var animalInfluence = mutTracker.TotalNormalizedInfluence;

            var num = Mathf.FloorToInt(animalInfluence * def.stages.Count);
            num = Mathf.Clamp(num, 0, def.stages.Count - 1); 
            
            return ThoughtState.ActiveAtStage(num); 
            
        }
    }
}