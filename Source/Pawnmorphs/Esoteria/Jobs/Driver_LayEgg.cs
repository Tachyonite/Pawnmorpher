// Driver_LayEgg.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 8:34 AM
// last updated 09/22/2019  8:34 AM

using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    /// <summary>
    /// job driver to make humanoid pawns lay eggs using HediffComp_Production 
    /// </summary>
    public class Driver_LayEgg : JobDriver
    {
        private const int WAIT_TIME = 250;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; 
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_General.Wait(WAIT_TIME);
            yield return Toils_General.Do(ProduceEgg); 
        }

        void ProduceEgg()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MutationsDefOf.EtherEggLayer);
            var comp = hediff?.TryGetComp<HediffComp_Production>();

            comp?.Produce();
        }
    }
}