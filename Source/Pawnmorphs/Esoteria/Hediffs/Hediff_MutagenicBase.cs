using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;
using Pawnmorph.Hediffs.Utility;
using Pawnmorph.Utilities;
using Pawnmorph.Utilities.Collections;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Abstract base class for all hediffs that cause mutations and transformation
    /// </summary>
    /// <seealso cref="Verse.Hediff" />
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_Descriptive" />
    public abstract class Hediff_MutagenicBase : Hediff_Descriptive
    {
        // Used to track what kind of stage we're in, so we don't have to check
        // every tick
        enum StageType
        {
            None,
            Mutation,
            Transformation
        }

        // Cache the stage index and stage, because CurStage/CurStageIndex both
        // calculate it every time they're called and it can get expensive
        private int cachedStageIndex = -1;
        [Unsaved] private HediffStage cachedStage;
        [Unsaved] private StageType cachedStageType;

        // The number of queued up mutations to add over the next few ticks
        private int queuedMutations;

        // A utility class to handle iterating over both body parts and mutations
        private BodyMutationManager bodyMutationManager = new BodyMutationManager();

        // Used to force-remove the hediff
        private bool forceRemove;

        [Unsaved] private List<ITfHediffObserverComp> observerCompsCache;

        /// <summary>
        /// All observer comps to notify when adding mutations and visiting parts to add mutations onto.
        /// </summary>
        /// <value>
        /// The observer comps.
        /// </value>
        [NotNull]
        protected IReadOnlyList<ITfHediffObserverComp> ObserverComps
        {
            get
            {
                if (observerCompsCache == null)
                    observerCompsCache = comps.MakeSafe().OfType<ITfHediffObserverComp>().ToList();

                return observerCompsCache;
            }
        }

        /// <summary>
        /// Called after this hediff is added to the pawn
        /// </summary>
        /// <param name="dinfo">The damage info.</param>
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            // If we somehow got a pawn that can't be mutated just remove the hediff
            // This is because AndroidTiers was giving android mutations because reasons
            if (!def.GetMutagenDef().CanInfect(pawn))
                MarkForRemoval();
        }

        /// <summary>
        /// Called when afte the hediff is removed.
        /// </summary>
        public override void PostRemoved()
        {
            base.PostRemoved();
            pawn.CheckRace();
        }

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            // Handle stage transitions
            if (CurStageIndex != cachedStageIndex)
            {
                OnStageChanged();

                // Only try to transform the pawn when entering a transformation stage
                // NOTE: This triggers regardless of whether the stages are increasing or decreasing.
                // TODO immunity
                if (cachedStageType == StageType.Transformation)
                    CheckAndDoTransformation();
            }

            if (cachedStageType == StageType.Mutation && pawn.IsHashIntervalTick(60))
                CheckAndAddMutations();
        }

        /// <summary>
        /// Checks if we should add mutations, and if so does
        /// Mutations are queued up and added one at a time to smooth out mutation rate when there are
        /// large spikes (e.g. severity-based MutationRates)
        /// </summary>
        protected virtual void CheckAndAddMutations()
        {
            if (!(cachedStage is HediffStage_Mutation stage))
            {
                Log.Error($"Hediff {def.defName} tried to mutate {pawn.Name} but stage {cachedStageIndex} ({cachedStage.label}) is not a mutation stage");
                return;
            }

            // MutationRates can request multiple muations be added at once,
            // but we'll queue them up so they only happen once a second
            //TODO mutagen sensitivity
            QueueUpMutations(stage.MutationRate.GetMutationsPerSecond(this));

            // Add a queued mutation, if any are waiting
            if (queuedMutations > 0)
            {

                //TODO
                queuedMutations--;
            }
        }

        /// <summary>
        /// Checks if we should transform the pawn, and if so does
        /// </summary>
        protected virtual void CheckAndDoTransformation()
        {
            if (!(cachedStage is HediffStage_Transformation stage))
            {
                Log.Error($"Hediff {def.defName} tried to transform {pawn.Name} but stage {cachedStageIndex} ({cachedStage.label}) is not a transformation stage");
                return;
            }

            if (stage.TFChance.ShouldTransform(this))
            {
                //TODO
            }
        }

        /// <summary>
        /// Updates the cached stage values
        /// </summary>
        private void OnStageChanged()
        {
            var oldStage = cachedStage;

            cachedStageIndex = CurStageIndex;
            cachedStage = def?.stages?[cachedStageIndex];

            if (cachedStage is HediffStage_Mutation newMutStage)
            {
                cachedStageType = StageType.Mutation;

                // Reset the spread manager and mutation cache, but only if the
                // ones in the new stage are different
                if (oldStage is HediffStage_Mutation oldMutStage)
                {
                    if (!newMutStage.SpreadOrder.EquivalentTo(oldMutStage.SpreadOrder))
                        ResetSpreadList();
                    if (!newMutStage.MutationTypes.EquivalentTo(oldMutStage.MutationTypes))
                        ResetMutationList();
                }
                else
                {
                    ResetSpreadList();
                    ResetMutationList();
                }
            }
            else if (cachedStage is HediffStage_Transformation)
            {
                cachedStageType = StageType.Transformation;
            }
            else
            {
                cachedStageType = StageType.None;
            }

            foreach (var comp in ObserverComps)
                comp.StageChanged();
        }

        /// <summary>
        /// Queues up a number of mutations to be added to the pawn.  Negative amounts
        /// can cancel out queued up mutations but won't remove already-existing mutations.
        /// </summary>
        /// <param name="mutations">Mutations.</param>
        protected void QueueUpMutations(int mutations)
        {
            queuedMutations += mutations;
            // Negative mutation counts can cancel already-queued mutations but should never go below 0
            queuedMutations = Math.Max(queuedMutations, 0);
        }

        /// <summary>
        /// Resets the spread list because something caused the current one to be invalid.
        /// Call this when SpreadOrder changes (usually due to a stage change).
        /// </summary>
        protected void ResetSpreadList()
        {
            if (cachedStage is HediffStage_Mutation mutStage)
            {
                var spreadList = mutStage.SpreadOrder.GetSpreadList(this);
                bodyMutationManager.ResetSpreadList(spreadList);

                // Let the observers know we've reset our spreading
                foreach (var comp in ObserverComps)
                    comp.Init();
            }
        }

        /// <summary>
        /// Resets the mutation list because something caused the current one to be invalid.
        /// Call this when MutationTypes changes, or something it relies on does.
        /// </summary>
        protected void ResetMutationList()
        {
            if (cachedStage is HediffStage_Mutation mutStage)
            {
                var mutations = mutStage.MutationTypes.GetMutations(this);
                bodyMutationManager.ResetMutationList(mutations);

                // Let the observers know we've reset our mutation types
                foreach (var comp in ObserverComps)
                    comp.Init();
            }
        }

        /// <summary>
        /// The severity of this hediff 
        /// </summary>
        /// <value>The severity.</value>
        public override float Severity
        {
            get { return base.Severity; }
            set
            {
                // Severity changes can potential queue up mutations
                if (cachedStage is HediffStage_Mutation mutStage)
                {
                    float diff = value - base.Severity;
                    int mutations = mutStage.MutationRate.GetMutationsPerSeverity(this, diff);
                    QueueUpMutations(mutations);
                }
                base.Severity = value;
            }
        }

        /// <summary>
        /// Controls the severity label that gets rendered in the health menu
        /// </summary>
        /// <value>The severity label.</value>
        public override string SeverityLabel
        {
            get
            {
                if (base.SeverityLabel != null)
                    return base.SeverityLabel;

                // Render based on max severity if there's no lethal severity, since
                // many mutagenic hediffs cause a full TF on max severity
                if (def.maxSeverity > 0f)
                    return (Severity / def.maxSeverity).ToStringPercent();

                return null;
            }
        }

        /// <summary>
        /// Controls whether or not this hediff gets removed 
        /// </summary>
        public override bool ShouldRemove => forceRemove || base.ShouldRemove;

        /// <summary>
        /// Marks this hediff removal.
        /// </summary>
        /// this is needed because Rimworld is touchy about removing hediffs. best to not do it manually and call this,
        /// the HediffTracker will then remove this hediff next tick once all hediffs are no longer running any code 
        public void MarkForRemoval()
        {
            forceRemove = true;
        }

        /// <summary>
        /// Exposes data to be saved/loaded from XML upon saving the game
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cachedStageIndex, nameof(cachedStageIndex), -1);
            Scribe_Values.Look(ref forceRemove, nameof(forceRemove));
            Scribe_Values.Look(ref queuedMutations, nameof(queuedMutations));
            Scribe_Deep.Look(ref bodyMutationManager, nameof(bodyMutationManager));

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                OnStageChanged();
            }
        }
    }
}