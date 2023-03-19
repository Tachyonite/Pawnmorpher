using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// the incident worker for chaomorph pass incident 
	/// </summary>
	/// <seealso cref="RimWorld.IncidentWorker" />
	public class IncidentWorker_ChaomorphPasses : IncidentWorker
	{
		/// <summary>
		/// Determines whether this instance can fire now with the given params.
		/// </summary>
		/// <param name="parms">The parms.</param>
		/// <returns>
		///   <c>true</c> if this instance can fire now with the specified parms; otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			return !map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(ThingDefOf.Thrumbo) && TryFindEntryCell(map, out IntVec3 intVec);
		}

		/// <summary>Tries the execute the incident.</summary>
		/// <param name="parms">The parms.</param>
		/// <returns></returns>
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			var map = (Map)parms.target;
			if (!TryFindEntryCell(map, out IntVec3 intVec)) return false;


			PawnKindDef animal = ChaomorphUtilities.GetRandomChaomorphPK(ChaomorphType.Chaomorph);

			if (animal == null)
			{
				Log.Error("unable to find random chaomorph for chaomorph passes event");
				return false;
			}

			float num = StorytellerUtility.DefaultThreatPointsNow(map);
			int num2 = GenMath.RoundRandom(num / animal.combatPower);
			int max = Rand.RangeInclusive(2, 4);
			num2 = Mathf.Clamp(num2, 1, max);
			int num3 = Rand.RangeInclusive(90000, 150000);
			IntVec3 invalid = IntVec3.Invalid;

			if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(intVec, map, 10f, out invalid))
				invalid = IntVec3.Invalid;

			Pawn pawn = null;
			for (var i = 0; i < num2; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10);
				pawn = Utilities.PawnGeneratorUtility.GenerateAnimal(animal);
				GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
				pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num3;
				if (invalid.IsValid) pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(invalid, map, 10);
			}

			Find.LetterStack.ReceiveLetter("LetterLabelChaomorphPasses".Translate(animal.label).CapitalizeFirst(),
										   "LetterChaomorphPasses".Translate(animal.label), LetterDefOf.PositiveEvent, pawn);
			return true;
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f, false, null);
		}
	}
}
