﻿// SimpleSlurryRefillerComp.cs created by Iron Wolf for Pawnmorph on 11/21/2020 2:43 PM
// last updated 11/21/2020  2:43 PM

using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.SlurryNet
{
    /// <summary>
    /// slurry net comp that takes slurry from the network to refill a refuelable comp 
    /// </summary>
    /// <seealso cref="Pawnmorph.SlurryNet.SlurryNetComp" />
    /// <seealso cref="Pawnmorph.SlurryNet.ISlurryNetTrader" />
    public class SimpleSlurryRefillerComp : SlurryNetComp, ISlurryNetTrader
    {
        private CompRefuelable _refuelableComp;

        [NotNull]
        CompRefuelable RefuelableComp
        {
            get
            {
                if (_refuelableComp == null)
                {
                    _refuelableComp = parent.TryGetComp<CompRefuelable>();
                }

                return _refuelableComp;
            }
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public Thing Parent => parent; 

        /// <summary>
        /// Gets or sets the network.
        /// </summary>
        /// <value>
        /// The network.
        /// </value>
        public override SlurryNet Network { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance can transmit slurry.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance transmits slurry; otherwise, <c>false</c>.
        /// </value>
        public override bool TransmitsNow => true;


        float SlurryDrawPerDay
        {
            get
            {
                var tg =  GenDate.TicksPerDay
                     * 0.3f
                     * ((RefuelableComp.TargetFuelLevel - RefuelableComp.Fuel + 1) / RefuelableComp.TargetFuelLevel);
                return Mathf.Max(tg, 0); 
            }
        }


        /// <summary>
        /// Gets the slurry used.
        /// </summary>
        /// gets the amount of slurry used by this trader. positive values are used while negative values are production 
        /// <value>
        /// The slurry used.
        /// </value>
        public float SlurryUsed {
            get
            {
                if (RefuelableComp.IsFull) return 0;

                return SlurryDrawPerDay;
            }

        }

        /// <summary>
        /// Tries to receive some amount of slurry.
        /// </summary>
        /// <param name="slurryReceived">The slurry received.</param>
        /// <returns></returns>
        public float TryReceiveSlurry(float slurryReceived)
        {

            var used = Mathf.Min(RefuelableComp.TargetFuelLevel - RefuelableComp.Fuel, slurryReceived); 
            RefuelableComp.Refuel(used);
            return used;
        }
    }
}