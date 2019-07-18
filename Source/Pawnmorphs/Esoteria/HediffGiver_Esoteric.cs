using Verse;

namespace Pawnmorph
{
    public class HediffGiver_Esoteric : HediffGiver
    {
        public float mtbDays;
        public bool once = false;
        private bool triggered = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && base.TryApply(pawn) && ((!triggered && once) || !once))
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                        if (once)
                        {
                            triggered = true;
                        }
                        if (cause.def.HasComp(typeof(HediffComp_Single)))
                        {

                            pawn.health.RemoveHediff(cause);

                        }
                    }
                }
            }
            catch { }
        }
    }
}
