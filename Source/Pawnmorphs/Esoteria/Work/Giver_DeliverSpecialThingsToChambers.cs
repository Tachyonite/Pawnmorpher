// Giver_DeliverSpecialThingsToChambers.cs created by Iron Wolf for Pawnmorph on 06/25/2021 6:20 PM
// last updated 06/25/2021  6:20 PM

using System.Linq;
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
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(PMThingDefOf.PM_NewMutagenicChamber);
		/// <summary>
		/// Gets the path end mode.
		/// </summary>
		/// <value>
		/// The path end mode.
		/// </value>
		public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

		/// <summary>
		/// Determines whether [has job on thing] [the specified pawn].
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="t">The t.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns>
		///   <c>true</c> if [has job on thing] [the specified pawn]; otherwise, <c>false</c>.
		/// </returns>
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.def != PMThingDefOf.PM_NewMutagenicChamber) return CheckForChamberNeedingItem(pawn, t, forced);
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

		private bool CheckForChamberNeedingItem(Pawn pawn, Thing item, bool forced)
		{
			var chambers = item.Map.listerThings.ThingsOfDef(PMThingDefOf.PM_NewMutagenicChamber).OfType<MutaChamber>();
			foreach (MutaChamber chamber in chambers)
			{

				if (!chamber.WaitingOnSpecialThing || chamber.SpecialThingNeeded != item.def) continue;
				Log.Message($"found chamber needing {chamber.SpecialThingNeeded.defName}");
				if (!pawn.CanReserve(item)) continue;
				return true;
			}

			return false;
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
			if (t.def != PMThingDefOf.PM_NewMutagenicChamber)
			{
				return CheckForJobOnPotentiallySpecialItem(pawn, t, forced);
			}
			if ((t is MutaChamber chamber))
			{
				if (!chamber.WaitingOnSpecialThing || chamber.SpecialThingNeeded == null) return null;
				var thing = FindBestThingFor(chamber.SpecialThingNeeded, pawn);
				if (thing == null) return null;
				var job = JobMaker.MakeJob(PMJobDefOf.PM_CarrySpecialToMutagenChamber, thing, chamber);
				if (job?.count != null)
					job.count = 1;
				return job;
			}

			return null;
		}

		private Job CheckForJobOnPotentiallySpecialItem(Pawn pawn, Thing item, bool forced)
		{
			var chambers = item.Map.listerThings.ThingsOfDef(PMThingDefOf.PM_NewMutagenicChamber).OfType<MutaChamber>();
			foreach (MutaChamber chamber in chambers)
			{
				if (!chamber.WaitingOnSpecialThing || chamber.SpecialThingNeeded != item.def) continue;
				if (!pawn.CanReserve(item)) continue;
				var job = JobMaker.MakeJob(PMJobDefOf.PM_CarrySpecialToMutagenChamber, item, chamber);
				if (job != null)
					job.count = 1;
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