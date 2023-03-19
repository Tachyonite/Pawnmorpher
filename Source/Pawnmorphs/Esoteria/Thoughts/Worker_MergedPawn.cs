// Worker_MergedPawn.cs created by Iron Wolf for Pawnmorph on 05/08/2020 11:37 AM
// last updated 05/08/2020  11:37 AM

using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought worker for merged pawns 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_MergedPawn : ThoughtWorker
	{
		/// <summary>
		/// gets the current state of this thought for the given pawn
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			var sTracker = p?.GetSapienceTracker();
			if (sTracker == null) return false;
			if (sTracker.CurrentState?.StateDef != SapienceStateDefOf.MergedPawn) return false;
			if (!def.IsValidFor(p)) return false;

			var idx = (int)sTracker.SapienceLevel;
			return ThoughtState.ActiveAtStage(Mathf.Min(def.stages.Count - 1, idx));

		}
	}
}