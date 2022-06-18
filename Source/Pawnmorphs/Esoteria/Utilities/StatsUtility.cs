using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Utilities
{
    /// <summary>
    /// Utility class for making getting pawn's stats easy and simple while maintaining performance.
    /// </summary>
    public static class StatsUtility
    {
        private static Dictionary<ulong, TimedCache<float>> _statCache;

        static StatsUtility()
        {
            _statCache = new Dictionary<ulong, TimedCache<float>>(400);
        }

        /// <summary>
        /// Gets the specied statDef of the specific pawn and adds it to the caching management if not already.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="statDef">The stat definition.</param>
        /// <param name="maxAge">Max amount of ticks since stat was last updated.</param>
        /// <returns></returns>
        public static float? GetStat(Pawn pawn, StatDef statDef, int maxAge)
        {
            ulong lookupID = (ulong)pawn.thingIDNumber << 32 | statDef.index;


            if (_statCache.TryGetValue(lookupID, out TimedCache<float> cachedValue) == false)
            {
                if (pawn.Spawned == false)
                    return null;

                // Cache new stat
                cachedValue = new TimedCache<float>(() => pawn.GetStatValueForPawn(statDef, pawn));
                _statCache[lookupID] = cachedValue;
            }

            return cachedValue.GetValue(maxAge);
        }
    }
}
