// SlurryFueld.cs created by Iron Wolf for Pawnmorph on 11/07/2020 10:40 AM
// last updated 11/07/2020  10:40 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    /// CompRefuelable specifically for mutagenic slurry that takes slurry from nearby mutagen tanks
    /// </summary>
    /// <seealso cref="RimWorld.CompRefuelable" />
    public class SlurryFueled : CompRefuelable
    {
        private Room CurRoom => parent?.Position.GetRoom(parent.Map);


        [NotNull]
        IEnumerable<CompRefuelable> GetNearbyTanks()
        {
            var rm = CurRoom; 
            if(rm == null) yield break;

            foreach (Thing containedThing in rm.ContainedThings(PMThingDefOf.PM_MutagenTank))
            {
                if(containedThing == parent) continue;
                var comp = containedThing.TryGetComp<CompRefuelable>();
                if(comp == null) continue;
                yield return comp; 
            }
        }


        /// <summary>
        /// Posts the spawn setup.
        /// </summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            TryRefuelFromTank();
        }

        /// <summary>
        /// Notifies the signal received.
        /// </summary>
        /// <param name="signal">The signal.</param>
        public override void Notify_SignalReceived(Signal signal)
        {
            if (signal.tag == RanOutOfFuelSignal)
            {
                TryRefuelFromTank();
            }
        }

        private void TryRefuelFromTank()
        {
            float refuel = Fuel;
            foreach (CompRefuelable tank in GetNearbyTanks())
            {
                var needed = (TargetFuelLevel - refuel);
                if(needed <= 0) break;
                var take = Mathf.Min(tank.Fuel, needed);
                if (take <= 0) continue;
                refuel += take;
                tank.ConsumeFuel(take);
            }
            Refuel(refuel);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="RimWorld.CompProperties_Refuelable" />
    public class SlurryFueledProps : CompProperties_Refuelable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlurryFueledProps"/> class.
        /// </summary>
        public SlurryFueledProps()
        {
            compClass = typeof(SlurryFueled);
        }
    }
    

}