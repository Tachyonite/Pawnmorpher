// SapientAnimalRestriction.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 9:14 AM
// last updated 12/02/2019  9:14 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.DefExtensions
{

    /// <summary>
    /// des restriction that either restricts a def to sapient animals or forbids them 
    /// </summary>
    /// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
    public class SapientAnimalRestriction : RestrictionExtension
    {
        /// <summary>
        /// if true, then the def this is attached to is marked as invalid for sapient animals
        /// if false, then the def is is valid only for sapient animals 
        /// </summary>
        public bool isForbidden;

        /// <summary>
        /// filter used to further specify the attached mod to only work with/without specific animals 
        /// </summary>
        /// <see cref="Filter{T}"/>
        [NotNull]
        public Filter<ThingDef> raceFilter = new Filter<ThingDef>();

        /// <summary>
        /// checks if the given pawn passes the restriction.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>if the def can be used with the given pawn</returns>
        public override bool PassesRestriction(Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            var isSapientAnimal = pawn.GetFormerHumanStatus() == FormerHumanStatus.Sapient;
            if (isSapientAnimal)
            {
                bool passesFilter = raceFilter.PassesFilter(pawn.def);
                return isForbidden ? !passesFilter : passesFilter; //isForbidden should invert the check if true 
            }

            return isForbidden; 

        }
    }
}