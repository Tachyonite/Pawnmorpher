// DBG_TrainingWorkGiver.cs created by Iron Wolf for Pawnmorph on 05/10/2020 8:41 AM
// last updated 05/10/2020  8:41 AM

using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.DebugUtils
{
	/// <summary>
	/// debug class for the training work giver 
	/// </summary>
	/// <seealso cref="RimWorld.WorkGiver_Train" />
	public class DBG_TrainingWorkGiver : WorkGiver_Train
	{
		/// <summary>
		/// Jobs the on thing.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="t">The t.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns></returns>
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			var dbgLog = pawn.jobs?.debugLog == true;

			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || !(pawn2.RaceProps.Animal || pawn2.GetIntelligence() == Intelligence.Animal))
			{
				if (dbgLog) Log.Message($"{pawn2?.Name?.ToStringFull ?? "NULL"} is null or not an animals or animalistic");
				return null;
			}
			if (pawn2.Faction != pawn.Faction)
			{
				if (dbgLog)
				{
					Log.Message($"{pawn2.Name} is not part of the same faction as {pawn.Name}");
				}
				return null;
			}
			if (TrainableUtility.TrainedTooRecently(pawn2))
			{
				if (dbgLog) Log.Message($"{pawn2.Name} was trained to recently");
				JobFailReason.Is(WorkGiver_InteractAnimal.AnimalInteractedTooRecentlyTrans);
				return null;
			}
			if (pawn2.training == null)
			{
				if (dbgLog) Log.Message($"{pawn2.Name} has no training message");

				return null;
			}
			if (pawn2.training.NextTrainableToTrain() == null)
			{
				if (dbgLog) Log.Message($"{pawn2.Name} has no trainability to train");

				return null;
			}
			if (!CanInteractWithAnimal(pawn, pawn2, forced))
			{

				if (dbgLog) Log.Message($"{pawn2.Name} cannot interact with {pawn.Name}");
				return null;
			}
			if (pawn2.RaceProps.EatsFood && !HasFoodToInteractAnimal(pawn, pawn2))
			{
				Job job = TakeFoodForAnimalInteractJob(pawn, pawn2);
				if (job == null)
				{
					if (dbgLog) Log.Message($"{pawn.Name} cannot get food for {pawn2.Name}");

					JobFailReason.Is(WorkGiver_InteractAnimal.NoUsableFoodTrans);
				}
				return job;
			}
			return JobMaker.MakeJob(JobDefOf.Train, t);
		}
	}
}