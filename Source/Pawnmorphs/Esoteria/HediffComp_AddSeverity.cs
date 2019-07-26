using Verse;

namespace Pawnmorph
{
    public class HediffComp_AddSeverity : HediffComp
    {
        public HediffCompProperties_AddSeverity Props
        {
            get
            {
                return (HediffCompProperties_AddSeverity)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            AddSeverity();
        }

        public void AddSeverity()
        {

            if (Rand.MTBEventOccurs(Props.mtbDays, 60000f, 60f) && !triggered && Pawn.health.hediffSet.HasHediff(Props.hediff))
            {
                HealthUtility.AdjustSeverity(Pawn, Props.hediff, Props.severity);
                triggered = true;
            }
        }

        private bool triggered = false;
    }
}
