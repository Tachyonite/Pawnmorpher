// FormerHuman.cs modified by Iron Wolf for Pawnmorph on 02/17/2020 9:29 PM
// last updated 02/17/2020  9:29 PM

using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    /// thing comp to track the 'former human' status of a pawn 
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class FormerHuman : ThingComp
    {
        private FormerHumanStatus? _status;

        /// <summary>
        /// Gets the current former human status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public FormerHumanStatus? Status => _status;


        /// <summary>
        /// Gets a value indicating whether this instance is a former human.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a former human; otherwise, <c>false</c>.
        /// </value>
        public bool IsFormerHuman => _status != null;

        /// <summary>
        /// saves/loads all data
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _status, "status", null); 
            base.PostExposeData();
        }
    }
}