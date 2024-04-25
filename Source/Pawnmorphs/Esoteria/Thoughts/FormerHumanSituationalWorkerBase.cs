// FormerHumanSituationalWorkerBase.cs modified by Iron Wolf for Pawnmorph on 01/21/2020 8:46 PM
// last updated 01/21/2020  8:46 PM

using RimWorld;
using UnityEngine;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// abc for former human situational thoughts 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public abstract class FormerHumanSituationalWorkerBase : ThoughtWorker
	{
		/// <summary>
		/// Gets the index of the stage for the given sapience level 
		/// </summary>
		/// <param name="sapienceLevel">The sapience level.</param>
		/// <returns></returns>
		protected int GetStageIndex(SapienceLevel sapienceLevel)
		{
			return Mathf.Min(def.stages.Count - 1, (int)sapienceLevel);
		}
	}
}