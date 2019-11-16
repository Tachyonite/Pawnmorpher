using Verse;

namespace Pawnmorph.Jobs
{
    /// <summary> Job driver to make humanoid pawns milk themselves using HediffComp_Production. </summary>
    public class Driver_MilkSelf : Driver_ProduceThing
    {
        /// <summary>
        /// Produce whatever resources this driver is producing.
        /// </summary>
        public override void Produce()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MutationsDefOf.EtherUdder);
            var comp = hediff?.TryGetComp<HediffComp_Production>();

            comp?.Produce();
        }
    }
}
