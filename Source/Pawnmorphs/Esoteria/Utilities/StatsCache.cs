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
    public class StatsCache : WorldComponent
    {
        private class CachedStat
        {
            public Cached<float> Stat { get; set; }

            /// <summary>
            /// Timestamp in ticks for when the stat was last recalculated.
            /// </summary>
            public int Timestamp { get; set; }

            public CachedStat(Cached<float> stat, int ticks)
            {
                Stat = stat;
                Timestamp = ticks;
            }
        }

        const byte STAT_UPDATE_INTERVAL_TICKS = 200;
        const byte STAT_MAX_UPDATE_PER_TICK = 5;


        private static StatsCache _instance;

        public static StatsCache Instance
        {
            get 
            { 
                if (_instance == null)
                    _instance = Find.World.GetComponent<StatsCache>();

                return _instance; 
            }
        }

        private Dictionary<Pawn, Dictionary<StatDef, CachedStat>> _statCache;
        private Queue<CachedStat> _cachedStatUpdateQueue;
        private TickManager _tickManager;
        private int _hibernate;

        public StatsCache(World world) : base(world)
        {
            _statCache = new Dictionary<Pawn, Dictionary<StatDef, CachedStat>>();
            _tickManager = Find.TickManager;
            _cachedStatUpdateQueue = new Queue<CachedStat>(100);
            _hibernate = 0;
        }

        public override void WorldComponentTick()
        {
            if (_cachedStatUpdateQueue.Count == 0)
                return;

            if (_hibernate-- > 0)
                return;

            int ticksSinceOldestStat = _tickManager.TicksGame - _cachedStatUpdateQueue.Peek().Timestamp;
            if (ticksSinceOldestStat < STAT_UPDATE_INTERVAL_TICKS)
            {
                // 30 ticks since the oldest was updated and stat update interval is 60, then hibernate for 30 ticks - 1 for current tick
                _hibernate = STAT_UPDATE_INTERVAL_TICKS - ticksSinceOldestStat - 1;
                return;
            }

            int calculateThisTick = Math.Min(_cachedStatUpdateQueue.Count, STAT_MAX_UPDATE_PER_TICK);
            CachedStat stat;
            for (byte updated = 0; updated < calculateThisTick; updated++)
            {
                stat = _cachedStatUpdateQueue.Dequeue();
                UpdateStat(stat);
                _cachedStatUpdateQueue.Enqueue(stat);
            }
        }

        /// <summary>
        /// Gets the specied statDef of the specific pawn and adds it to the caching management if not already.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="statDef">The stat definition.</param>
        /// <returns></returns>
        public float? GetStat(Pawn pawn, StatDef statDef)
        {
            // Cannot get stat for unspawned pawn.
            if (pawn.Spawned == false)
                return null;

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
                // Cache new stat
                Cached<float> statValue = new Cached<float>(() => pawn.GetStatValueForPawn(statDef, pawn));
                statValue.Recalculate();
                stat = new CachedStat(statValue, _tickManager.TicksGame);

                _cachedStatUpdateQueue.Enqueue(stat);
                pawnCache[statDef] = stat;
            }

            return stat.Stat.Value;
        }

        /// <summary>
        /// Recalculates the stat and updates the tick timestamp.
        /// </summary>
        /// <param name="stat">The stat.</param>
        private void UpdateStat(CachedStat stat)
        {
            if (_tickManager.TicksGame - stat.Timestamp > STAT_UPDATE_INTERVAL_TICKS)
            {
                stat.Stat.Recalculate();
                stat.Timestamp = _tickManager.TicksGame;
            }
        }
    }
}
