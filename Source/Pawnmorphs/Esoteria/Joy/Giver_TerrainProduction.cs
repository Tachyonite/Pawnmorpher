// Giver_FindMushrooms.cs modified by Iron Wolf for Pawnmorph on 11/09/2019 8:13 AM
// last updated 11/09/2019  8:13 AM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Joy
{
	/// <summary>
	/// joy giver for terrain production jobs 
	/// </summary>
	/// <seealso cref="RimWorld.JoyGiver" />
	public class Giver_TerrainProduction : JoyGiver
	{
		/// <summary>
		/// Tries to give a job to the given pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public override Job TryGiveJob(Pawn pawn)
		{
			if (!JoyUtility.EnjoyableOutsideNow(pawn, null))
			{
				return null;
			}
			if (PawnUtility.WillSoonHaveBasicNeed(pawn))
			{
				return null;
			}


			if (!def.IsValidFor(pawn)) return null;


			var allProductionComps = pawn.health.hediffSet.hediffs.Select(h => h.TryGetComp<Comp_TerrainProduction>()?.Props)
										 .Where(p => p != null)
										 .ToList();

			if (allProductionComps.Count == 0) return null;

			bool IsValidCell(IntVec3 cell)
			{
				if (PawnUtility.KnownDangerAt(cell, pawn.Map, pawn)) return false;
				var terrain = cell.GetTerrain(pawn.Map);
				if (terrain == null) return false;
				if (!allProductionComps.Any(p => p.CanProduceOn(terrain))) return false;
				return cell.Standable(pawn.Map);
			}


			bool IsValidRegion(Region region)
			{
				if (region.IsForbiddenEntirely(pawn)) return false;
				return region.TryFindRandomCellInRegionUnforbidden(pawn, IsValidCell, out IntVec3 _);
			}

			if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(RegionType.Set_Passable),
													 TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn), IsValidRegion,
													 100, out Region reg, RegionType.Set_Passable))
				return null;

			if (!reg.TryFindRandomCellInRegionUnforbidden(pawn, IsValidCell, out IntVec3 root))
				return null;

			if (!WalkPathFinder.TryFindWalkPath(pawn, root, out var result))
			{
				return null;
			}

			Job job = new Job(def.jobDef, result[0]) { targetQueueA = new List<LocalTargetInfo>() };
			for (int i = 1; i < result.Count; i++)
			{
				job.targetQueueA.Add(result[i]);
			}

			job.locomotionUrgency = LocomotionUrgency.Walk;
			return job;
		}


	}
}