// TransformationStage.cs created by Iron Wolf for Pawnmorph on 01/02/2020 1:44 PM
// last updated 01/02/2020  1:44 PM

using System.Collections.Generic;
using Pawnmorph.Utilities;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     hediff stage that adds the possibility of adding mutations
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
    public class TransformationStage : TransformationStageBase 
    {
        /// <summary>The mutations that this stage can add</summary>
        public List<MutationEntry> mutations;

        /// <summary>
        /// Gets all mutation entries in this stage
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public override IEnumerable<MutationEntry> Entries => mutations.MakeSafe(); 

    }
}