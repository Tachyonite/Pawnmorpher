// SlurryNetComp.cs created by Iron Wolf for Pawnmorph on 11/21/2020 11:01 AM
// last updated 11/21/2020  11:01 AM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.SlurryNet
{
    /// <summary>
    /// abstract base class for all slurry net comps 
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public abstract class SlurryNetComp : ThingComp 
    {
        /// <summary>
        /// Gets or sets the network.
        /// </summary>
        /// <value>
        /// The network.
        /// </value>
        [CanBeNull]
        public abstract SlurryNet Network { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance can transmit slurry.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance transmits slurry; otherwise, <c>false</c>.
        /// </value>
        public abstract bool TransmitsNow { get; }

    }

    /// <summary>
    /// interface for all slurry net traders 
    /// </summary>
    public interface ISlurryNetTrader
    {
        /// <summary>
        /// Gets the slurry used.
        /// </summary>
        /// gets the amount of slurry used by this trader. positive values are used while negative values are production 
        /// <value>
        /// The slurry used.
        /// </value>
        float SlurryUsed { get; }


        /// <summary>
        /// Tries to receive some amount of slurry.
        /// </summary>
        /// <param name="slurryReceived">The slurry received.</param>
        /// <returns></returns>
        bool TryReceiveSlurry(float slurryReceived);
    }


    /// <summary>
    /// the interface for all slurry net storage comps 
    /// </summary>
    public interface ISlurryNetStorage
    {
        /// <summary>
        /// Gets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        float Capacity { get; }

        /// <summary>
        /// Gets or sets the storage.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        float Storage { get; set; }
    }
}