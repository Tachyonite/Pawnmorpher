using System.Collections.Generic;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    /// <summary> Base class for productive mutation's job driver. </summary>
    public abstract class Driver_ProduceThing : JobDriver
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
            yield return Toils_General.Do(Produce);
        }

        public abstract void Produce();
    }
}
