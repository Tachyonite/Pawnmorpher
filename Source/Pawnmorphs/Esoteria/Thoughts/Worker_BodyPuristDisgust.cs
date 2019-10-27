// Worker_BodyPuristDisgust.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:36 PM
// last updated 09/18/2019  2:36 PM

using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// thought worker for pawns that have the body purist to add opinions about other pawns with mutations 
    /// </summary>
    public class Worker_BodyPuristDisgust : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (!p.RaceProps.Humanlike) return false;
            if (!otherPawn.RaceProps.Humanlike) return false; //make sure only humanlike pawns are affected by this 
            if (!p.story.traits.HasTrait(TraitDefOf.BodyPurist)) return false;
            if (!RelationsUtility.PawnsKnowEachOther(p, otherPawn)) return false; //the pawns have to know each other 

            var tracker = otherPawn.GetMutationTracker();
            if (tracker == null) return false;
            if (tracker.MutationsCount == 0) return false;  
            int n = Mathf.FloorToInt(tracker.TotalNormalizedInfluence * def.stages.Count);
            n = Mathf.Clamp(n, 0, def.stages.Count - 1); 
            
            return ThoughtState.ActiveAtStage(n);
        }
    }
}
