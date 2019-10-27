using Verse;

namespace Pawnmorph.Jobs
{
    /// <summary> Job driver to make humanoid pawns lay eggs using HediffComp_Production. </summary>
    public class Driver_LayEgg : Driver_ProduceThing
    {
        public override void Produce()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MutationsDefOf.EtherEggLayer);
            var comp = hediff?.TryGetComp<HediffComp_Production>();

            comp?.Produce();
        }
    }
}