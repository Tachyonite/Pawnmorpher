// ThoughtWorker_EtherHediff.cs modified by Iron Wolf for Pawnmorph on 07/29/2019 7:22 AM
// last updated 07/29/2019  7:22 AM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought worker for a thought that is active when a certain hediff is present, and who's stage depends on the ether state of the pawn 
	/// </summary>
	public class ThoughtWorker_EtherHediff : ThoughtWorker
	{
		/// <summary>Gets the current thought state of the given pawn.</summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			var hediff = p.health.hediffSet.GetFirstHediffOfDef(def.hediff);

			if (hediff == null) //ignoring the hediff's stage, the thought's stage is dependent on only the ether state of the pawn c
			{
				return false;
			}

			var stageIndex = Mathf.Min(def.stages.Count - 1, (int)p.GetEtherState());

			return ThoughtState.ActiveAtStage(stageIndex);
		}
	}
}