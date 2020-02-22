// PMFoodUtilities.cs modified by Iron Wolf for Pawnmorph on 01/07/2020 7:51 PM
// last updated 01/07/2020  7:51 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.FormerHumans;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static class for food related utilities 
    /// </summary>
    public static class PMFoodUtilities
    {

        /// <summary>
        /// Gets the cannibal status of food for pawn.
        /// </summary>
        /// <param name="raceDef">The race definition.</param>
        /// <param name="foodSource">The food source.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// raceDef
        /// or
        /// foodSource
        /// </exception>
        public static CannibalThoughtStatus GetStatusOfFoodForPawn([NotNull] ThingDef raceDef, [NotNull] Thing foodSource)
        {
            if (raceDef == null) throw new ArgumentNullException(nameof(raceDef));
            if (foodSource == null) throw new ArgumentNullException(nameof(foodSource));
            if (foodSource.def == raceDef.race?.meatDef) return CannibalThoughtStatus.Direct;
            if (foodSource.TryGetComp<CompIngredients>()?.ingredients?.Any(d => d == raceDef.race?.meatDef) ?? false)
                return CannibalThoughtStatus.Ingredient;
            return CannibalThoughtStatus.None; 
        }
        
    }
    /// <summary>
    /// the status of the cannibal thought to receive 
    /// </summary>
    public enum CannibalThoughtStatus
    {
        /// <summary>
        /// the pawn did not eat anything they would consider cannibalism
        /// </summary>
        None,
        /// <summary>
        /// The pawn directly ate something they would consider cannibalism
        /// </summary>
        Direct,
        /// <summary>
        /// the pawn ate something they would consider cannibalism as an ingredient 
        /// </summary>
        Ingredient
    }
}