using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	/// job giver for pawns milking themselves 
	/// </summary>
	public class Giver_MilkSelf : Giver_Producer
	{
		/// <summary>
		/// attempt to generate a job for the given pawn 
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		[CanBeNull]
		protected override Job TryGiveJob(Pawn pawn)
		{
			var pos = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 2.5f, null, Danger.Some);
			var job = new Job(PMJobDefOf.PMMilkSelf, pos);
			return job;
		}
	}
}
