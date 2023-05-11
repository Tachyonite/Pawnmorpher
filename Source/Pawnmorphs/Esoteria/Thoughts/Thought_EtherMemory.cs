// EtherThought.cs modified by Iron Wolf for Pawnmorph on 07/28/2019 3:34 PM
// last updated 07/28/2019  3:34 PM

using JetBrains.Annotations;
using RimWorld;
using UnityEngine;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// a memory thought that depends in some way on the etherstate of it's associated pawn 
	/// </summary>
	[UsedImplicitly]
	public class Thought_EtherMemory : Thought_Memory
	{
		/// <summary>Gets the index of the current stage.</summary>
		/// <value>The index of the current stage.</value>
		public override int CurStageIndex => Mathf.Min(def.stages.Count - 1, (int)pawn.GetEtherState());

	}


}