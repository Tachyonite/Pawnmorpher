// FormerHuman.cs modified by Iron Wolf for Pawnmorph on 02/17/2020 9:29 PM
// last updated 02/17/2020  9:29 PM

using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    ///     thing comp to track the 'former human' status of a pawn
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class FormerHumanTracker : ThingComp
    {
        private bool _isFormerHuman;

        private bool _permanentlyFeral;

        /// <summary>
        ///     Gets a value indicating whether this instance is a former human.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is a former human; otherwise, <c>false</c>.
        /// </value>
        public bool IsFormerHuman => _isFormerHuman;


        /// <summary>
        ///     Gets a value indicating whether this instance is permanently feral.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is permanently feral; otherwise, <c>false</c>.
        /// </value>
        public bool IsPermanentlyFeral => _isFormerHuman && _permanentlyFeral;


        /// <summary>
        ///     Makes the parent a former human.
        /// </summary>
        /// <returns></returns>
        public void MakeFormerHuman() //TODO move most of FormerHumanUtilities.MakeAnimalSapient here 
        {
            if (_isFormerHuman)
            {
                Log.Warning($"{nameof(MakeFormerHuman)} is being called on {parent.def}'s {nameof(FormerHumanTracker)} more then once!");
                return;
            }

            _isFormerHuman = true;
        }


        /// <summary>
        ///     Makes the parent thing permanently feral.
        /// </summary>
        public void MakePermanentlyFeral()
        {
            if (!_isFormerHuman)
                Log.Error($"trying to make a non former human \"{PMThingUtilities.GetDebugLabel(parent)}\" permanently feral");

            _permanentlyFeral = true;
            //TODO move most of FormerHumanUtilities.MakePermanentlyFeral here 
        }

        /// <summary>
        ///     saves/loads all data
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _isFormerHuman, "isFormerHuman");
            Scribe_Values.Look(ref _permanentlyFeral, nameof(_permanentlyFeral));
            base.PostExposeData();
        }
    }
}