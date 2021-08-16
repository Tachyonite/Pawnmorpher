using System.Linq;
using Pawnmorph.TfSys;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A TF hediff type where the morph/animal type can be changed dynamically
    /// </summary>
    /// <seealso cref="Verse.Hediff" />
    public class Hediff_DynamicTf : MorphTf
    {
        private MorphDef morphDef;

        /// <summary>
        /// The morph def associated with this hediff, which controls the mutations
        /// and transformation it can cause.
        /// </summary>
        /// <value>
        /// The MorphDef
        /// </value>
        public MorphDef MorphDef => morphDef;

        public override bool ShouldRemove => MutationStatValue <= 0;

        /// <summary>
        /// Initializes this instance to use the given morphDef
        /// </summary>
        /// <param name="morphDef">The morphDef to use.</param>
        public void Initialize(MorphDef morphDef)
        {
            this.morphDef = morphDef;
            ResetMutationCaches();
        }

        /// <summary>
        ///     Called when the stage changes.
        /// </summary>
        /// <param name="currentStage">The last stage.</param>
        protected override void OnStageChanged(HediffStage currentStage)
        {
            base.OnStageChanged(currentStage);
            if (currentStage == def.stages[def.stages.Count - 1]) DoTf();
        }

        private void DoTf()
        {
            if (kind == null)
            {
                _chosenKind = DefDatabase<PawnKindDef>.AllDefs.Where(p => p.RaceProps.Animal).RandomElement();
            }
            var tfRequest = new TransformationRequest(_chosenKind, pawn);
            var res = MutagenDefOf.defaultMutagen.MutagenCached.Transform(tfRequest);
            if (res != null)
            {
                Find.World.GetComponent<PawnmorphGameComp>().AddTransformedPawn(res); 
            }
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref morphDef, "chosenKind");
            base.ExposeData();
        }
    }
}