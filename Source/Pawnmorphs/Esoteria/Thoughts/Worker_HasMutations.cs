// Worker_HasMutations.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:14 PM
// last updated 09/18/2019  2:14 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
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
        /// <summary>
        /// return the thought state for the given pawn 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!def.IsValidFor(p)) 
                return ThoughtState.Inactive;

            MutationTracker mutTracker = p.GetMutationTracker();
            if (mutTracker == null) 
                return ThoughtState.Inactive;
            

            if (!mutTracker.AllMutations.Any()) 
                return ThoughtState.Inactive;
            
            var nInfluence = mutTracker.TotalNormalizedInfluence;
            
            RaceMutationSettingsExtension racialMutations = p.def.TryGetRaceMutationSettings();
            if (racialMutations != null)
            {
                foreach (var racialMutationGiver in racialMutations.mutationRetrievers.OfType<Hediffs.MutationRetrievers.AnimalClassRetriever>())
                    nInfluence -= mutTracker.GetDirectNormalizedInfluence(racialMutationGiver.animalClass);
                
            }

            var idx = Mathf.FloorToInt(Mathf.Clamp(nInfluence * def.stages.Count, 0, def.stages.Count - 1));

            if (idx > 0)
                return ThoughtState.ActiveAtStage(idx);

            return ThoughtState.Inactive;
            
        }
    }
}