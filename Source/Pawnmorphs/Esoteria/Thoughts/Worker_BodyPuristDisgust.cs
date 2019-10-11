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
            var numMutation = otherPawn.health.hediffSet.hediffs.Count(h => h is Hediff_AddedMutation); //get the number of mutations on the other pawn 
            if (numMutation == 0) return false;
            return ThoughtState.ActiveAtStage(Mathf.Min(numMutation, def.stages.Count - 1));
        }
    }
}
