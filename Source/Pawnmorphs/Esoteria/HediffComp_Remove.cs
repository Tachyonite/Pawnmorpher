using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
    public class HediffComp_Remove : HediffComp
    {
        public HediffCompProperties_Remove Props
        {
            get
            {
                return (HediffCompProperties_Remove)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            List<Hediff> hS = new List<Hediff>(Pawn.health.hediffSet.hediffs);

            foreach (Hediff hD in hS)
            {
                if (Props.makeImmuneTo.Contains(hD.def))
                {
                    Pawn.health.RemoveHediff(hD);
                }
            }
        }
    }
}
