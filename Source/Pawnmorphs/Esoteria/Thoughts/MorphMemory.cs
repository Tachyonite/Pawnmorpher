// Worker_HasMutationsMemory.cs modified by Iron Wolf for Pawnmorph on 09/26/2019 8:40 PM
// last updated 09/26/2019  8:40 PM

using RimWorld;
using UnityEngine;

namespace Pawnmorph.Thoughts
{
    public class MorphMemory : Thought_Memory
    {
        public override int CurStageIndex
        {
            get
            {
                var tracker = pawn.GetMutationTracker();
                if (tracker == null) return 0;
                float humanInfluence = 1 - tracker.TotalNormalizedInfluence;

                int n = Mathf.FloorToInt(humanInfluence * def.stages.Count); //evenly split up the stages between humanInfluence of [0,1] 
                n = Mathf.Clamp(n, 0, def.stages.Count - 1);
                return n; 

            }
        }

        

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void ThoughtInterval()
        {
            base.ThoughtInterval();
        }
    }
}