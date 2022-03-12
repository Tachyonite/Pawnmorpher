using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     hediff representing a mutation
    /// </summary>
    /// <seealso cref="Verse.HediffWithComps" />
    public class Hediff_AddedMutation : Hediff_StageChanges
    {
        /// <summary>
        ///     The mutation description
        /// </summary>
        public string mutationDescription;

        /// <summary>
        ///     if this part should be removed or not
        /// </summary>
        protected bool shouldRemove;

        [NotNull] private readonly Dictionary<int, string> _descCache = new Dictionary<int, string>();

        private MutationDef _mDef;

        [NotNull] private MutationCauses _causes = new MutationCauses();

        private Comp_MutationSeverityAdjust _sevAdjComp;

        private bool _waitingForUpdate;

        private int _currentStageIndex = -1;


        /// <summary>
        ///     Gets the definition.
        /// </summary>
        /// <value>
        ///     The definition.
        /// </value>
        [NotNull]
        public MutationDef Def
        {
            get
            {
                if (_mDef == null)
                    try
                    {
                        _mDef = (MutationDef) def;
                    }
                    catch (InvalidCastException e)
                    {
                        Log.Error($"cannot convert {def.GetType().Name} to {nameof(MutationDef)}!\n{e}");
                    }

                return _mDef;
            }
        }

        /// <summary>
        ///     Gets the current mutation stage. null if the hediff has no stages or the current stage is not a mutation stage
        /// </summary>
        /// <value>
        ///     The current mutation stage.
        /// </value>
        [CanBeNull]
        public MutationStage CurrentMutationStage
        {
            get
            {
                if (Def.stages.NullOrEmpty()) return null;
                return Def.CachedMutationStages[CurStageIndex];
            }
        }

        /// <summary>
        ///     Gets the influence this mutation confers
        /// </summary>
        /// <value>
        ///     The influence.
        /// </value>
        [NotNull]
        public List<AnimalClassBase> Influence
        {
            get
            {
                if (def is MutationDef mDef)
                    return mDef.ClassInfluences;

                Log.Warning($"{def.defName} is a mutation but does not use {nameof(MutationDef)}! this will cause problems!");
                return new List<AnimalClassBase>() { AnimalClassDefOf.Animal };
            }
        }


        /// <summary>
        ///     Gets the base label .
        /// </summary>
        /// <value>
        ///     The base label .
        /// </value>
        public override string LabelBase
        {
            get
            {
                var label = base.LabelBase;

                if (SeverityAdjust?.Halted == true) label += " (halted)";

                return label;
            }
        }

        public override string DebugString()
        {
            string debugString = base.DebugString();

            HediffComp_Production productionComp = this.TryGetComp<HediffComp_Production>();
            if (productionComp != null)
            {
                debugString += Environment.NewLine;
                debugString += "Production Component: " + Environment.NewLine;
                debugString += productionComp.ToStringFull();
            }

            return debugString;
        }

        /// <summary>
        ///     Gets the causes of this mutation
        /// </summary>
        /// <value>
        ///     The causes.
        /// </value>
        public override string Description // TODO - the extra features here might be better off in Hediff_Descriptive
        {
            get
            {
                string desc;
                if (!_descCache.TryGetValue(CurStageIndex, out desc))
                {
                    StringBuilder builder = new StringBuilder();
                    CreateDescription(builder);
                    desc = builder.ToString();
                    _descCache[CurStageIndex] = desc;
                }

                return desc;
            }
        }

        [NotNull]
        public MutationCauses Causes => _causes;


        /// <summary>
        ///     Gets a value indicating whether should be removed.
        /// </summary>
        /// <value><c>true</c> if should be removed; otherwise, <c>false</c>.</value>
        public override bool ShouldRemove
        {
            get
            {
                foreach (HediffComp hediffComp in comps.MakeSafe())
                    if (hediffComp.CompShouldRemove)
                        return true;

                return shouldRemove;
            }
        }

        /// <summary>Gets the extra tip string .</summary>
        /// <value>The extra tip string .</value>
        public override string TipStringExtra
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(base.TipStringExtra);
                stringBuilder.AppendLine("Efficiency".Translate() + ": " + def.addedPartProps.partEfficiency.ToStringPercent());
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is a core mutation.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is a core mutation; otherwise, <c>false</c>.
        /// </value>
        public bool IsCoreMutation => this.TryGetComp<RemoveFromPartComp>()?.Layer == MutationLayer.Core;

        /// <summary>
        ///     Gets the severity adjust comp
        /// </summary>
        /// <value>
        ///     The severity adjust comp
        /// </value>
        [CanBeNull]
        public Comp_MutationSeverityAdjust SeverityAdjust
        {
            get
            {
                if (_sevAdjComp == null) _sevAdjComp = this.TryGetComp<Comp_MutationSeverityAdjust>();

                return _sevAdjComp;
            }
        }


        /// <summary>
        ///     Gets or sets a value indicating whether progression is halted or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if progression halted; otherwise, <c>false</c>.
        /// </value>
        public bool ProgressionHalted => SeverityAdjust?.Halted == true;



        /// <summary>
        /// Called when the hediff stage changes.
        /// </summary>
        protected override void OnStageChanged(HediffStage oldStage, HediffStage newStage)
        {
            if (newStage is MutationStage mStage)
            {
                //check for aspect skips 
                if (mStage.SkipAspects.Any(e => e.Satisfied(pawn)))
                {
                    SkipStage();
                    return;
                }
            }
        }

        ///     checks if this mutation blocks the addition of a new mutation at the given part
        /// </summary>
        /// <param name="otherMutation">The other mutation.</param>
        /// <param name="addPart">The add part.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">otherMutation</exception>
        public virtual bool Blocks([NotNull] MutationDef otherMutation, [CanBeNull] BodyPartRecord addPart)
        {
            if (otherMutation == null) throw new ArgumentNullException(nameof(otherMutation));
            var mDef = def as MutationDef;
            return mDef?.BlocksMutation(otherMutation, Part, addPart) == true;
        }

        /// <summary>Creates the description.</summary>
        /// <param name="builder">The builder.</param>
        public virtual void CreateDescription(StringBuilder builder)
        {
            string rawDescription = GetRawDescription();
            if (rawDescription == null)
            {
                builder.AppendLine("PawnmorphTooltipNoDescription".Translate());
                return;
            }

            string res = rawDescription.AdjustedFor(pawn);
            builder.AppendLine(res);
        }

        /// <summary>Exposes the data.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _currentStageIndex, nameof(_currentStageIndex), -1);
            Scribe_Values.Look(ref shouldRemove, nameof(shouldRemove));

            Scribe_Deep.Look(ref _causes, "causes");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Part == null)
            {
                Log.Error($"Hediff_AddedPart [{def.defName},{Label}] has null part after loading.");
                pawn.health.hediffSet.hediffs.Remove(this);
            }
        }

        /// <summary>
        ///     Marks this mutation for removal.
        /// </summary>
        public void MarkForRemoval()
        {
            shouldRemove = true;
        }

        /// <summary>called after this instance is added to the pawn.</summary>
        /// <param name="dinfo">The dinfo.</param>
        public override void PostAdd(DamageInfo? dinfo)
            // After the hediff has been applied.
        {
            base.PostAdd(dinfo); // Do the inherited method.
            if (PawnGenerator.IsBeingGenerated(pawn) || !pawn.Spawned
            ) //if the pawn is still being generated do not update graphics until it's done 
            {
                _waitingForUpdate = true;
                return;
            }

            UpdatePawnInfo();

            foreach (Hediff_AddedMutation otherMutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
                try
                {
                    if (Blocks((MutationDef) otherMutation.def, otherMutation.Part))
                        otherMutation.shouldRemove = true; //don't actually remove the hediffs, just mark them for removal
                }
                catch (InvalidCastException e) //just pretty up the error message a bit and continue on 
                {
                    Log.Error($"could not cast {otherMutation.def.defName} of type {otherMutation.def.GetType().Name} to {nameof(MutationDef)}!\n{e}");
                }
        }

        /// <summary>called after this instance is removed from the pawn</summary>
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!PawnGenerator.IsBeingGenerated(pawn))
                pawn.GetMutationTracker()?.NotifyMutationRemoved(this);
        }

        /// <summary>
        ///     Posts the tick.
        /// </summary>
        public override void PostTick()
        {
            base.PostTick();
            if (_waitingForUpdate)
            {
                UpdatePawnInfo();
                _waitingForUpdate = false;
            }
        }

        /// <summary>
        ///     Restarts the adaption progression for this mutation if halted, does nothing if the part is fully adapted or not
        ///     halted
        /// </summary>
        public void ResumeAdaption()
        {
            SeverityAdjust?.Restart();
        }

        /// <summary>
        ///     called every tick
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (_currentStageIndex != CurStageIndex)
            {
                _currentStageIndex = CurStageIndex;
                OnStageChanges();
            }
        }

        /// <summary>
        ///     Called when the hediff stage changes.
        /// </summary>
        protected virtual void OnStageChanges()
        {
            if (CurStage is MutationStage mStage)
                //check for aspect skips 
                if (mStage.SkipAspects.Any(e => e.Satisfied(pawn)))
                {
                    SkipStage();
                    return;
                }


            if (CurStage is IExecutableStage exeStage) exeStage.EnteredStage(this);
        }

        private string GetRawDescription()
        {
            string descOverride = (CurStage as IDescriptiveStage)?.DescriptionOverride;
            return string.IsNullOrEmpty(descOverride) ? def.description : descOverride;
        }

        private void SkipStage()
        {
            //make sure to skip in the correct direction 
            float severityAdj = SeverityAdjust?.ChangePerDay ?? 0;

            int nextIndex;

            if (severityAdj < 0) nextIndex = Mathf.Max(0, CurStageIndex - 1);
            else nextIndex = Mathf.Min(def.stages.Count - 1, CurStageIndex + 1);

            if (nextIndex == CurStageIndex) return; //don't skip as there's no stage to skip to 
            HediffStage nextStage = def.stages[nextIndex];

            Severity = nextStage.minSeverity;
        }

        private void UpdatePawnInfo()
        {
            pawn.GetMutationTracker()?.NotifyMutationAdded(this);
            var gUpdater = pawn.GetComp<GraphicsUpdaterComp>();
            if (gUpdater != null)
            {
                //try and defer graphics update to the next tick so we don't lag when adding a bunch of mutations at once 
                gUpdater.IsDirty = true;
                return;
            }

            if (Current.ProgramState == ProgramState.Playing
             && MutationUtilities.AllMutationsWithGraphics.Contains(def)
             && pawn.IsColonist)
            {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(pawn);
            }
        }
    }
}