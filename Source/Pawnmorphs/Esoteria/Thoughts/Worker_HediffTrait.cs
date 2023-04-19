// Worker_HediffTrait.cs created by Iron Wolf for Pawnmorph on 09/17/2019 8:06 AM
// last updated 09/17/2019  8:06 AM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary> Thought worker that works like ThoughtWorker_Hediff except is also respects traits. </summary>
	public class Worker_HediffTrait : ThoughtWorker
	{
		/// <summary>Gets the current thought state of the given pawn</summary>
		/// <param name="p">The pawn.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			var firstHediff = p.health?.hediffSet?.GetFirstHediffOfDef(def.hediff);
			if (firstHediff?.def.stages == null) return ThoughtState.Inactive; //the target hediff must have stages 

			if (!CheckTraits(p)) return ThoughtState.Inactive;

			var hStageIndex = firstHediff.CurStageIndex;
			var index = Mathf.Min(def.stages.Count - 1, hStageIndex);

			return ThoughtState.ActiveAtStage(index);
		}

		/// <summary> Check to make sure that the pawn's traits allow for the thought to be active. </summary>
		/// <returns> If traits allow the thought can be active. </returns>
		private bool CheckTraits(Pawn pawn)
		{
			var storyTraits = pawn.story?.traits;
			if (def.nullifyingTraits != null)
			{
				foreach (var nullifyingTrait in def.nullifyingTraits)
				{
					if (storyTraits?.HasTrait(nullifyingTrait) ?? false) return false;
				}
			}

			if (def.requiredTraits != null)
			{
				if (storyTraits == null) return false;
				foreach (var requiredTrait in def.requiredTraits)
				{
					if (!storyTraits.HasTrait(requiredTrait)) return false;
				}
			}

			return true;
		}
	}
}
