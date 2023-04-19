// Worker_Sapience.cs created by Iron Wolf for Pawnmorph on 04/25/2020 4:32 PM
// last updated 04/25/2020  4:32 PM

using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought worker for giving pawns thoughts if they are in a sapience state 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_Sapience : ThoughtWorker
	{
		/// <summary>
		/// Currents the state internal.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			var sapienceLevel = p.GetQuantizedSapienceLevel();
			if (sapienceLevel == null) return false;
			if (p.needs?.mood == null) return false;
			if (!def.IsValidFor(p)) return false;
			var idx = Mathf.Clamp((int)sapienceLevel.Value, 0, def.stages.Count - 1);
			return ThoughtState.ActiveAtStage(idx);
		}
	}
}