// Giver_RecruitSapientAnimal.cs created by Iron Wolf for Pawnmorph on 03/15/2020 3:41 PM
// last updated 03/15/2020  3:41 PM

using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Work
{
	/// <summary>
	/// work giver for recruiting a sapient animal
	/// </summary>
	/// <seealso cref="RimWorld.WorkGiver_InteractAnimal" />
	public class Giver_RecruitSapientAnimal : WorkGiver_InteractAnimal
	{
		private static string CantInteractAnimalDownedTrans;
		private static string CantInteractAnimalAsleepTrans;
		private static string CantInteractAnimalBusyTrans;

		/// <summary>
		/// Resets the static data.
		/// </summary>
		public new static void ResetStaticData()
		{

			CantInteractAnimalDownedTrans = (string)"CantInteractAnimalDowned".Translate(); //TODO make these sapient animal specific 
			CantInteractAnimalAsleepTrans = (string)"CantInteractAnimalAsleep".Translate();
			CantInteractAnimalBusyTrans = (string)"CantInteractAnimalBusy".Translate();
		}

		/// <summary>
		/// Potentials the work things global.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(PMDesignationDefOf.RecruitSapientFormerHuman))
			{
				yield return item.target.Thing;
			}
		}

		/// <summary>
		/// determines if this work giver should be skipped 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns></returns>
		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(PMDesignationDefOf.RecruitSapientFormerHuman);
		}

		/// <summary>
		/// returns the job for the given pawn on the given thing 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="t">The t.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns></returns>
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || !(pawn2.Faction == null || !pawn2.Faction.def.humanlikeFaction))
			{
				return null;
			}
			if (pawn.Map.designationManager.DesignationOn(t, PMDesignationDefOf.RecruitSapientFormerHuman) == null)
			{
				return null;
			}
			if (TameUtility.TriedToTameTooRecently(pawn2))
			{
				JobFailReason.Is(WorkGiver_InteractAnimal.AnimalInteractedTooRecentlyTrans);
				return null;
			}
			if (!CanInteractWithAnimal(pawn, pawn2, forced))
			{
				return null;
			}

			return JobMaker.MakeJob(PMJobDefOf.RecruitSapientFormerHuman, t);
		}

		/// <summary>
		/// Determines whether this instance with the specified pawn [can interact with animal] 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="animal">The animal.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns>
		///   <c>true</c> if this instance with the specified pawn  [can interact with animal]  otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanInteractWithAnimal(Pawn pawn, Pawn animal, bool forced)
		{
			if (!pawn.CanReserve((LocalTargetInfo)((Thing)animal), 1, -1, (ReservationLayerDef)null, forced))
				return false;
			if (animal.Downed)
			{
				JobFailReason.Is(CantInteractAnimalDownedTrans, (string)null);
				return false;
			}
			if (!animal.Awake())
			{
				JobFailReason.Is(CantInteractAnimalAsleepTrans, (string)null);
				return false;
			}
			if (!animal.CanCasuallyInteractNow(false))
			{
				JobFailReason.Is(CantInteractAnimalBusyTrans, (string)null);
				return false;
			}

			return true;
		}
	}
}