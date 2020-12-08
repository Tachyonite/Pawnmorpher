// SlurryNetUtilities.cs created by Iron Wolf for Pawnmorph on 11/21/2020 10:58 AM
// last updated 11/21/2020  10:58 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.SlurryNet
{
    /// <summary>
    /// static class for various slurry net related utilities 
    /// </summary>
    public static class SlurryNetUtilities
    {
        /// <summary>
        /// Determines whether this instance is producer.
        /// </summary>
        /// <param name="trader">The trader.</param>
        /// <returns>
        ///   <c>true</c> if the specified trader is producer; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProducer([NotNull] this ISlurryNetTrader trader)
        {
            return trader.SlurryUsed < 0; 
        }
        /// <summary>
        /// gets the hash offset for ticks 
        /// </summary>
        /// <param name="network">The network.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">network</exception>
        public static int HashOffsetTicks([NotNull] this SlurryNet network)
        {
            if (network == null) throw new ArgumentNullException(nameof(network));
            return Find.TickManager.TicksGame + network.GetHashCode().HashOffset();
        }

        /// <summary>
        /// Determines whether the given interval lies within the tick interval for the given network .
        /// </summary>
        /// <param name="network">The network.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        ///   <c>true</c> if [is hash interval tick] [the specified interval]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">network</exception>
        public static bool IsHashIntervalTick([NotNull] this SlurryNet network, int interval)
        {
            if (network == null) throw new ArgumentNullException(nameof(network));
            return network.HashOffsetTicks() % interval == 0;
        }

        /// <summary>
        /// Gets the slurry net manager.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">map</exception>
        [NotNull]
        public static SlurryNetManager GetSlurryNetManager([NotNull] this Map map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            return map.GetComponent<SlurryNetManager>();
        }


        /// <summary>
        /// Gets the adjacent slurry comps.
        /// </summary>
        /// <param name="comp">The comp.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">comp</exception>
        [NotNull]
        public static IEnumerable<SlurryNetComp> GetAdjacentSlurryComps([NotNull] this SlurryNetComp comp)
        {
            if (comp == null) throw new ArgumentNullException(nameof(comp));


            foreach (IntVec3 intVec3 in comp.parent.CellsAdjacent8WayAndInside())
            {
                if(comp.parent.Map == null) continue;
                var l = intVec3.GetThingList(comp.parent.Map); 
                if(l == null) continue;
                foreach (Thing thing in l)
                {
                    if(thing == comp.parent) continue;
                    var c = thing?.TryGetComp<SlurryNetComp>();
                    if(c == null) continue;
                    yield return c; 
                }
            }
        }
    }
}