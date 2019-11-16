using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    /// <summary>
    /// job giver for pawns milking themselves 
    /// </summary>
    public class Giver_MilkSelf : ThinkNode_JobGiver
    {
        /// <summary>
        /// attempt to generate a job for the given pawn 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        [CanBeNull]
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(MutationsDefOf.EtherUdder)?.TryGetComp<HediffComp_Production>()
                == null) return null;

            var pos = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 2.5f, null, Danger.Some);
            var job = new Job(PMJobDefOf.PMMilkSelf, pos);
            return job;
        }
    }
}
