// SimpleSlurryNetStorageComp.cs created by Iron Wolf for Pawnmorph on 11/21/2020 2:28 PM
// last updated 11/21/2020  2:28 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.SlurryNet
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Pawnmorph.SlurryNet.SlurryNetComp" />
    /// <seealso cref="Pawnmorph.SlurryNet.ISlurryNetStorage" />
    public class SimpleSlurryNetStorageComp :  SlurryNetComp, ISlurryNetStorage
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

        /// <summary>
        /// Gets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        public float Capacity => RefuelableComp.TargetFuelLevel;

        /// <summary>
        /// Gets or sets the storage.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        public float Storage {
            get { return RefuelableComp.Fuel; }
            set
            {
                var delta = RefuelableComp.Fuel - value;
                if (delta < 0)
                {
                    RefuelableComp.ConsumeFuel(-delta);
                }
                else
                {
                    RefuelableComp.Refuel(delta);
                }
            }
        }
    }


    
}