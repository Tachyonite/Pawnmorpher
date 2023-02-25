// Giver_WorkAtSequencer.cs created by Iron Wolf for Pawnmorph on 11/14/2020 8:54 AM
// last updated 11/14/2020  8:54 AM

using Pawnmorph.ThingComps;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Work
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="RimWorld.WorkGiver_Scanner" />
	public class Giver_WorkAtSequencer : WorkGiver_OperateScanner
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
			return JobMaker.MakeJob(PMJobDefOf.PM_OperateSequencer, (LocalTargetInfo)t, 1500, true);
		}

		/// <summary>
		/// determines if he pawn should skip this giver.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns></returns>
		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			var list = pawn.Map.listerThings.ThingsOfDef(ScannerDef);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Faction == pawn.Faction)
				{
					var compScanner = list[i].TryGetComp<MutationSequencerComp>();
					if (compScanner != null && compScanner.CanUseNow)
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Determines whether the given pawn has a job on the thing.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="t">The t.</param>
		/// <param name="forced">if set to <c>true</c> [forced].</param>
		/// <returns>
		///   <c>true</c> if the given pawn has a job on the thing; otherwise, <c>false</c>.
		/// </returns>
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!BaseHasJobOnThing(pawn, t, forced))
				return false;

			var scannerComp = t.TryGetComp<MutationSequencerComp>();
			if (scannerComp == null)
			{
				Log.ErrorOnce($"unable to find sequencer comp on {t.ThingID}", t.thingIDNumber);
				return false;
			}

			return scannerComp.CanUseNow;
		}

		bool BaseHasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			Building building = t as Building;
			if (building == null)
			{
				return false;
			}
			if (building.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReserve(building, 1, -1, null, forced))
			{
				return false;
			}

			if (building.IsBurning())
			{
				return false;
			}
			return true;
		}
	}
}