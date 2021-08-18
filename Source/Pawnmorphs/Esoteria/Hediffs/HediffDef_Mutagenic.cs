using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// HediffDef for hediffs that cause mutations and transformation
    /// </summary>
    /// <seealso cref="Verse.HediffDef" />
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_MutagenicBase" />
    public class HediffDef_Mutagenic : HediffDef
    {
        [UsedImplicitly] private MutSpreadOrder spreadOrder;
        [UsedImplicitly] private MutRate mutationRate;
        [UsedImplicitly] private MutTypes mutationTypes;
        [UsedImplicitly] private TFTypes transformationTypes;
        [UsedImplicitly] private TFGenderSettings transformationGenderSettings;
        [UsedImplicitly] private TFMiscSettings transformationSettings;


        /// <summary>
        /// Controls the order that mutations spread over the body
        /// </summary>
        /// <param name="stageIndex">The index of the stage to get</param>
        public MutSpreadOrder SpreadOrder(int stageIndex)
        {
            var stage = stages[stageIndex];
            var val = (stage as HediffStage_Mutation)?.SpreadOrder ?? spreadOrder;
            if (val == null) Log.Error($"{defName} has no defined spreadOrder for stage {stageIndex} ({stage.label})!");
            return val;
        }

        /// <summary>
        /// Controls how fast mutations are added
        /// </summary>
        /// <param name="stageIndex">The index of the stage to get</param>
        public MutRate MutationRate(int stageIndex)
        {
            var stage = stages[stageIndex];
            var val = (stage as HediffStage_Mutation)?.MutationRate ?? mutationRate;
            if (val == null) Log.Error($"{defName} has no defined mutationRate for stage {stageIndex} ({stage.label})!");
            return val;
        }

        /// <summary>
        /// Controls what kinds of mutations can be added
        /// </summary>
        /// <value>The mutation types.</value>
        public MutTypes MutationTypes(int stageIndex)
        {
            var stage = stages[stageIndex];
            var val = (stage as HediffStage_Mutation)?.MutationTypes ?? mutationTypes;
            if (val == null) Log.Error($"{defName} has no defined mutationTypes for stage {stageIndex} ({stage.label})!");
            return val;
        }

        /// <summary>
        /// Controls what kind of animals transformations can result in
        /// </summary>
        /// <value>The TF types.</value>
        public TFTypes TFTypes(int stageIndex)
        {
            var stage = stages[stageIndex];
            var val = (stage as HediffStage_Transformation)?.TFTypes ?? transformationTypes;
            if (val == null) Log.Error($"{defName} has no defined transformationTypes for stage {stageIndex} ({stage.label})!");
            return val;
        }

        /// <summary>
        /// Controls the gender of the post-transformation pawn
        /// </summary>
        /// <value>The TF gender settings.</value>
        public TFGenderSettings TFGenderSettings(int stageIndex)
        {
            var stage = stages[stageIndex];
            var val = (stage as HediffStage_Transformation)?.TFGenderSettings ?? transformationGenderSettings;
            if (val == null) Log.Error($"{defName} has no defined transformationGenderSettings for stage {stageIndex} ({stage.label})!");
            return val;
        }

        /// <summary>
        /// Controls miscellaneous settings related to full transformations
        /// </summary>
        /// <value>The TF misc settings.</value>
        public TFMiscSettings TFMiscSettings(int stageIndex)
        {
            var stage = stages[stageIndex];
            var val = (stage as HediffStage_Transformation)?.TFMiscSettings ?? transformationSettings;
            if (val == null) Log.Error($"{defName} has no defined transformationSettings for stage {stageIndex} ({stage.label})!");
            return val;
        }
    }
}