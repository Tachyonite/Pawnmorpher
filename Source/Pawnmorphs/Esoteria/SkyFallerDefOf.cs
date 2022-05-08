using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// Custom <see cref="Skyfaller"/> defs. 
    /// </summary>
    [DefOf]
    public class SkyFallerDefOf
    {
        static SkyFallerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SkyFallerDefOf));
        }

        /// <summary>
        /// Skyfaller to animate flight when landing.
        /// </summary>
        public static ThingDef FlightIncoming;

        /// <summary>
        /// Skyfaller to animate flight when taking off.
        /// </summary>
        public static ThingDef FlightLeaving;
    }
}
