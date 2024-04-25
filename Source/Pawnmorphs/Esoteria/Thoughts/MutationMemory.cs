// MutationMemory.cs created by Iron Wolf for Pawnmorph on 09/16/2019 2:47 PM
// last updated 09/16/2019  2:47 PM

using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// memory who's stage depends on the pawn's current mutation outlook 
	/// </summary>
	public class MutationMemory : Thought_Memory
	{
		/// <summary>Gets the index of the current stage.</summary>
		/// <value>The index of the current stage.</value>
		public override int CurStageIndex
		{
			get
			{
				try
				{



					int maxStage = def.stages.Count - 1;

					MutationOutlook mutationOutlook;

					if (pawn != null)
					{
						mutationOutlook = pawn.GetMutationOutlook();
					}
					else
					{
						mutationOutlook = MutationOutlook.Neutral;
						Log.Warning($"{def?.defName ?? "NULL"} encountered a null pawn while getting the current stage");
					}


					if (mutationOutlook == MutationOutlook.PrimalWish && maxStage < (int)MutationOutlook.PrimalWish)
						mutationOutlook = MutationOutlook.Furry; //use the furry stage if the primal wish stage isn't there 
					if (mutationOutlook == MutationOutlook.Transhumanist && maxStage < (int)MutationOutlook.Transhumanist)
					{
						mutationOutlook = MutationOutlook.Neutral;
					}
					return Mathf.Min(maxStage, (int)mutationOutlook);
				}
				catch (ArgumentNullException argEx)
				{
					throw new
						ArgumentException($"{def?.defName ?? "NULL THOUGHT"} encountered exception while getting the current stag index",
										  argEx);
				}
			}
		}
	}
}