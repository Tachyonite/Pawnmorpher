// AffinityTracker.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:25 AM
// last updated 09/22/2019  11:25 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{

    /// <summary>
    /// thing comp for tracking 'mutation affinities'
    /// </summary>
    public class AffinityTracker : ThingComp, IMutationEventReceiver, IRaceChangeEventReceiver
    {
        private List<Affinity> _affinities = new List<Affinity>();

        private Pawn Pawn => (Pawn) parent;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref _affinities, "affinities", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (Affinity affinity in _affinities)
                {
                    affinity.Initialize(); 
                }
            }

        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn"); 
        }


        public IEnumerable<Affinity> Affinities => _affinities;

        public void Add([NotNull] Affinity affinity)
        {
            if (_affinities.Contains(affinity))
            {
                Log.Warning($"trying to add affinity  to pawn more then once");
                return; 
            }

            _affinities.Add(affinity);
            affinity.Added(Pawn); 
        }

        private List<Affinity> _rmCache = new List<Affinity>(); 
        public void Remove(Affinity affinity)
        {
            _rmCache.Add(affinity);  //don't remove them quite yet '
        }

        public override void CompTick()
        {
            base.CompTick();

            foreach (Affinity affinity in _affinities)
            {
                affinity.PostTick();
            }

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

        void IMutationEventReceiver.MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            foreach (var affinity in _affinities.OfType<IMutationEventReceiver>())
            {
                affinity.MutationAdded(mutation, tracker); 
            }
        }

        void IMutationEventReceiver.MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            foreach (var affinity in _affinities.OfType<IMutationEventReceiver>())
            {
                affinity.MutationRemoved(mutation, tracker); 
            }
        }

        void IRaceChangeEventReceiver.OnRaceChange(ThingDef oldRace)
        {
            foreach (IRaceChangeEventReceiver raceChangeEventReceiver in _affinities.OfType<IRaceChangeEventReceiver>())
            {
                raceChangeEventReceiver.OnRaceChange(oldRace); 
            }
        }
    }
}