// MutagenUtilities.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:11 PM
// last updated 08/13/2019  4:11 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph
{

    /// <summary>
    /// collection of mutagen related utility functions 
    /// </summary>
    public static class MutagenUtilities
    {

        
        /// <summary>
        /// Determines whether this instance can infect the specified pawn.
        /// </summary>
        /// <param name="morphTf">The morph tf hediff.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can infect the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// morphTf
        /// or
        /// pawn
        /// </exception>
        public static bool CanInfect([NotNull] this Hediff_Morph morphTf, [NotNull] Pawn pawn)
        {
            if (morphTf == null) throw new ArgumentNullException(nameof(morphTf));
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            var mutDef = morphTf.def as Def_MorphTf;
            var mutagen = mutDef?.mutagenSource ?? MutagenDefOf.defaultMutagen;
            return mutagen.CanInfect(pawn); 
        }

        /// <summary>
        /// Gets the mutagen associated with this tf hediff 
        /// </summary>
        /// <param name="morphTf">The morph tf.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">morphTf</exception>
        [NotNull]
        public static MutagenDef GetMutagenDef([NotNull] this Hediff_Morph morphTf)
        {
            if (morphTf == null) throw new ArgumentNullException(nameof(morphTf));

            var def = morphTf.def as Hediffs.Def_MorphTf;
            return def?.mutagenSource ?? MutagenDefOf.defaultMutagen; 
        }
    }
}