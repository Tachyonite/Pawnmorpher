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
        /// <summary>
        /// Timestamp and pawn stat combination.
        /// </summary>
        private class CachedStat
        {
            public Cached<float> Stat { get; set; }

            /// <summary>
            /// Timestamp in ticks for when the stat was last recalculated.
            /// </summary>
            public int Timestamp { get; set; }

            public bool RequestedUpdate { get; set; }

            public CachedStat(Cached<float> stat, int ticks)
            {
                Stat = stat;
                Timestamp = ticks;
                RequestedUpdate = false;
            }
        }

        private static Dictionary<Pawn, Dictionary<StatDef, CachedStat>> _statCache;
        private static TickManager _tickManager;

        static StatsUtility()
        {
            _statCache = new Dictionary<Pawn, Dictionary<StatDef, CachedStat>>();
            _tickManager = Find.TickManager;
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
            Dictionary<StatDef, CachedStat> pawnCache = _statCache.TryGetValue(pawn);
            if (pawnCache == null)
            {
                // Cache new pawn
                pawnCache = new Dictionary<StatDef, CachedStat>();
                _statCache[pawn] = pawnCache;
            }

            CachedStat stat = pawnCache.TryGetValue(statDef);
            if (stat == null)
            {
                if (pawn.Spawned == false)
                    return null;

                // Cache new stat
                Cached<float> statValue = new Cached<float>(() => pawn.GetStatValueForPawn(statDef, pawn));
                stat = new CachedStat(statValue, _tickManager.TicksGame);
                pawnCache[statDef] = stat;
            }
            else if (stat.RequestedUpdate == false && pawn.Spawned)
            {
                // If stat has not already been queued for update, then check if it should be updated.

                // If stat is older than age limit, recalculate.
                if (_tickManager.TicksGame - stat.Timestamp > maxAge)
                {
                    stat.RequestedUpdate = true;
                    LongEventHandler.ExecuteWhenFinished(() =>
                    {
                        // Invalidate
                        stat.Stat.Recalculate();
                        // Get and cache new value
                        var _ = stat.Stat.Value;

                        stat.Timestamp = _tickManager.TicksGame;
                        stat.RequestedUpdate = false;
                    });
                }
            }

            return stat.Stat.Value;
        }
    }
}
