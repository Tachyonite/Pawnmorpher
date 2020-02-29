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
    public class IncidentWorker_MutagenicShipCrash : IncidentWorker_CrashedShipPart
    {
        // Field ShipPointsFactor with token 0400094F
        private const float ShipPointsFactor = 0.9f;

        // Field IncidentMinimumPoints with token 04000950
        private const int IncidentMinimumPoints = 300;

        /// <summary>
        /// Tries to execute the worker.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            var targetInfoList = new List<TargetInfo>();
            ThingDef shipPartDef = def.mechClusterBuilding;
            IntVec3 dropPodLocation = MechClusterUtility.FindDropPodLocation(map, spot =>
            {
                if (!spot.Fogged(map) && GenConstruct.CanBuildOnTerrain(shipPartDef, spot, map, Rot4.North))
                    return GenConstruct.CanBuildOnTerrain(shipPartDef,
                                                          new IntVec3(spot.x - Mathf.CeilToInt(shipPartDef.size.x / 2f), spot.y,
                                                                      spot.z), map, Rot4.North);
                return false;
            });
            if (dropPodLocation == IntVec3.Invalid)
                return false;
            float num = Mathf.Max(parms.points * 0.9f, 300f);
            var genParams = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = map.Tile,
                faction = Faction.OfMechanoids,
                points = num
            };
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(genParams)
                                                   .ToList();
            Thing innerThing = ThingMaker.MakeThing(shipPartDef);
            innerThing.SetFaction(Faction.OfMechanoids);
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend(new List<Thing>
            {
                innerThing
            }, Faction.OfMechanoids, 28f, dropPodLocation, false, false), map, list);
            DropPodUtility.DropThingsNear(dropPodLocation, map, list);
            foreach (Thing thing in list)
                thing.TryGetComp<CompCanBeDormant>()?.ToSleep();
            targetInfoList.AddRange(list.Select(p => new TargetInfo(p)));
            GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(PMThingDefOf.CrashedMutagenicShipPartIncoming, innerThing), dropPodLocation, map);
            targetInfoList.Add(new TargetInfo(dropPodLocation, map));
            SendStandardLetter(parms, targetInfoList, Array.Empty<NamedArgument>());
            return true;
        }


        /// <summary>
        /// Gets the count to spawn.
        /// </summary>
        /// <value>
        /// The count to spawn.
        /// </value>
        protected virtual int CountToSpawn => 1;

        /// <summary>
        /// Determines whether this instance can fire now with the specified parms
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns>
        ///   <c>true</c> if this instance can fire now with the specified parms otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanFireNowSub( IncidentParms parms)
        {
            return ((Map) parms.target).listerThings.ThingsOfDef(def.mechClusterBuilding).Count <= 0;
        }

    }
}