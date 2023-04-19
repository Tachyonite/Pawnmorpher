// Giver_LayEgg.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 9:19 AM
// last updated 09/22/2019  9:19 AM

using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary> Job giver for making a human pawn lay eggs. </summary>
	public class Giver_LayEgg : Giver_Producer
	{
		/// <summary>
		/// attempt to create a new job for the given pawn 
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		[CanBeNull]
		protected override Job TryGiveJob(Pawn pawn)
		{
			var pos = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 2.5f, null, Danger.Some);
			var job = new Job(PMJobDefOf.PMLayEgg, pos);
			return job;
		}
	}
}