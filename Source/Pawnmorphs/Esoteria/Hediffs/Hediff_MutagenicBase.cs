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
        // calculate it every time they're called
        private int cachedStageIndex = -1;
        [Unsaved] private HediffStage cachedStage;
        [Unsaved] private StageType cachedStageType;

        private bool forceRemove;

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            int stageIndex = CurStageIndex;
            if (stageIndex != cachedStageIndex)
            {
                cachedStageIndex = stageIndex;
                cachedStage = def?.stages?[stageIndex];

                // Only try to transform the pawn when entering a transformation stage
                // NOTE: This triggers regardless of whether the stages are increasing or decreasing.
                // Keep that in mind when adding a transformation stage to a hediff that can do both.
                // TODO immunity
                if (cachedStageType == StageType.Transformation)
                    CheckAndDoTransformation();
            }

            if (cachedStageType == StageType.Mutation && pawn.IsHashIntervalTick(60))
                CheckAndAddMutations();
        }

        /// <summary>
        /// Checks if we should add mutations, and if so does
        /// </summary>
        protected virtual void CheckAndAddMutations()
        {
            //TODO
        }

        /// <summary>
        /// Checks if we should transform the pawn, and if so does
        /// </summary>
        protected virtual void CheckAndDoTransformation()
        {
            //TODO
        }

        /// <summary>
        /// Updates the cached stage values
        /// </summary>
        private void UpdateCachedStage()
        {
            cachedStageIndex = CurStageIndex;
            cachedStage = def?.stages?[cachedStageIndex];

            if (cachedStage is HediffStage_Mutation)
                cachedStageType = StageType.Mutation;
            else if (cachedStage is HediffStage_Transformation)
                cachedStageType = StageType.Transformation;
            else
                cachedStageType = StageType.None;
        }

        /// <summary>
        /// How much the severity of this hediff is changing per day (used for certain components)
        /// This is somewhat expensive to calculate, so call sparingly.
        /// </summary>
        /// <value>The severity label.</value>
        public float SeverityChangePerDay
        {
            get
            {
                float gainRate = 0f;
                gainRate += this.TryGetComp<Comp_ImmunizableMutation>()?.SeverityChangePerDay() ?? 0f;
                gainRate += this.TryGetComp<HediffComp_Immunizable>()?.SeverityChangePerDay() ?? 0f;
                gainRate += this.TryGetComp<HediffComp_SeverityPerDay>()?.SeverityChangePerDay() ?? 0f;
                gainRate += this.TryGetComp<HediffComp_TendDuration>()?.SeverityChangePerDay() ?? 0f;
                return gainRate;
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

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                UpdateCachedStage();
            }
        }
    }
}