// Worker_FurryAppreciation.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:07 PM
// last updated 09/18/2019  2:07 PM

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
		/// <summary>gets the current thought state for the given pawns</summary>
		/// <param name="p">The pawn that is having the thought</param>
		/// <param name="otherPawn">The pawn the thought is about</param>
		/// <returns></returns>
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
		{
			if (!p.RaceProps.Humanlike) return false;
			if (!otherPawn.RaceProps.Humanlike) return false; //make sure only humanlike pawns are affected by this 
			if (!p.story.traits.HasTrait(PMTraitDefOf.MutationAffinity)) return false;
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
