// IncidentWorker_MutagenicShipCrash.cs modified by Iron Wolf for Pawnmorph on 12/23/2019 6:44 PM
// last updated 12/23/2019  6:44 PM

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

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
            return ((Map) parms.target).listerThings.ThingsOfDef(def.shipPart).Count <= 0;
        }

        /// <summary>
        /// Tries the execute worker.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        protected override bool TryExecuteWorker(  IncidentParms parms)
        {
            var target = (Map) parms.target;
            var num1 = 0;
            int countToSpawn = CountToSpawn;
            var targetInfoList = new List<TargetInfo>();
            float num2 = Rand.Range(0.0f, 360f);
            IntVec3 cell;
            for (var index = 0;
                 index < countToSpawn
              && CellFinderLoose.TryFindSkyfallerCell(PMThingDefOf.CrashedMutagenicShipPartIncoming, target, out cell, 14, new IntVec3(), -1,
                                                      false, true, true, true);
                 ++index)
            {
                var buildingCrashedShipPart = (Building_CrashedShipPart) ThingMaker.MakeThing(def.shipPart);
                buildingCrashedShipPart.SetFaction(Faction.OfMechanoids);
                buildingCrashedShipPart.GetComp<CompSpawnerMechanoidsOnDamaged>().pointsLeft =
                    Mathf.Max(parms.points * 0.9f, 300f);
                Skyfaller skyfaller =
                    SkyfallerMaker.MakeSkyfaller(PMThingDefOf.CrashedMutagenicShipPartIncoming, buildingCrashedShipPart);
                skyfaller.shrapnelDirection = num2;
                GenSpawn.Spawn(skyfaller, cell, target);
                ++num1;
                targetInfoList.Add(new TargetInfo(cell, target));
            }

            if (num1 > 0)
                SendStandardLetter(targetInfoList);
            return num1 > 0;
        }
    }
}