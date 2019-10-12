using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    public class Giver_MilkSelf : ThinkNode_JobGiver
    {
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
