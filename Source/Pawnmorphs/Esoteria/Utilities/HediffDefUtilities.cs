// HediffDefUtilities.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/11/2019 7:59 AM
// last updated 09/11/2019  7:59 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Utilities
{
    /// <summary>
    /// collection of hediff def related utility functions 
    /// </summary>
    public static class HediffDefUtilities
    {
        /// <summary>
        /// get all hediff givers attached to this HediffDef 
        /// </summary>
        /// <param name="hediffDef"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">hediffDef</exception>
        [NotNull]
        public static IEnumerable<HediffGiver> GetAllHediffGivers([NotNull] this HediffDef hediffDef)
        {
            if (hediffDef == null) throw new ArgumentNullException(nameof(hediffDef));
            foreach (var giver in hediffDef.hediffGivers ?? Enumerable.Empty<HediffGiver>())
            {
                yield return giver; 
            }


            foreach (var stage in hediffDef.stages ?? Enumerable.Empty<HediffStage>())
            {
                foreach (var giver in stage.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                {
                    yield return giver;
                }
            }
        }
    }
}