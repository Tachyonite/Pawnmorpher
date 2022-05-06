using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Abilities.Skyfallers
{
    [DefOf]
    public class SkyFallerDefOf
    {
        static SkyFallerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SkyFallerDefOf));
        }

        public static ThingDef FlightIncoming;
        public static ThingDef FlightLeaving;
    }
}
