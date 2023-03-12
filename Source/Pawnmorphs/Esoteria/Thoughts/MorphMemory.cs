// Worker_HasMutationsMemory.cs modified by Iron Wolf for Pawnmorph on 09/26/2019 8:40 PM
// last updated 09/26/2019  8:40 PM

using RimWorld;
using UnityEngine;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// a memory who's stage is dependent on if the pawn is a given morph
	/// </summary>
	/// <seealso cref="RimWorld.Thought_Memory" />
	public class MorphMemory : Thought_Memory
	{
		/// <summary>Gets the index of the current stage.</summary>
		/// <value>The index of the current stage.</value>
		public override int CurStageIndex
		{
			get
			{
				var tracker = pawn.GetMutationTracker();
				if (tracker == null) return 0;
				float animalInfluence = tracker.TotalNormalizedInfluence;

				int n = Mathf.FloorToInt(animalInfluence * def.stages.Count); //evenly split up the stages between humanInfluence of [0,1] 
				n = Mathf.Clamp(n, 0, def.stages.Count - 1);
				return n;

			}
		}
	}
}