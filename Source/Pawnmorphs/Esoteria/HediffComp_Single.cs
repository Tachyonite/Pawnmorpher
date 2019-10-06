using UnityEngine;
using Verse;

namespace Pawnmorph
{
    public class HediffComp_Single : HediffComp
    {
        public int stacks = 1;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref stacks, nameof(stacks), 1);
        }

        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);

            var comp = other.TryGetComp<HediffComp_Single>();
            stacks = Mathf.Min(Props.maxStacks, stacks + comp.stacks); 
        }

        public HediffCompProperties_Single Props
        {
            get
            {
                return (HediffCompProperties_Single)props;
            }
        }
    }
}
