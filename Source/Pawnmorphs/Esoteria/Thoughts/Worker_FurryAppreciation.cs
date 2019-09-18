// Worker_FurryAppreciation.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:07 PM
// last updated 09/18/2019  2:07 PM

using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// thought worker for the furry mutation appreciation thought
    /// </summary>
    public class Worker_FurryAppreciation : ThoughtWorker 
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (!p.RaceProps.Humanlike) return false;
            if (!otherPawn.RaceProps.Humanlike) return false; //make sure only humanlike pawns are affected by this 
            if (!p.story.traits.HasTrait(PMTraitDefOf.FurryTrait)) return false;
            if (!RelationsUtility.PawnsKnowEachOther(p, otherPawn)) return false; //the pawns have to know each other 
            var numMutation = otherPawn.health.hediffSet.hediffs.Count(h => h is Hediff_AddedMutation); //get the number of mutations on the other pawn 
            if (numMutation == 0) return false;
            return ThoughtState.ActiveAtStage(Mathf.Min(numMutation, def.stages.Count - 1));
        }
    }
}