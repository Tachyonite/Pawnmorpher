using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    class Giver_MegaShaveSelf : ThinkNode_JobGiver
    {
        [CanBeNull]
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(MutationsDefOf.EtherMegawoolly)?.TryGetComp<HediffComp_Production>()
                == null) return null;

            var pos = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 2.5f, null, Danger.Some);
            var job = new Job(PMJobDefOf.PMMegaShaveSelf, pos);
            return job;
        }
    }
}
