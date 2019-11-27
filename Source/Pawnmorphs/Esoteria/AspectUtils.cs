// AspectUtils.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:30 AM
// last updated 09/22/2019  11:30 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// a collection of aspect related utilities 
    /// </summary>
    public static class AspectUtils
    {
        /// <summary>
        /// get the aspect tracker from this pawn 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        [CanBeNull]
        public static AspectTracker GetAspectTracker([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            return pawn.GetComp<AspectTracker>(); 
        }

        /// <summary> Get the total production multiplier for the given mutation. </summary>
        public static float GetProductionBoost([NotNull] this IEnumerable<Aspect> aspects, HediffDef mutation)
        {
            float accum = 0;
            foreach (Aspect aspect in aspects)
            {
                accum += aspect.GetBoostOffset(mutation); 
            }

            return accum; 
        }

        /// <summary>
        /// Determines whether this instance can receive rare mutations 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can receive rare mutations  otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">pawn</exception>
        public static bool CanReceiveRareMutations([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            return pawn.GetAspectTracker()?.GetAspect(AspectDefOf.RareMutant) != null; 
        }

        /// <summary>
        /// Determines whether this instance can grow mutagenic plants.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can grow mutagenic plants; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">pawn</exception>
        public static bool CanGrowMutagenicPlants([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            return pawn.GetAspectTracker()?.GetAspect(AspectDefOf.PlantAffinity) != null;
        }
    }
}