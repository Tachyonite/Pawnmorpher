// Worker_FHNaked.cs modified by Iron Wolf for Pawnmorph on 12/12/2019 7:27 PM
// last updated 12/12/2019  7:27 PM

using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought worker for former human naked thoughts 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_FHNaked : ThoughtWorker
	{
		/// <summary>
		/// Currents the state internal.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!def.IsValidFor(p)) return false;

			//don't check apparel because rimworld thinks animals are wearing pants 
			SapienceLevel? st = p.GetQuantizedSapienceLevel(); //don't check if they're  
			if (st == null || p.GetSapienceState()?.StateDef != SapienceStateDefOf.FormerHuman) return false;

			if (ModsConfig.IdeologyActive) return false;

			int idx = Mathf.Min(def.stages.Count - 1,
								(int)st.Value); //make sure it's a valid index for the stage array in the def 
			return ThoughtState.ActiveAtStage(idx);
		}
	}
}