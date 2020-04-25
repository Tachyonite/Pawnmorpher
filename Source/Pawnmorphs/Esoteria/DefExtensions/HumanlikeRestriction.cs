// HumanoidRestriction.cs created by Iron Wolf for Pawnmorph on 04/22/2020 5:26 PM
// last updated 04/22/2020  5:26 PM

using System;
using Verse;

namespace Pawnmorph.DefExtensions
{
    /// <summary>
    /// def extension that restricts something to humanlike pawns, taking account of former humans 
    /// </summary>
    /// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
    public class HumanlikeRestriction : RestrictionExtension
    {
        /// <summary>
        /// if non sapient former humans should be included 
        /// </summary>
        public bool includeNonSapientFormerHumans; 

        /// <summary>
        ///     checks if the given pawn passes the restriction.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///     if the def can be used with the given pawn
        /// </returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        protected override bool PassesRestrictionImpl(Pawn pawn)
        {
            return pawn.IsHumanlike(includeNonSapientFormerHumans); 
        }
    }
}