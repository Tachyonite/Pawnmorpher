using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// An abstracty class for hediffs that need to do things on stage changes.
    /// Also implements the IDescriptiveHediff interface
    /// </summary>
    public abstract class Hediff_StageChanges : Hediff_Descriptive
    {
        // Cache the stage index and stage, because CurStage/CurStageIndex both
        // calculate it every time they're called and it can get expensive
        private int cachedStageIndex = -1;
        [Unsaved] private HediffStage cachedStage;

        private List<IStageChangeObserverComp> observerComps;
        private IEnumerable<IStageChangeObserverComp> ObserverComps
        {
            get
            {
                if (observerComps == null)
                    observerComps = comps.MakeSafe().OfType<IStageChangeObserverComp>().ToList();
                return observerComps;
            }
        }

        // CurStageIndex is kind of expensive to calculate, so use the cache when possible
        public override int CurStageIndex => cachedStageIndex;
        public override HediffStage CurStage => cachedStage;

        /// <summary>
        /// Called after the hediff is created, but before it's added to a pawn
        /// </summary>
        public override void PostMake()
        {
            base.PostMake();
            RecacheStage(base.CurStageIndex);
        }

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            int stageIndex = base.CurStageIndex; // Make sure to get the actual index from the base
            if (stageIndex != cachedStageIndex)
            {
                var oldStage = cachedStage;
                RecacheStage(stageIndex);

                OnStageChanged(oldStage, cachedStage);

                foreach (var comp in ObserverComps)
                    comp.OnStageChanged(oldStage, cachedStage);
            }
        }

        /// <summary>
        /// Reloads the stage cache
        /// </summary>
        /// <param name="stageIndex">Stage index.</param>
        private void RecacheStage(int stageIndex)
        {
            cachedStageIndex = stageIndex;
            cachedStage = def?.stages?[cachedStageIndex];
        }

        /// <summary>
        /// Called when the stage changes
        /// </summary>
        protected abstract void OnStageChanged(HediffStage oldStage, HediffStage newStage);

        /// <summary>
        /// Exposes data to be saved/loaded from XML upon saving the game
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cachedStageIndex, nameof(cachedStageIndex), base.CurStageIndex);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                RecacheStage(cachedStageIndex);
            }
        }
    }
}
