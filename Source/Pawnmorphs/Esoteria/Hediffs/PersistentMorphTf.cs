// PersistentMorphTf.cs created by Iron Wolf for Pawnmorph on 07/06/2020 7:03 AM
// last updated 07/06/2020  7:04 AM

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// morph tf that sticks around on non mutable pawns even if they don't mutate 
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.MorphTf" />
    public class PersistentMorphTf : MorphTf
    {
        /// <summary>
        /// Gets a value indicating whether  this should be removed if the pawn is not mutable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this should be removed if the pawn is not mutable.; otherwise, <c>false</c>.
        /// </value>
        protected override bool RemoveIfNotMutable => false; 
    }
}