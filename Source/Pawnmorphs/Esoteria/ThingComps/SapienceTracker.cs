// FormerHuman.cs modified by Iron Wolf for Pawnmorph on 02/17/2020 9:29 PM
// last updated 02/17/2020  9:29 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    ///     thing comp to track the 'former human' status of a pawn
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class SapienceTracker : ThingComp
    {
        //TODO make this more extendable some how 
        //pawn state or something? 
        private bool _isFormerHuman;


        private SapienceLevel _sapienceLevel;


        /// <summary>
        ///     Gets the sapience need.
        /// </summary>
        /// <value>
        ///     The sapience need.
        /// </value>
        [CanBeNull]
        public Need_Control SapienceNeed { get; private set; }

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
        ///     Gets or sets the sapience level.
        /// </summary>
        /// <value>
        ///     The sapience level.
        /// </value>
        public SapienceLevel SapienceLevel
        {
            get => _sapienceLevel;
            set
            {
                if (_sapienceLevel != value)
                {
                    SapienceLevel last = _sapienceLevel;
                    _sapienceLevel = value;
                    OnSapienceLevelChanges(last);
                }
            }
        }

        /// <summary>
        ///     Gets the sapience level of the pawn
        /// </summary>
        /// <value>
        ///     The sapience.
        /// </value>
        public float Sapience => SapienceNeed?.CurLevel ?? 0;

        private Pawn Pawn => (Pawn) parent;

        /// <summary>
        ///     Initializes the specified props.
        /// </summary>
        /// <param name="props">The props.</param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (!(parent is Pawn))
            {
                Log.Error($"{nameof(SapienceTracker)} is attached to {parent.GetType().Name}! this comp can only be added to a pawn");
                return;
            }

            SapienceNeed = Pawn.needs?.TryGetNeed<Need_Control>();
        }


        /// <summary>
        ///     Makes the parent a former human.
        /// </summary>
        /// <returns></returns>
        public void MakeFormerHuman(float initialLevel) //TODO move most of FormerHumanUtilities.MakeAnimalSapient here 
        {
            if (_isFormerHuman)
            {
                Log.Warning($"{nameof(MakeFormerHuman)} is being called on {parent.def}'s {nameof(SapienceTracker)} more then once!");
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

        /// <summary>
        ///     Sets the sapience.
        /// </summary>
        /// <param name="sapience">The sapience.</param>
        public void SetSapience(float sapience)
        {
            if (SapienceNeed == null)
            {
                Log.Error("trying to set the sapience level of a pawn that does not have the sapience need!");
                return;
            }

            SapienceNeed.SetSapience(sapience);
        }

        private void OnNoLongerColonist()
        {
            Find.ColonistBar.MarkColonistsDirty();
        }

        private void OnNoLongerSapient()
        {
        }

        private void OnSapienceLevelChanges(SapienceLevel lastSapienceLevel)
        {
            if (lastSapienceLevel.IsColonistAnimal() && !_sapienceLevel.IsColonistAnimal())
                OnNoLongerColonist();
            else if (lastSapienceLevel <= SapienceLevel.MostlyFeral && _sapienceLevel > SapienceLevel.MostlyFeral)
                OnNoLongerSapient();


            //try to send a message 
            if (Pawn.Faction?.IsPlayer == true)
                SendTransitionLetter();
        }

        private void SendTransitionLetter()
        {
            // the translation keys should be $SapienceLevel_TransitionLabel and $SapienceLevel_TransitionContent
            string translationLabel = _sapienceLevel + "_Transition";
            string letterLabelKey = translationLabel + "Label";
            string letterContentKey = translationLabel + "Content";
            TaggedString letterContent, letterLabel;


            if (letterLabelKey.TryTranslate(out letterLabel) && letterContentKey.TryTranslate(out letterContent))
            {
                letterLabel = letterLabel.AdjustedFor(Pawn);
                letterContent = letterContent.AdjustedFor(Pawn);

                Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent, new LookTargets(Pawn));
            }
        }
    }
}