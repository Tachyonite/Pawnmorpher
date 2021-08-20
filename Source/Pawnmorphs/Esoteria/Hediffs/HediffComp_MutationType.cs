using System;
using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// An abstract base comp for all comps to to be used with MutType_FromComp
    /// and TFType_FromComp.  These comps track mutation state to allow different
    /// hediff stages to share the same mutation/TF types
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.Composable.MutTypes_FromComp"/>
    /// <seealso cref="Pawnmorph.Hediffs.Composable.TFTypes_FromComp"/>
    public abstract class HediffComp_MutationType : HediffComp
    {
        /// <summary>
        /// Returns a list of mutations all MutTypes_FromComp stages will use
        /// </summary>
        /// <returns>The mutations.</returns>
        public abstract IEnumerable<MutationDef> GetMutations();

        /// <summary>
        /// Gets the TF.
        /// </summary>
        /// <returns>The TF.</returns>
        public abstract IEnumerable<PawnKindDef> GetTFs();
    }
}
