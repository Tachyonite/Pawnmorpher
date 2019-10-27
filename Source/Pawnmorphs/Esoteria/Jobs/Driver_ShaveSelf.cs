using Verse;

namespace Pawnmorph.Jobs
{
    class Driver_ShaveSelf : Driver_ProduceThing
    {
        public override void Produce()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MutationsDefOf.EtherWooly);
            var comp = hediff?.TryGetComp<HediffComp_Production>();

            comp?.Produce();
        }
    }
}
