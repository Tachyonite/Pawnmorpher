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

        
        private SapienceLevel _sapienceLevel; 

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
        public bool IsPermanentlyFeral => _isFormerHuman && _sapienceLevel == SapienceLevel.PermanentlyFeral;

        /// <summary>
        /// Gets or sets the sapience level.
        /// </summary>
        /// <value>
        /// The sapience level.
        /// </value>
        public SapienceLevel SapienceLevel
        {
            get { return _sapienceLevel;  }
            set
            {
                if (_sapienceLevel != value)
                {
                    var last = _sapienceLevel; 
                    _sapienceLevel = value;
                    OnSapienceLevelChanges(last); 
                }

            }
        }

        private void OnSapienceLevelChanges(SapienceLevel lastSapienceLevel)
        {
            if (lastSapienceLevel.IsColonistAnimal() && !_sapienceLevel.IsColonistAnimal())
            {
                OnNoLongerColonist(); 
            }else if (lastSapienceLevel <= SapienceLevel.MostlyFeral && _sapienceLevel > SapienceLevel.MostlyFeral)
            {
                OnNoLongerSapient(); 
            }


        }

        private void OnNoLongerSapient()
        {
           
        }

        private void OnNoLongerColonist()
        {
            Find.ColonistBar.MarkColonistsDirty();
        }


        /// <summary>
        ///     Makes the parent a former human.
        /// </summary>
        /// <returns></returns>
        public void MakeFormerHuman(float initialLevel) //TODO move most of FormerHumanUtilities.MakeAnimalSapient here 
        {
            if (_isFormerHuman)
            {
                Log.Warning($"{nameof(MakeFormerHuman)} is being called on {parent.def}'s {nameof(FormerHumanTracker)} more then once!");
                return;
            }

            _isFormerHuman = true;
            SapienceLevel = FormerHumanUtilities.GetQuantizedSapienceLevel(initialLevel); 
        }


        /// <summary>
        ///     Makes the parent thing permanently feral.
        /// </summary>
        public void MakePermanentlyFeral()
        {
            if (!_isFormerHuman)
                Log.Error($"trying to make a non former human \"{PMThingUtilities.GetDebugLabel(parent)}\" permanently feral");

            SapienceLevel = SapienceLevel.PermanentlyFeral; 
            //TODO move most of FormerHumanUtilities.MakePermanentlyFeral here 
        }

        /// <summary>
        ///     saves/loads all data
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _isFormerHuman, "isFormerHuman");
            Scribe_Values.Look(ref _sapienceLevel, "sapience");   
            base.PostExposeData();
        }
    }
}