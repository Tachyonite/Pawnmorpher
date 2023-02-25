using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	class Giver_DrainChemcyst : Giver_Producer
	{
		[CanBeNull]
		protected override Job TryGiveJob(Pawn pawn)
		{
			var pos = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 2.5f, null, Danger.Some);
			var job = new Job(PMJobDefOf.PMDrainChemcyst, pos);
			return job;
		}
	}
}
