using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Mutation stage that causes a transformation into an animal species decided by the hediff
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.TransformationStageBase" />
    public class HediffStage_Mutation_Dynamic : TransformationStageBase
    {
        /// <summary>
        /// Gets the mutation entries for the given pawn and hediff
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="source">The hediff.</param>
        /// <returns>an enumerable of possible mutations</returns>
        public override IEnumerable<MutationEntry> GetEntries(Pawn pawn, Hediff source)
        {
            if (!(source is Hediff_DynamicTf hediff))
            {
                Log.Error($"Hediff {source.def.defName} has a HediffStage_Mutation_Dynamic but is not a Hediff_DynamicTF!");
                return Enumerable.Empty<MutationEntry>();
            }

            //TODO caching?
            return hediff.MorphDef.GetAllMorphsInClass()
                    .SelectMany(m => m.AllAssociatedMutations)
                    .Select(mut => new MutationEntry
                    {
                        mutation = mut,
                        addChance = mut.defaultAddChance,
                        blocks = mut.defaultBlocks
                    });
        }
    }
}