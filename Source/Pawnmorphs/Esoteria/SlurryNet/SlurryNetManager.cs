// SlurryNetManager.cs created by Iron Wolf for Pawnmorph on 11/22/2020 4:07 PM
// last updated 11/22/2020  4:07 PM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.SlurryNet
{
    /// <summary>
    ///     manager class for slurry nets on the map
    /// </summary>
    /// <seealso cref="Verse.MapComponent" />
    public class SlurryNetManager : MapComponent
    {
        private const int NET_TICK_PERIOD = 25;

        [NotNull] readonly
            private List<SlurryNet> _nets = new List<SlurryNet>();

        [NotNull] readonly
            private LinkedList<SlurryNetComp> _scratchList = new LinkedList<SlurryNetComp>();

        private bool _initialized;


        
        /// <summary>
        ///     Initializes a new instance of the <see cref="SlurryNetManager" /> class.
        /// </summary>
        /// <param name="map">The map.</param>
        public SlurryNetManager(Map map) : base(map)
        {
        }

        /// <summary>
        ///     Gets the nets.
        /// </summary>
        /// <value>
        ///     The nets.
        /// </value>
        [NotNull]
        public IReadOnlyList<SlurryNet> Nets => _nets;

        /// <summary>
        ///     Finalizes the initialization.
        /// </summary>
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Initialize();
        }

        /// <summary>
        ///     Gets the net at.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns></returns>
        [CanBeNull]
        public SlurryNet GetNetAt(IntVec3 cell)
        {
            foreach (SlurryNet slurryNet in Nets)
                if (slurryNet.Contains(cell))
                    return slurryNet;

            return null;
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                Log.Error($"Trying to initialize net on map:{map.uniqueID} more then once!");
                return;
            }


            _initialized = true;

            IEnumerable<SlurryNetComp> allComps = map.listerBuildings.allBuildingsColonist
                                                     .Select(t => t?.TryGetComp<SlurryNetComp>())
                                                     .Where(t => t != null);

            CreateSlurryNets(allComps);
        }

        /// <summary>
        ///     called every tick .
        /// </summary>
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            foreach (SlurryNet slurryNet in Nets)
                if (slurryNet.IsHashIntervalTick(NET_TICK_PERIOD))
                    slurryNet.Update(NET_TICK_PERIOD);
        }

        /// <summary>
        ///     Maps the component update.
        /// </summary>
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();


            DebugDrawNets();
        }

        /// <summary>
        ///     notifies this instance that a connector was added
        /// </summary>
        /// <param name="comp">The comp.</param>
        /// <exception cref="ArgumentNullException">comp</exception>
        public void NotifyConnectorAdded([NotNull] SlurryNetComp comp)
        {
            if (comp == null) throw new ArgumentNullException(nameof(comp));

            DebugLogUtils.LogMsg(LogLevel.Messages, $"adding connector for '{comp.parent.Label}'");

            if (comp.Network != null || Nets.Any(n => n.Connectors.Contains(comp)))
            {
                Log.Error($"adding slurry comp {comp.parent.Label} which is already part of a network");
                return;
            }

            List<SlurryNet> neighbors = comp.GetAdjacentSlurryComps()
                                            .Select(n => n.Network)
                                            .Where(n => n != null) 
                                            .Distinct()
                                            .ToList();

            if (neighbors.Count == 1)
            {
                neighbors[0].Register(comp);
            }
            else
            {
                foreach (SlurryNet slurryNet in neighbors) DestroyNet(slurryNet);

                SlurryNet net = CreateSlurryNetFrom(comp, null);
                _nets.Add(net);
            }
        }


        /// <summary>
        ///     Notifies this instance that the connector was destroyed.
        /// </summary>
        /// <param name="comp">The comp.</param>
        /// <exception cref="ArgumentNullException">comp</exception>
        public void NotifyConnectorDestroyed([NotNull] SlurryNetComp comp)
        {
            if (comp == null) throw new ArgumentNullException(nameof(comp));
            if (comp.Network == null) return;

            DestroyNet(comp.Network);
            CreateSlurryNets(comp.GetAdjacentSlurryComps());
        }

        private SlurryNet CreateSlurryNetFrom([NotNull] SlurryNetComp root, [CanBeNull] HashSet<SlurryNetComp> seen)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            seen = seen ?? new HashSet<SlurryNetComp>();
            var queue = new Queue<SlurryNetComp>();
            var network = new HashSet<SlurryNetComp>();

            if (!root.TransmitsNow)
            {
                seen.Add(root);
                return new SlurryNet(map, root);
            }

            // basic flood fill. Start somewhere, put all neighbours on a queue,
            // continue until there are no more neighbours.
            queue.Enqueue(root);
            seen.Add(root);
            while (queue.Count > 0)
            {
                SlurryNetComp cur = queue.Dequeue();
                // sanity check; is this already in a network?
                if (cur.Network != null)
                    Log.Error($"trying to add {cur.parent.Label} to a new network while it still has a network");

                network.Add(cur);

                // check neighbours, add to queue if eligible
                foreach (SlurryNetComp neighbour in cur.GetAdjacentSlurryComps())
                    if (!seen.Contains(neighbour))
                    {
                        seen.Add(neighbour);
                        if (!neighbour.TransmitsNow) continue;
                        queue.Enqueue(neighbour);
                    }
            }

            return new SlurryNet(map, network);
        }

        private void CreateSlurryNets([NotNull] IEnumerable<SlurryNetComp> comps)
        {
            _scratchList.Clear();
            _scratchList.AddRange(comps);
            var seen = new HashSet<SlurryNetComp>();
            while (_scratchList.Count > 0)
            {
                seen.Clear();
                _nets.Add(CreateSlurryNetFrom(_scratchList.First.Value, seen));


                foreach (SlurryNetComp slurryNetComp in seen) _scratchList.Remove(slurryNetComp);
            }
        }

        [Conditional("DEBUG")]
        private void DebugDrawNets()
        {
            if (!SlurryNet.slurryNetDebugging) return; 
            foreach (SlurryNet slurryNet in Nets) slurryNet.DebugOnGUI();
        }

        private void DestroyNet(SlurryNet slurryNet)
        {
            _nets.Remove(slurryNet);
            slurryNet.Destroy();
        }
    }
}