// MutationMemory.cs created by Iron Wolf for Pawnmorph on 09/16/2019 2:47 PM
// last updated 09/16/2019  2:47 PM

using RimWorld;
using UnityEngine;

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
                int maxStage = def.stages.Count - 1;

                MutationOutlook mutationOutlook = pawn.GetMutationOutlook();

                if (mutationOutlook == MutationOutlook.PrimalWish && maxStage < (int) MutationOutlook.PrimalWish)
                    mutationOutlook = MutationOutlook.Furry; //use the furry stage if the primal wish stage isn't there 

                return Mathf.Min(maxStage, (int) mutationOutlook);
            }
        }
    }
}