// Aspect.cs created by Iron Wolf for Pawnmorph on 09/23/2019 8:07 AM
// last updated 09/23/2019  12:39 PM

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     base class for all "mutation affinities"
    /// </summary>
    /// affinities are things that are more global than hediffs but more temporary than traits
    public class Aspect : IExposable
    {
        public AspectDef def;

        private int _stage;

        private Pawn _pawn;

        private bool _shouldRemove;
        private bool _wasStarted;

        protected List<AspectStage> Stages => def.stages;


        public IEnumerable<PawnCapacityModifier> CapMods => CurrentStage.capMods ?? Enumerable.Empty<PawnCapacityModifier>();

        public bool HasCapMods => CurrentStage.capMods != null && CurrentStage.capMods.Count != 0;  

        /// <summary>
        ///     the current stage index
        /// </summary>
        public int StageIndex
        {
            get => _stage;
            set
            {
                var st = Mathf.Clamp(value, 0, Stages.Count - 1);
                if (_stage != st)
                {
                    _stage = st;
                    StageChanged(_stage);
                }
            }
        }

        /// <summary>
        ///     the current stage
        /// </summary>
        public AspectStage CurrentStage => Stages[StageIndex];


        public string Label
        {
            get
            {
                var lBase = string.IsNullOrEmpty(CurrentStage.label) ? def.label : CurrentStage.label;
                if (!string.IsNullOrEmpty(CurrentStage.modifier)) lBase = $"{lBase} ({CurrentStage.modifier})";

                return lBase;
            }
        }

        /// <summary>
        ///     the description of the aspect, taking into account it's current stage
        /// </summary>
        public string Description =>
            string.IsNullOrEmpty(CurrentStage.description) ? def.description : CurrentStage.description;

        /// <summary>
        ///     the pawn this is attached to
        /// </summary>
        public Pawn Pawn => _pawn;

        /// <summary>
        ///     if this affinity should be removed or not
        /// </summary>
        public bool ShouldRemove
        {
            get => _shouldRemove;
            protected set => _shouldRemove = value;
        }


        /// <summary>
        ///     called after this affinity is added to the pawn
        /// </summary>
        /// <param name="pawn"></param>
        public void Added(Pawn pawn)
        {
            _pawn = pawn;
            if (!_wasStarted)
            {
                _wasStarted = true;
                Start();
            }

            PostAdd();
        }

        /// <summary>
        ///     called during startup to initialize all affinities
        /// </summary>
        public void Initialize()
        {
            PostInit();
            if (!_wasStarted)
            {
                _wasStarted = true;
                Start();
            }
        }


        /// <summary>
        ///     called after the pawn is despawned
        /// </summary>
        public virtual void PostDeSpawn()
        {
        }

        /// <summary>
        ///     called when the pawn's race changes
        /// </summary>
        /// <param name="oldRace"></param>
        public virtual void PostRaceChange(ThingDef oldRace)
        {
        }

        /// <summary>
        ///     called after this affinity is removed from the pawn
        /// </summary>
        public virtual void PostRemove()
        {
        }

        /// <summary>
        ///     called after the pawn is spawned
        /// </summary>
        /// <param name="respawningAfterLoad"></param>
        public virtual void PostSpawnSetup(bool respawningAfterLoad)
        {
        }

        /// <summary>
        ///     called every tick
        /// </summary>
        public virtual void PostTick()
        {
        }

        /// <summary>
        ///     call to set ShouldRemove to true
        /// </summary>
        public void StageToRemove()
        {
            ShouldRemove = true;
        }

        /// <summary>
        ///     called During IExposable's ExposeData to serialize data
        /// </summary>
        protected virtual void ExposeData() //want this hidden from the public interface of the class 
        {
        }

        /// <summary>
        ///     called after this instance is added to the pawn
        /// </summary>
        protected virtual void PostAdd()
        {
        }


        /// <summary>
        ///     called after the base instance is initialize
        /// </summary>
        protected virtual void PostInit()
        {
        }

        protected virtual void PostStageChanged(int lastStage)
        {
        }

        /// <summary>
        ///     called once during the startup of this instance, either after initialization or after being added to the pawn
        /// </summary>
        protected virtual void Start()
        {
        }

        private void StageChanged(int lastStage)
        {
            PostStageChanged(lastStage);
            //TODO
        }

        void IExposable.ExposeData()
        {
            Scribe_References.Look(ref _pawn, nameof(Pawn));
            Scribe_Values.Look(ref _shouldRemove, nameof(ShouldRemove));
            Scribe_Defs.Look(ref def, nameof(def));
            Scribe_Values.Look(ref _stage, nameof(StageIndex));
            ExposeData();
        }
    }
}