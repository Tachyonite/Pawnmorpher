// FormerHuman.cs modified by Iron Wolf for Pawnmorph on 02/17/2020 9:29 PM
// last updated 02/17/2020  9:29 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.HPatches;
using Pawnmorph.SapienceStates;
using RimWorld;
using Verse;
using Verse.AI;

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

		//only used for moving old saves to new system 
		//probably safe to remove 
		private bool _isFormerHuman;

		private SapienceLevel _sapienceLevel;


		private Intelligence? _lastIntelligenceLevel;

		/// <summary>
		///     Gets the current sapience state that pawn is in
		/// </summary>
		/// <value>
		///     Gets the current sapience state that pawn is in
		/// </value>
		public SapienceState CurrentState => _sapienceState;

		/// <summary>
		///     Gets the current intelligence of the attached pawn.
		/// </summary>
		/// <value>
		///     The current intelligence.
		/// </value>
		public Intelligence CurrentIntelligence
		{
			get
			{
				if (_sapienceState == null)
					return Pawn.RaceProps.intelligence;

				//hacky fix to correct null reference exception during loading 
				if (_sapienceState.Tracker == null)
					_sapienceState.Tracker = this;

				return _sapienceState.CurrentIntelligence;
			}
		}


		/// <summary>
		///     Gets the sapience need.
		/// </summary>
		/// <value>
		///     The sapience need.
		/// </value>
		[CanBeNull]
		public Need_Control SapienceNeed => Pawn.needs?.TryGetNeed<Need_Control>();


		/// <summary>
		///     Gets a value indicating whether this instance is permanently feral.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is permanently feral; otherwise, <c>false</c>.
		/// </value>

		public bool IsPermanentlyFeral =>
			_sapienceState?.IsFormerHuman == true && _sapienceLevel == SapienceLevel.PermanentlyFeral;

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
					SapienceLevelChanges(SapienceNeed, Pawn, last, value);
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
		///     Gets the pawn this comp is attached to
		/// </summary>
		/// <value>
		///     The pawn.
		/// </value>
		public Pawn Pawn => (Pawn)parent;


		/// <summary>
		///     called every tick
		/// </summary>
		public override void CompTick()
		{
			base.CompTick();
			_sapienceState?.Tick();
		}


		/// <summary>
		///     enter the given sapience state
		/// </summary>
		/// <param name="stateDef">The state definition.</param>
		/// <param name="initialLevel">The initial level.</param>
		public void EnterState([NotNull] SapienceStateDef stateDef, float initialLevel)
		{
			_sapienceState?.Exit();
			_sapienceState = stateDef.CreateState();


			//need to refresh comps and needs for pawn here 

			Pawn.needs?.AddOrRemoveNeedsAsAppropriate();
			_sapienceState.Init(this);
			SapienceLevel = FormerHumanUtilities.GetQuantizedSapienceLevel(initialLevel);
			PawnComponentsUtility.AddAndRemoveDynamicComponents(Pawn);
			Need_Control sNeed = SapienceNeed;
			sNeed?.SetSapience(initialLevel);
			_sapienceState.Enter();

			FormerHumanUtilities.InvalidateIntelligence(Pawn);
			if (Pawn.Faction == Faction.OfPlayer)
				Find.ColonistBar?.MarkColonistsDirty();

			//initialize work settings if they have it 
			Pawn.workSettings?.EnableAndInitializeIfNotAlreadyInitialized();

			//interrupts any jobs in case this changes their intelligence 
			if (Pawn.thinker != null)
				Pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
		}


		/// <summary>
		///     Exits the current sapience state.
		/// </summary>
		/// <param name="recalculateComps">if set to <c>true</c> dynamic components will be recalculated after exiting the state.</param>
		public void ExitState(bool recalculateComps = true)
		{
			if (_sapienceState == null)
			{
				DebugLogUtils.Warning($"trying to exit sapience state in {Pawn.Name} but they aren't in one");
				return;
			}

			_sapienceState.Exit();
			_sapienceState = null;
			if (recalculateComps) PawnComponentsUtility.AddAndRemoveDynamicComponents(Pawn);
		}

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
		///     Makes the parent thing permanently feral.
		/// </summary>
		public void MakePermanentlyFeral()
		{
			if (!_isFormerHuman && !(_sapienceState != null && _sapienceState.IsFormerHuman))
			{
				Log.Error($"trying to make a non former human \"{PMThingUtilities.GetDebugLabel(parent)}\" permanently feral");
				return;
			}


			//hacky 
			//need a better solution 
			try
			{
				var fhState = (FormerHuman)_sapienceState;
				fhState.MakePermanentlyFeral();
				SapienceLevel = SapienceLevel.PermanentlyFeral;

				PawnPatches.QueuePostTickAction(Pawn, () =>
				{
					_sapienceState.AddOrRemoveDynamicComponents();
					_sapienceState.Exit();
					_sapienceState = null;
					Pawn.needs?.AddOrRemoveNeedsAsAppropriate(); //make sure any comps get added/removed as appropriate 
				});
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

			//need to make sure the tracker is always non null 
			if (_sapienceState != null) _sapienceState.Tracker = this;

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				//check to move old saves to the new sapience system 
				if (_isFormerHuman && _sapienceState == null)
				{
					_sapienceState = SapienceStateDefOf.FormerHuman.CreateState();
					_sapienceState.Init(this);
				}


				_sapienceState?.Init(this);
				TrySubscribe();

				FormerHumanUtilities.InvalidateIntelligence(Pawn);
				if (Pawn.Faction == Faction.OfPlayer)
					Find.ColonistBar?.MarkColonistsDirty();
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

		private void SapienceLevelChanges(Need_Control sender, Pawn pawn, SapienceLevel oldLevel, SapienceLevel currentLevel)
		{
			FormerHumanUtilities.InvalidateIntelligence(Pawn);
			if (pawn.Faction?.def.isPlayer != true)
				return;

			if (oldLevel.IsColonistAnimal() && !currentLevel.IsColonistAnimal())
				OnNoLongerColonist();

			if (_lastIntelligenceLevel != CurrentIntelligence)
			{
				_lastIntelligenceLevel = CurrentIntelligence;

				// Release draft if pawn sapience dropped below draftable.
				if (CurrentIntelligence == Intelligence.Animal)
				{
					if (pawn.Drafted)
						pawn.drafter.Drafted = false;
				}
			}
		}

		private void TrySubscribe()
		{
			if (_subscribed) return;
			Need_Control sNeed = SapienceNeed;
			if (sNeed != null)
			{
				_subscribed = true;
				sNeed.SapienceLevelChanged += SapienceLevelChanges;
			}
		}
	}
}