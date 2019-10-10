// AspectTracker.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:25 AM
// last updated 09/22/2019  11:25 AM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
    /// <summary> Thing comp for tracking 'mutation aspects'. </summary>
    public class AspectTracker : ThingComp, IMutationEventReceiver, IRaceChangeEventReceiver, IEnumerable<Aspect>
    {
        private readonly List<Aspect> _rmCache = new List<Aspect>();
        private List<Aspect> _aspects = new List<Aspect>();

        public IEnumerable<Aspect> Aspects => _aspects;

        private Pawn Pawn => (Pawn)parent;

        void IMutationEventReceiver.MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            foreach (IMutationEventReceiver affinity in _aspects.OfType<IMutationEventReceiver>())
                affinity.MutationAdded(mutation, tracker);
        }

        void IMutationEventReceiver.MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            foreach (IMutationEventReceiver affinity in _aspects.OfType<IMutationEventReceiver>())
                affinity.MutationRemoved(mutation, tracker);
        }

        void IRaceChangeEventReceiver.OnRaceChange(ThingDef oldRace)
        {
            var oldMorph = oldRace.GetMorphOfRace();
            var curMorph = parent.def.GetMorphOfRace();
            HandleMorphChangeAffinities(oldMorph, curMorph); 

            foreach (IRaceChangeEventReceiver raceChangeEventReceiver in _aspects.OfType<IRaceChangeEventReceiver>())
                raceChangeEventReceiver.OnRaceChange(oldRace);
        }

        /// <summary> Add the aspect to this pawn at the given stage. </summary>
        public void Add([NotNull] Aspect aspect, int startStage=0)
        {
            if (_aspects.Contains(aspect))
            {
                Log.Warning($"trying to add aspect {aspect.Label} to pawn more then once");
                return;
            }

            if (_aspects.Count(a => a.def == aspect.def) > 0)
            {
                Log.Warning($"trying to add an aspect with def {aspect.def.defName} to pawn {Pawn.Name} which already has an aspect of that def");
                return;
            }

            _aspects.Add(aspect);
            aspect.Added(Pawn, startStage);
            if (aspect.HasCapMods)
            {
                Pawn?.health?.capacities?.Notify_CapacityLevelsDirty();
            }
        }

        public void Notify_AspectChanged(Aspect aspect)
        {
            Pawn?.health?.capacities?.Notify_CapacityLevelsDirty();
        }


        /// <summary> Add the given aspect to this pawn at the specified stage index. </summary>
        public void Add([NotNull] AspectDef def, int startStage=0)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));

            if (_aspects.Count(a => a.def == def) > 0)
            {
                Log.Warning($"trying to add an aspect with def {def.defName} to pawn {Pawn.Name} which already has an aspect of that def");
                return;
            }

            Add(def.CreateInstance(), startStage);
        }

        public override void CompTick()
        {
            base.CompTick();

            foreach (Aspect affinity in _aspects) affinity.PostTick();

            if (_rmCache.Count != 0)
            {
                foreach (Aspect affinity in _rmCache) //remove the affinities here so we don't invalidate the enumerator above 
                {
                    _aspects.Remove(affinity);
                    affinity.PostRemove();
                }

                _rmCache.Clear();
            }
        }

        public bool Contains(Aspect aspect)
        {
            return _aspects.Contains(aspect);
        }

        public bool Contains(AspectDef aspect)
        {
            return _aspects.Any(a => a.def == aspect);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref _aspects, "affinities", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_aspects == null) _aspects = new List<Aspect>(); 
                foreach (Aspect affinity in _aspects)
                    affinity.Initialize();

            }
        }

        public void Remove(Aspect aspect)
        {
            _rmCache.Add(aspect); // Don't remove them quite yet.
        }

        /// <summary> Removes the aspect with the given def from the pawn. </summary>
        public void Remove(AspectDef def)
        {
            Aspect af = _aspects.FirstOrDefault(a => a.def == def);
            if (af != null) _rmCache.Add(af);
        }

        /// <summary> Handle affinities that need to be removed or added after a pawn changes race. </summary>
        private void HandleMorphChangeAffinities([CanBeNull] MorphDef lastMorph, [CanBeNull] MorphDef curMorph)
        {
            bool ShouldRemove(MorphDef.AddedAspect a)
            {
                if (!Contains(a.def)) return false;
                if (a.keepOnReversion) return false;
                if (curMorph?.addedAspects.Any(aa => aa.def == a.def) == true)
                    return false; //if the morph the pawn turned into adds the same aspect don't remove it 
                return true;
            }

            if (lastMorph != null)
            {
                IEnumerable<Aspect> rmAffinities = lastMorph.addedAspects.Where(ShouldRemove)
                                                              .Select(a => GetAspect(a.def)); //select the instances from the _affinity list  
                _rmCache.AddRange(rmAffinities.Where(a => a != null)); 
            }

            if (curMorph != null)
            {
                IEnumerable<AspectDef> addAf = curMorph.addedAspects.Where(a => !Contains(a.def)).Select(a => a.def);
                foreach (AspectDef affinityDef in addAf) Add(affinityDef);
            }
        }

        /// <summary> Get the aspect in this tracker of the given def, if one exists. </summary>
        [CanBeNull]
        public Aspect GetAspect(AspectDef aspectDef)
        {
            return _aspects.FirstOrDefault(d => d.def == aspectDef); 
        }

        /// <summary> Returns an enumerator that iterates through the collection. </summary>
        /// <returns> A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection. </returns>
        public IEnumerator<Aspect> GetEnumerator()
        {
            return _aspects.GetEnumerator();
        }

        /// <summary> Returns an enumerator that iterates through a collection. </summary>
        /// <returns> An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection. </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _aspects).GetEnumerator();
        }
    }
}
