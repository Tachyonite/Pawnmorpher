// AffinityTracker.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:25 AM
// last updated 09/22/2019  11:25 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
    /// <summary>
    ///     thing comp for tracking 'mutation affinities'
    /// </summary>
    public class AffinityTracker : ThingComp, IMutationEventReceiver, IRaceChangeEventReceiver
    {
        private List<Affinity> _affinities = new List<Affinity>();

        private readonly List<Affinity> _rmCache = new List<Affinity>();

        void IMutationEventReceiver.MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            foreach (IMutationEventReceiver affinity in _affinities.OfType<IMutationEventReceiver>())
                affinity.MutationAdded(mutation, tracker);
        }

        void IMutationEventReceiver.MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            foreach (IMutationEventReceiver affinity in _affinities.OfType<IMutationEventReceiver>())
                affinity.MutationRemoved(mutation, tracker);
        }

        void IRaceChangeEventReceiver.OnRaceChange(ThingDef oldRace)
        {
            foreach (IRaceChangeEventReceiver raceChangeEventReceiver in _affinities.OfType<IRaceChangeEventReceiver>())
                raceChangeEventReceiver.OnRaceChange(oldRace);
        }


        public IEnumerable<Affinity> Affinities => _affinities;

        private Pawn Pawn => (Pawn) parent;

        public void Add([NotNull] Affinity affinity)
        {
            if (_affinities.Contains(affinity))
            {
                Log.Warning($"trying to add affinity {affinity.Label} to pawn more then once");
                return;
            }

            if (_affinities.Count(a => a.def == affinity.def) > 0)
            {
                Log.Warning($"trying to add an affinity with def {affinity.def.defName} to pawn {Pawn.Name} which already has an affinity of that def");
                return;
            }

            _affinities.Add(affinity);
            affinity.Added(Pawn);
        }

        public void Add([NotNull] AffinityDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));

            if (_affinities.Count(a => a.def == def) > 0)
            {
                Log.Warning($"trying to add an affinity with def {def.defName} to pawn {Pawn.Name} which already has an affinity of that def");
                return;
            }

            Add(def.CreateInstance());
        }

        public override void CompTick()
        {
            base.CompTick();

            foreach (Affinity affinity in _affinities) affinity.PostTick();

            if (_rmCache.Count != 0)
            {
                foreach (Affinity affinity in _rmCache) //remove the affinities here so we don't invalidate the enumerator above 
                {
                    _affinities.Remove(affinity);
                    affinity.PostRemove();
                }

                _rmCache.Clear();
            }
        }

        /// <summary>
        ///     get the first affinity with the given def
        /// </summary>
        /// <param name="def"></param>
        /// <returns>the affinity with the given def, null if one isn't found </returns>
        [CanBeNull]
        public Affinity GetAffinityOfDef(AffinityDef def)
        {
            return _affinities.FirstOrDefault(a => a.def == def);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref _affinities, "affinities", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                foreach (Affinity affinity in _affinities)
                    affinity.Initialize();
        }

        public void Remove(Affinity affinity)
        {
            _rmCache.Add(affinity); //don't remove them quite yet '
        }

        /// <summary>
        ///     removes the affinity with the given def from the pawn
        /// </summary>
        /// <param name="def"></param>
        public void Remove(AffinityDef def)
        {
            Affinity af = _affinities.FirstOrDefault(a => a.def == def);
            if (af != null) _rmCache.Add(af);
        }
    }
}