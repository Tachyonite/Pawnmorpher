// SapienceThought.cs created by Iron Wolf for Pawnmorph on 04/22/2020 8:54 PM
// last updated 04/22/2020  8:55 PM

using RimWorld;
using UnityEngine;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// memory who's stage depends on the pawn's sapience 
	/// </summary>
	/// <seealso cref="RimWorld.Thought_Memory" />
	public class SapienceMemory : Thought_Memory
	{
		/// <summary>
		/// Gets the index of the current stage.
		/// </summary>
		/// <value>
		/// The index of the current stage.
		/// </value>
		public override int CurStageIndex
		{
			get
			{
				var sapience = (int)(pawn.GetQuantizedSapienceLevel() ?? SapienceLevel.Sapient);

				var idx = Mathf.Clamp(sapience, 0, def.stages.Count - 1);
				return idx;
			}
		}
	}
}