using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Full transformation stage that causes a transformation into an animal species decided by the hediff
    /// Must be used with Hediff_DynamicTf
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.Hediff_DynamicTf" />
    public class HediffStage_Transformation_Dynamic : HediffStage, IExecutableStage, IInitializable
    {
        /// <summary>
        /// The chance of fully transforming
        /// </summary>
        public float transformationChance = 1f;

        /// <summary>
        /// Called when the given hediff enters this stage
        /// </summary>
        /// <param name="hediff">The hediff.</param>
        public void EnteredStage(Hediff hediff)
        {
            if (Rand.Value > transformationChance) return;

            if (!(hediff is Hediff_DynamicTf tfHediff))
            {
                Log.Error($"Hediff {hediff.def.defName} has a HediffStage_Transformation_Dynamic but is not a Hediff_DynamicTF!");
                return;
            }

            var transformer = tfHediff.MorphDef?.fullTransformation?.GetAllTransformers().FirstOrDefault();
            if (transformer == null)
            {
                Log.Error($"could not find transformer for morph {tfHediff.MorphDef?.defName ?? "NULL"} with full tf {tfHediff.MorphDef?.fullTransformation?.defName ?? "NULL"}");
                return;
            }
            transformer.TransformPawn(hediff.pawn, hediff);
        }

        /// <summary>
        /// Gets all Configuration errors in this instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ConfigErrors()
        {
            return Enumerable.Empty<string>();
        }
    }
}