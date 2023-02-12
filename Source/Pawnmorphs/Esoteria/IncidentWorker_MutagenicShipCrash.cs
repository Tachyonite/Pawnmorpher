// IncidentWorker_MutagenicShipCrash.cs modified by Iron Wolf for Pawnmorph on 12/23/2019 6:44 PM
// last updated 12/23/2019  6:44 PM

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Pawnmorph
{
	/// <summary>
	///     incident worker for mutagenic ship crash
	/// </summary>
	/// <seealso cref="RimWorld.IncidentWorker" />
	public class IncidentWorker_MutagenicShipCrash : IncidentWorker
	{
		// Field ShipPointsFactor with token 0400094F
		private const float ShipPointsFactor = 0.9f;

		// Field IncidentMinimumPoints with token 04000950
		private const int IncidentMinimumPoints = 300;

		/// <summary>
		///     Gets the count to spawn.
		/// </summary>
		/// <value>
		///     The count to spawn.
		/// </value>
		protected virtual int CountToSpawn => 1;

		/// <summary>
		///     Determines whether this instance can fire now with the specified parms
		/// </summary>
		/// <param name="parms">The parms.</param>
		/// <returns>
		///     <c>true</c> if this instance can fire now with the specified parms otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return ((Map)parms.target).listerThings.ThingsOfDef(def.mechClusterBuilding).Count <= 0;
		}

		/// <summary>
		///     Tries to execute the worker.
		/// </summary>
		/// <param name="parms">The parms.</param>
		/// <returns></returns>
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			var map = (Map)parms.target;
			var list = new List<TargetInfo>();
			ThingDef shipPartDef = def.mechClusterBuilding;
			IntVec3 intVec = FindDropPodLocation(map, CanPlaceAt);
			if (intVec == IntVec3.Invalid) return false;
			float points = Mathf.Max(parms.points * 0.9f, 300f);
			List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Combat,
				tile = map.Tile,
				faction = Faction.OfMechanoids,
				points = points
			})
													.ToList();
			Thing thing = ThingMaker.MakeThing(shipPartDef);
			thing.SetFaction(Faction.OfMechanoids);
			LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend(new List<Thing>
			{
				thing
			}, Faction.OfMechanoids, 28f, intVec, false, false), map, list2);
			DropPodUtility.DropThingsNear(intVec, map, list2);
			foreach (Pawn item in list2) item.TryGetComp<CompCanBeDormant>()?.ToSleep();
			list.AddRange(list2.Select(p => new TargetInfo(p)));
			GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(PMThingDefOf.CrashedMutagenicShipPartIncoming, thing), intVec, map);
			list.Add(new TargetInfo(intVec, map));
			SendStandardLetter(parms, list);
			return true;

			bool CanPlaceAt(IntVec3 loc)
			{
				if (loc.Fogged(map) || !GenAdj.OccupiedRect(loc, Rot4.North, shipPartDef.Size).InBounds(map)) return false;
				return GenConstruct.CanBuildOnTerrain(shipPartDef, loc, map, Rot4.North);
			}
		}


		private static IntVec3 FindDropPodLocation(Map map, Predicate<IntVec3> validator)
		{
			for (var i = 0; i < 200; i++)
			{
				IntVec3 intVec = RCellFinder.FindSiegePositionFrom(DropCellFinder.FindRaidDropCenterDistant(map), map);
				if (validator(intVec)) return intVec;
			}

			return IntVec3.Invalid;
		}
	}
}