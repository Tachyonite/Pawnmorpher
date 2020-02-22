// SapientAnimalRestriction.cs modified by Iron Wolf for Pawnmorph on 12/11/2019 8:03 PM
// last updated 12/11/2019  8:03 PM

using System;
using Verse;

namespace Pawnmorph.DefExtensions
{
    /// <summary>
    /// restriction on defs that mark them for use for sapient animals only 
    /// </summary>
    /// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
    public class SapientAnimalRestriction : RestrictionExtension
    {
        
        /// <summary>
        /// checks if the given pawn passes the restriction.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        /// if the def can be used with the given pawn
        /// </returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        protected override bool PassesRestrictionImpl(Pawn pawn)
        {
            return pawn.IsSapientFormerHuman(); 
        }
    }
}