using Verse;

//Depricated, use HedifGiver_Mutaions instead.
namespace Pawnmorph
{
    public class HediffGiver_Esoteric : HediffGiver
    {
        public float mtbDays;
        public bool once = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (Rand.MTBEventOccurs(mtbDays, 60000f, 60f) && TryApply(pawn))
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                if (cause.def.HasComp(typeof(HediffComp_Single)))
                {
                    pawn.health.RemoveHediff(cause);
                }
            }
        }
    }
}
