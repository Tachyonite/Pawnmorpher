// JobGiver_HuntingBreak.cs modified by Iron Wolf for Pawnmorph on 12/15/2019 5:19 PM
// last updated 12/15/2019  5:19 PM

using Pawnmorph.Mental;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Verse.AI.ThinkNode_JobGiver" />
	public class Giver_HuntingBreak : ThinkNode_JobGiver
	{
		/// <summary>
		/// Tries the give a job to the pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!(pawn.MentalState is State_Hunting huntingState)) return null;

			if (huntingState.Prey == null || huntingState.Prey.Dead) return null;

			if (pawn.jobs?.curJob?.def == JobDefOf.PredatorHunt) return null;

			var job = new Job(JobDefOf.PredatorHunt, huntingState.Prey)
			{
				killIncappedTarget = true,
			};
			return job;
		}
	}
}