// EtherThought.cs modified by Iron Wolf for Pawnmorph on 07/28/2019 3:34 PM
// last updated 07/28/2019  3:34 PM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// a memory thought that depends in some way on the etherstate of it's associated pawn 
    /// </summary>
    public class Thought_EtherMemory : Thought_Memory
    {
        public override int CurStageIndex => Mathf.Min(def.stages.Count - 1, (int) pawn.GetEtherState());
        
    }

   
}