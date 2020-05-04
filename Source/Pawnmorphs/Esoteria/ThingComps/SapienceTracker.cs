// FormerHuman.cs modified by Iron Wolf for Pawnmorph on 02/17/2020 9:29 PM
// last updated 02/17/2020  9:29 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;
using FormerHuman = Pawnmorph.SapienceStates.FormerHuman;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    ///     thing comp to track the 'former human' status of a pawn
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class SapienceTracker : ThingComp
    {
        private SapienceState _sapienceState;
        private bool _subscribed;

        void TrySubscribe()
        {   
            if (_subscribed) return; 
            var sNeed = SapienceNeed;
            if (sNeed != null)
            {
                _subscribed = true; 
                sNeed.SapienceLevelChanged += SapienceLevelChanges; 
            }
        }

        private void SapienceLevelChanges(Need_Control sender, Pawn pawn, SapienceLevel sapiencelevel)
        {
            if (pawn.Faction != Faction.OfPlayer) return;

           
                Find.ColonistBar.MarkColonistsDirty();
            
        }

        /// <summary>
        /// Gets the current sapience state that pawn is in 
        /// </summary>
        /// <value>
        /// Gets the current sapience state that pawn is in 
        /// </value>
        public SapienceState CurrentState => _sapienceState; 

        //TODO make this more extendable some how 
        //pawn state or something? 
        private bool _isFormerHuman;


        private SapienceLevel _sapienceLevel;

        /// <summary>
        /// called every tick 
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            _sapienceState?.Tick();
        }


        /// <summary>
        /// enter the given sapience state
        /// </summary>
        /// <param name="stateDef">The state definition.</param>
        /// <param name="initialLevel">The initial level.</param>
        public void EnterState([NotNull] SapienceStateDef stateDef, float initialLevel)
        {
            _sapienceState?.Exit();
            _sapienceState = stateDef.CreateState();
            _sapienceState.Init(this);

            SapienceLevel = FormerHumanUtilities.GetQuantizedSapienceLevel(initialLevel);
            //need to refresh comps and needs for pawn here 
            PawnComponentsUtility.AddAndRemoveDynamicComponents(Pawn);
            Pawn.needs?.AddOrRemoveNeedsAsAppropriate();
            var sNeed = SapienceNeed;
            sNeed?.SetSapience(initialLevel);
            _sapienceState.Enter();

          


        }

        /// <summary>
        /// Gets the current intelligence of the attached pawn.
        /// </summary>
        /// <value>
        /// The current intelligence.
        /// </value>
        public Intelligence CurrentIntelligence => _sapienceState?.CurrentIntelligence ?? Pawn.RaceProps.intelligence; 


        /// <summary>
        ///     Gets the sapience need.
        /// </summary>
        /// <value>
        ///     The sapience need.
        /// </value>
        [CanBeNull]
        public Need_Control SapienceNeed => Pawn.needs?.TryGetNeed<Need_Control>(); 

        /// <summary>
        ///     Gets a value indicating whether this instance is a former human.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is a former human; otherwise, <c>false</c>.
        /// </value>
        [Obsolete("use " + nameof(CurrentIntelligence) + " or " + nameof(CurrentState) + " instead")]
        public bool IsFormerHuman => _sapienceState?.StateDef == SapienceStateDefOf.FormerHuman;


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

        /// <summary>
        /// Gets the pawn this comp is attached to 
        /// </summary>
        /// <value>
        /// The pawn.
        /// </value>
        public Pawn Pawn => (Pawn) parent;

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

            _sapienceState?.Init(this); 
            TrySubscribe();
        }


        /// <summary>
        ///     Makes the parent a former human.
        /// </summary>
        /// <returns></returns>
        [Obsolete("use " + nameof(EnterState) + "instead")]
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
            if (!_isFormerHuman || _sapienceState == null)
            {
                Log.Error($"trying to make a non former human \"{PMThingUtilities.GetDebugLabel(parent)}\" permanently feral");
                return;
            }


            //hacky 
            //need a better solution 
            try
            {
                var fhState = (FormerHuman) _sapienceState;
                fhState.MakePermanentlyFeral();
                SapienceLevel = SapienceLevel.PermanentlyFeral;

            }
            catch (InvalidCastException e)
            {
                    Log.Error($"tried to make {Pawn.Name} in state \"{_sapienceState.GetType().Name}\" permanently feral but this is only supported for {nameof(FormerHuman)}!\n{e.ToString().Indented("|\t")}");
            }
        }

        /// <summary>
        ///     saves/loads all data
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _isFormerHuman, "isFormerHuman");
            Scribe_Values.Look(ref _sapienceLevel, "sapience");
            Scribe_Deep.Look(ref _sapienceState, nameof(CurrentState));

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _sapienceState?.Init(this); 
                TrySubscribe();
            }


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

        }

      
    }
}