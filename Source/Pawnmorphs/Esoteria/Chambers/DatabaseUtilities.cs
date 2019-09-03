// DatabaseUtilities.cs modified by Iron Wolf for Pawnmorph on 09/02/2019 8:44 AM
// last updated 09/02/2019  8:44 AM

using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Chambers
{
    /// <summary>
    /// static class for various chamber database utility functions 
    /// </summary>
    public static class DatabaseUtilities
    {
        
        /// <summary>
        /// Determines whether this instance is the def of an animal that can be added to the chamber database
        /// </summary>
        /// <param name="inst">The inst.</param>
        /// <returns>
        ///   <c>true</c> if this instance can be added to the chamber database ; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">inst</exception>
        public static bool IsValidAnimal([NotNull] this ThingDef inst)
        {
            if (inst == null) throw new ArgumentNullException(nameof(inst));
            if (inst.race?.Animal != true) return false; //use != because inst.race?.Animal can be true,false or null
            return !IsChao(inst); 
        }

        public static bool IsChao(ThingDef def)
        {
            return def.race.FleshType == DefDatabase<FleshTypeDef>.GetNamed("Chaomorph"); //all chaomorphs have the same flesh type of Chaomorph 
        }
    }
}