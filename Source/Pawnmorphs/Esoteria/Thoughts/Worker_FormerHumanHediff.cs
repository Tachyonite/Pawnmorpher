// Worker_FormerHumanHediff.cs modified by Iron Wolf for Pawnmorph on 12/21/2019 7:58 PM
// last updated 12/21/2019  7:58 PM

using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought worker for activating a thought for former humans when they have a specific hediff
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_FormerHumanHediff : ThoughtWorker
	{
		/// <summary>
		/// Gets the current thought state 
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!def.IsValidFor(p)) return false;
			if (def.hediff == null)
			{
				Log.Error($"{def.defName} has worker {nameof(Worker_FormerHumanHediff)} but no hediff is set!");
				return false;
			}
			var hasHediff = p.health.hediffSet.GetFirstHediffOfDef(def.hediff) != null;
			if (!hasHediff) return false;

			var qL = p.GetQuantizedSapienceLevel() ?? SapienceLevel.PermanentlyFeral;
			var stageIndex = Mathf.Min(def.stages.Count - 1, (int)qL);
			return ThoughtState.ActiveAtStage(stageIndex);
		}
	}
}