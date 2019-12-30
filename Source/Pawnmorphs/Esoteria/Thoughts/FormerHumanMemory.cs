// FormerHumanMemory.cs created by Iron Wolf for Pawnmorph on 12/12/2019 11:28 AM
// last updated 12/12/2019  11:28 AM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// memory who's stage depends on the former human status of the pawn 
    /// </summary>
    /// <seealso cref="RimWorld.Thought_Memory" />
    public class FormerHumanMemory : Thought_Memory
    {
        /// <summary>Gets the index of the current stage.</summary>
        /// <value>The index of the current stage.</value>
        public override int CurStageIndex
        {
            get
            {
                var fSapienceStatus = pawn.GetQuantizedSapienceLevel();
                if (fSapienceStatus == null)
                {
                    Log.Warning($"Pawn {pawn.Name} is not a former human but has {def.defName}?");
                    return 0; 
                }

                return Mathf.Min(def.stages.Count - 1, (int) fSapienceStatus.Value);
            }
        }
    }
}