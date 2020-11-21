// SlurryNet.cs created by Iron Wolf for Pawnmorph on 11/21/2020 10:48 AM
// last updated 11/21/2020  10:48 AM

//code referenced from Vanilla Power Extended's gas network

//TODO per tick production/consumption calc  

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace Pawnmorph.SlurryNet
{
    /// <summary>
    ///     class for storing the net containing/production/draining of a mutagen grid
    /// </summary>
    public class SlurryNet
    {
        [NotNull] readonly
            private BoolGrid _disjointSet;

        [NotNull] readonly
            private List<SlurryNetComp> _connectors;

        [NotNull] private readonly List<ISlurryNetTrader> _traders;

        [NotNull] private readonly List<ISlurryNetStorage> _storage;


        [NotNull] readonly
            private LinkedList<ISlurryNetTrader> _consumerCache = new LinkedList<ISlurryNetTrader>();

        [NotNull] readonly
            private List<ISlurryNetTrader> _producerCache = new List<ISlurryNetTrader>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="SlurryNet" /> class.
        /// </summary>
        /// <param name="connectors">The connectors.</param>
        /// <param name="map">The map.</param>
        public SlurryNet([NotNull] Map map, [NotNull] IEnumerable<SlurryNetComp> connectors) : this(map)
        {
            foreach (SlurryNetComp slurryNetComp in connectors) Register(slurryNetComp);
        }


        /// <summary>
        ///     Initializes a new instance of the <see cref="SlurryNet" /> class.
        /// </summary>
        /// <param name="map">The map.</param>
        public SlurryNet([NotNull] Map map)
        {
            _disjointSet = new BoolGrid(map);
            _connectors = new List<SlurryNetComp>();
            _traders = new List<ISlurryNetTrader>();
            _storage = new List<ISlurryNetStorage>();
            Map = map;
#if DEBUG

            _drawer = new CellBoolDrawer(i => _disjointSet[i], () => _color, i => _color, map.Size.x, map.Size.z);

#endif
        }

        /// <summary>
        ///     Gets or sets the production.
        /// </summary>
        /// <value>
        ///     The production.
        /// </value>
        public float Production { get; protected set; }

        /// <summary>
        ///     Gets or sets the consumption.
        /// </summary>
        /// <value>
        ///     The consumption.
        /// </value>
        public float Consumption { get; protected set; }

        /// <summary>
        ///     Gets or sets the stored.
        /// </summary>
        /// <value>
        ///     The stored.
        /// </value>
        public float Stored { get; protected set; }


        /// <summary>
        ///     Gets the map.
        /// </summary>
        /// <value>
        ///     The map.
        /// </value>
        [NotNull]
        public Map Map { get; }

        /// <summary>
        ///     Gets the connectors.
        /// </summary>
        /// <value>
        ///     The connectors.
        /// </value>
        [NotNull]
        public IReadOnlyList<SlurryNetComp> Connectors => _connectors;

        /// <summary>
        ///     Gets the traders.
        /// </summary>
        /// <value>
        ///     The traders.
        /// </value>
        [NotNull]
        public IReadOnlyList<ISlurryNetTrader> Traders => _traders;

        /// <summary>
        ///     Gets the storages.
        /// </summary>
        /// <value>
        ///     The storages.
        /// </value>
        [NotNull]
        public IReadOnlyList<ISlurryNetStorage> StorageComps => _storage;


        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        public virtual void Destroy()
        {
            foreach (SlurryNetComp connector in Connectors)
                connector.Network = null;

#if DEBUG
            _drawer.SetDirty();
#endif
        }

        /// <summary>
        ///     Draws the specified amount.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public virtual void Draw(float amount)
        {
            // draw chunks of gas until amount fulfilled, 
            // chunksize starts at amount / numStorages,
            // but is decreased when necessary.
            int numChunks = 2 * StorageComps.Count;
            float chunkSize = amount / numChunks;
            var iteration = 0;

            while (amount > 0
                && iteration < numChunks
                && // worst case; all chunks come from same storage
                   StorageComps.Any(s => s.Storage > 0))
            {
                foreach (ISlurryNetStorage storage in StorageComps.Where(s => s.Storage > 0))
                {
                    float chunk = Mathf.Min(chunkSize, storage.Storage, amount);

                    if (!(chunk >= Mathf.Epsilon)) continue;
                    amount -= chunk;
                }

                iteration++;
            }

            if (amount > Mathf.Epsilon)
                Log.Warning("Tried to draw slurry than is available");
        }

        /// <summary>
        /// Registers the specified comp.
        /// </summary>
        /// <param name="comp">The comp.</param>
        /// <exception cref="ArgumentNullException">comp</exception>
        public virtual void Register([NotNull] SlurryNetComp comp)
        {
            if (comp == null) throw new ArgumentNullException(nameof(comp));
            _connectors.Add(comp);
            if (comp is ISlurryNetTrader trader) _traders.Add(trader);
            if (comp is ISlurryNetStorage storage) _storage.Add(storage);

            comp.Network = this;

            foreach (IntVec3 intVec3 in comp.parent.OccupiedRect().Cells) _disjointSet.Set(intVec3, true);


#if DEBUG
            _drawer.SetDirty();
#endif
        }


        /// <summary>
        /// Updates the net.
        /// </summary>
        /// <param name="deltaT">the number of ticks that have elapsed since this was previously called</param>
        public virtual void Update(int deltaT)
        {
            float mult = GetPerTickMultiplier(deltaT); 
            FillTraderCaches();
            UpdateTickStats(_consumerCache, _producerCache);

            float effProd = Production / mult;
            float effCons = Consumption / mult;

            //need to stop a little before zero 
            float available = effProd + Mathf.Max(Stored - 0.5f, 0);

            foreach (ISlurryNetTrader slurryNetTrader in _consumerCache)
            {
                if(available <= 0) break;
                var given = Mathf.Min(available, slurryNetTrader.SlurryUsed / mult);
                if (slurryNetTrader.TryReceiveSlurry(given))
                {
                    available -= given; 
                }
            }

            if(available > 0)
            {
                Store(available);
            }
        }

        private void Store(float available)
        {
            float cap = 0;
            foreach (ISlurryNetStorage slurryNetStorage in StorageComps)
            {
                cap += slurryNetStorage.Capacity;
            }

            var amount = Mathf.Min(available, cap);
            if (amount <= float.Epsilon) return;

            foreach (ISlurryNetStorage slurryNetStorage in StorageComps)
            {
                var a = amount * slurryNetStorage.Capacity / cap;
                slurryNetStorage.Storage += a; 
            }
        }


        /// <summary>
        /// Gets the per tick multiplier.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns></returns>
        protected float GetPerTickMultiplier(int ticks)
        {
            return ((float) GenDate.TicksPerDay) / ticks;
        }

        /// <summary>
        ///     Updates the tick stats Production, Consumption, and Stored
        /// </summary>
        /// <param name="consumers">The consumers.</param>
        /// <param name="producers">The producers.</param>
        protected void UpdateTickStats([NotNull] IEnumerable<ISlurryNetTrader> consumers,
                                       [NotNull] IEnumerable<ISlurryNetTrader> producers)
        {
            Production = 0;
            Consumption = 0;
            Stored = 0;
            foreach (ISlurryNetTrader slurryNetTrader in consumers) Consumption += slurryNetTrader.SlurryUsed;

            foreach (ISlurryNetTrader slurryNetTrader in producers) Production += -slurryNetTrader.SlurryUsed;

            foreach (ISlurryNetStorage slurryNetStorage in StorageComps) Stored += slurryNetStorage.Storage;
        }

        [Conditional("DEBUG")]
        internal void DebugOnGUI()
        {
            _drawer.CellBoolDrawerUpdate();
            _drawer.MarkForDraw();
        }


        private void FillTraderCaches()
        {
            //take these from a pool? 
            _consumerCache.Clear();
            _producerCache.Clear();

            foreach (ISlurryNetTrader trader in Traders)
                if (trader.SlurryUsed < 0)
                    _producerCache.Add(trader);
                else if (trader.SlurryUsed > 0)
                {
                    var n = _consumerCache.First; //sort the consumers while we add them to the cache, useful later 
                    while (n != null && n.Value.SlurryUsed < trader.SlurryUsed)
                    {
                        n = n.Next; 
                    }

                    if (n == null)
                        _consumerCache.AddLast(trader);
                    else
                        _consumerCache.AddBefore(n, trader); 
                }
        }


#if DEBUG
        public Color _color = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
        [NotNull] public CellBoolDrawer _drawer;
#endif
    }
}