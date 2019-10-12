using Verse;

namespace Pawnmorph.Jobs
{
    class Driver_MegaShaveSelf : Driver_ProduceThing
    {
        public override void Produce()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MutationsDefOf.EtherMegawoolly);
            var comp = hediff?.TryGetComp<HediffComp_Production>();

            comp?.Produce();
        }
    }
}
