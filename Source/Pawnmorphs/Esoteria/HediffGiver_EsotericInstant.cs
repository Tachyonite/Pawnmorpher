using Verse;

namespace Pawnmorph
{
    public class HediffGiver_EsotericInstant : HediffGiver
    {
        public float mtbDays;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (Rand.RangeInclusive(0, 5) == 1 && base.TryApply(pawn, null))
            {
                if (cause.def.HasComp(typeof(HediffComp_Single)))
                {
                    pawn.health.RemoveHediff(cause);
                }
            }
        }
    }
}
