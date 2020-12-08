// Giver_WorkAtSequencer.cs created by Iron Wolf for Pawnmorph on 11/14/2020 8:54 AM
// last updated 11/14/2020  8:54 AM

using Pawnmorph.ThingComps;
using RimWorld;
using Verse;

namespace Pawnmorph.Work
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="RimWorld.WorkGiver_Scanner" />
    public class Giver_WorkAtSequencer : WorkGiver_OperateScanner
    {
        /// <summary>
        /// Determines whether the given pawn has a job on the thing.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="t">The t.</param>
        /// <param name="forced">if set to <c>true</c> [forced].</param>
        /// <returns>
        ///   <c>true</c> if the given pawn has a job on the thing; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!base.HasJobOnThing(pawn, t, forced))
                return false;

            var scannerComp = t.TryGetComp<MutationSequencerComp>();
            if (scannerComp == null)
            {
                Log.ErrorOnce($"unable to find sequencer comp on {t.ThingID}", t.thingIDNumber);
                return false; 
            }

            return scannerComp.CanUseNow;
        }
    }
}