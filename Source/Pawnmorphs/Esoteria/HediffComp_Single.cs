using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// hediff comp to add a single mutation then remove the parent hediff 
    /// </summary>
    /// <seealso cref="Verse.HediffComp" />
    public class HediffComp_Single : HediffComp
    {
        /// <summary>The stacks</summary>
        public int stacks = 1;

        /// <summary>called to expose the data in this comp</summary>
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref stacks, nameof(stacks), 1);
        }

        /// <summary>called after the parent is merged with the other hediff</summary>
        /// <param name="other">The other.</param>
        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);

            var comp = other.TryGetComp<HediffComp_Single>();
            stacks = Mathf.Min(Props.maxStacks, stacks + comp.stacks); 
        }
        /// <summary>Gets the properties.</summary>
        /// <value>The properties.</value>
        public HediffCompProperties_Single Props
        {
            get
            {
                return (HediffCompProperties_Single)props;
            }
        }
    }
}
