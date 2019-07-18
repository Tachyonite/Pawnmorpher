using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class HediffGiver_EsotericInstant : HediffGiver
    {
        public float mtbDays;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (Rand.RangeInclusive(0, 5) == 1 && base.TryApply(pawn, null))
                {
                    if (cause.def.HasComp(typeof(HediffComp_Single)))
                    {
                        pawn.health.RemoveHediff(cause);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
