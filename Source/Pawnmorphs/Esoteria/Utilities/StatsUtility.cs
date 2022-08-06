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
        private interface IInvokable
        {
            void Invoke(Pawn pawn, StatDef stat, float oldValue, float newValue);
        }

        public class StatEventRegistry : IInvokable
        {
            public delegate void StatChangedHandler(Pawn pawn, StatDef stat, float oldValue, float newValue);
            public event StatChangedHandler StatChanged;

            void IInvokable.Invoke(Pawn pawn, StatDef stat, float oldValue, float newValue)
            {
                StatChanged?.Invoke(pawn, stat, oldValue, newValue);
            }
        }

        private static Dictionary<ulong, TimedCache<float>> _statCache;
        private static Dictionary<ushort, StatEventRegistry> _events;

        public static StatEventRegistry GetEvents(StatDef statDef)
        {
            if (_events.TryGetValue(statDef.index, out var events) == false)
            {
                events = new StatEventRegistry();
                _events[statDef.index] = events;
            }

            return events;
        }



        static StatsUtility()
        {
            _statCache = new Dictionary<ulong, TimedCache<float>>(400);
            _events = new Dictionary<ushort, StatEventRegistry>(20);
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
                cachedValue.ValueChanged += (TimedCache<float> sender, float oldValue, float newValue) => CachedValue_ValueChanged(pawn, statDef, oldValue, newValue);
                cachedValue.Update();
                _statCache[lookupID] = cachedValue;
            }

            return cachedValue.GetValue(maxAge);
        }

        private static void CachedValue_ValueChanged(Pawn pawn, StatDef statDef, float oldValue, float newValue)
        {
            if (_events.TryGetValue(statDef.index, out var events))
                ((IInvokable)events).Invoke(pawn, statDef, oldValue, newValue);
        }
    }
}
