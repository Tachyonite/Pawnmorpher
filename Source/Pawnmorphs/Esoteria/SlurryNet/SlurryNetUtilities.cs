// SlurryNetUtilities.cs created by Iron Wolf for Pawnmorph on 11/21/2020 10:58 AM
// last updated 11/21/2020  10:58 AM

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
    }
}