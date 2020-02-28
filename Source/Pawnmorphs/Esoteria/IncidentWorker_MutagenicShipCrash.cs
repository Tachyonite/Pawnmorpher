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
    public class IncidentWorker_MutagenicShipCrash : IncidentWorker_CrashedShipPart
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
            return ((Map) parms.target).listerThings.ThingsOfDef(def.mechClusterBuilding).Count <= 0;
        }

    }
}