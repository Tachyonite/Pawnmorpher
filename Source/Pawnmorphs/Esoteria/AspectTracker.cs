// AspectTracker.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:25 AM
// last updated 09/22/2019  11:25 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
    /// <summary>
    ///     thing comp for tracking 'mutation aspects'
    /// </summary>
    public class AspectTracker : ThingComp, IMutationEventReceiver, IRaceChangeEventReceiver
    {
        private readonly List<Aspect> _rmCache = new List<Aspect>();
        private List<Aspect> _aspects = new List<Aspect>();

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


        public IEnumerable<Aspect> Aspects => _aspects;

        private Pawn Pawn => (Pawn) parent;

        public void Add([NotNull] Aspect aspect)
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
            aspect.Added(Pawn);
        }

        public void Add([NotNull] AspectDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));

            if (_aspects.Count(a => a.def == def) > 0)
            {
                Log.Warning($"trying to add an aspect with def {def.defName} to pawn {Pawn.Name} which already has an aspect of that def");
                return;
            }

            Add(def.CreateInstance());
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

        /// <summary>
        ///     get the first aspect with the given def
        /// </summary>
        /// <param name="def"></param>
        /// <returns>the aspect with the given def, null if one isn't found </returns>
        [CanBeNull]
        public Aspect GetAffinityOfDef(AspectDef def)
        {
            return _aspects.FirstOrDefault(a => a.def == def);
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
            _rmCache.Add(aspect); //don't remove them quite yet '
        }

        /// <summary>
        ///     removes the aspect with the given def from the pawn
        /// </summary>
        /// <param name="def"></param>
        public void Remove(AspectDef def)
        {
            Aspect af = _aspects.FirstOrDefault(a => a.def == def);
            if (af != null) _rmCache.Add(af);
        }

        /// <summary>
        /// handle affinities that need to be removed or added after a pawn changes race 
        /// </summary>
        /// <param name="lastMorph"></param>
        /// <param name="curMorph"></param>
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
                                                              .Select(a => GetAffinityOfDef(a.def)); //select the instances from the _affinity list  
                _rmCache.AddRange(rmAffinities);
            }

            if (curMorph != null)
            {
                IEnumerable<AspectDef> addAf = curMorph.addedAspects.Where(a => !Contains(a.def)).Select(a => a.def);
                foreach (AspectDef affinityDef in addAf) Add(affinityDef);
            }
        }

        /// <summary>
        /// get the aspect in this tracker of the given def, if one exists 
        /// </summary>
        /// <param name="aspectDef"></param>
        /// <returns></returns>
        [CanBeNull]
        public Aspect GetAspect(AspectDef aspectDef)
        {
            return _aspects.FirstOrDefault(d => d.def == aspectDef); 
        }
    }
}