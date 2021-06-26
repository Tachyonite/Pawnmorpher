// Giver_DeliverSpecialThingsToChambers.cs created by Iron Wolf for Pawnmorph on 06/25/2021 6:20 PM
// last updated 06/25/2021  6:20 PM

using JetBrains.Annotations;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Work
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="RimWorld.WorkGiver_Scanner" />
    public class Giver_DeliverSpecialThingsToChambers : WorkGiver_Scanner
    {
        /// <summary>
        /// Gets the potential work thing request.
        /// </summary>
        /// <value>
        /// The potential work thing request.
        /// </value>
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(PMThingDefOf.MutagenicChamber);
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.def != PMThingDefOf.MutagenicChamber) return false;
            if ((t is MutaChamber chamber))
            {
                if (!chamber.WaitingOnSpecialThing || chamber.SpecialThingNeeded == null) return false;
                return FindBestThingFor(chamber.SpecialThingNeeded, pawn) != null; 
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Jobs the on thing.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="t">The t.</param>
        /// <param name="forced">if set to <c>true</c> [forced].</param>
        /// <returns></returns>
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.def != PMThingDefOf.MutagenicChamber) return null;
            if ((t is MutaChamber chamber))
            {
                if (!chamber.WaitingOnSpecialThing || chamber.SpecialThingNeeded == null) return null;
                var thing = FindBestThingFor(chamber.SpecialThingNeeded, pawn);
                if (thing == null) return null;
                var job = JobMaker.MakeJob(PMJobDefOf.PM_CarrySpecialToMutagenChamber, thing, chamber);
                return job; 
            }

            return null; 
        }

        [CanBeNull]
        Thing FindBestThingFor([NotNull] ThingDef target, Pawn p)
        {
            var req = ThingRequest.ForDef(target);
            return GenClosest.ClosestThingReachable(p.Position, p.Map, req, PathEndMode.ClosestTouch, TraverseParms.For(p),
                                             validator: t => Validator(t, p, target)); 
        }


        bool Validator(Thing t, Pawn p, [NotNull] ThingDef targetDef)
        {
            return t?.def == targetDef && !t.IsForbidden(p) && p.CanReserveAndReach(t, PathEndMode.Touch, Danger.Unspecified); 
        }
    }
}