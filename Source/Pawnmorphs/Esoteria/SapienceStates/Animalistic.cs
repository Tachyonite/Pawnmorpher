// Animalistic.cs created by Iron Wolf for Pawnmorph on 04/25/2020 2:17 PM
// last updated 04/25/2020  2:17 PM

using System;
using Pawnmorph.Aspects;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.SapienceStates
{
	/// <summary>
	/// sapience state for 'animalistic' humanoids 
	/// </summary>
	/// <seealso cref="Pawnmorph.SapienceState" />
	public class Animalistic : SapienceState
	{
		/// <summary>
		/// Gets a value indicating whether this state makes the pawn count as a 'former human'.
		/// </summary>
		/// <value>
		///   <c>true</c> if this state makes the pawn count as a 'former human'; otherwise, <c>false</c>.
		/// </value>
		public override bool IsFormerHuman => false;

		private bool CanRemove
		{
			get
			{
				var asp = Pawn.GetAspectTracker()?.Aspects;
				foreach (Aspect aspect in asp.MakeSafe())
				{
					if (aspect is SapienceHit) return false;
				}

				return true;
			}
		}

		/// <summary>
		///     Gets the current intelligence.
		/// </summary>
		/// <value>
		///     The current intelligence.
		/// </value>
		public override Intelligence CurrentIntelligence
		{
			get
			{
				switch (CurrentSapience)
				{
					case SapienceLevel.Sapient:
					case SapienceLevel.MostlySapient:
					case SapienceLevel.Conflicted:
						return Intelligence.Humanlike;
					case SapienceLevel.MostlyFeral:
					case SapienceLevel.Feral:
					case SapienceLevel.PermanentlyFeral:
						return Intelligence.Animal;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		///     called after every tick
		/// </summary>
		public override void Tick()
		{
			if (_doPostInit)
			{
				//need to defer this to the first tick after being added so Tracker has a chance to update correctly 
				_waiting = Tracker.SapienceLevel == SapienceLevel.Sapient;
				_doPostInit = false;
			}

			if (_waiting && RandUtilities.MtbDaysEventOccured(MTB))
			{
				if (!CanRemove) return;
				Tracker?.ExitState();
			}
		}

		/// <summary>
		///     called to save/load all data.
		/// </summary>
		protected override void ExposeData()
		{
			Scribe_Values.Look(ref _waiting, "waiting");
		}

		/// <summary>
		/// Adds the or remove dynamic components.
		/// </summary>
		public override void AddOrRemoveDynamicComponents()
		{
			switch (CurrentSapience)
			{
				case SapienceLevel.Sapient:
				case SapienceLevel.MostlySapient:
				case SapienceLevel.Conflicted:
					InitHumanlikeComps();
					break;
				case SapienceLevel.MostlyFeral:
					InitMostlyFeralComps();
					break;
				case SapienceLevel.Feral:
				case SapienceLevel.PermanentlyFeral:
					SetupFeralComps();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void InitMostlyFeralComps()
		{
			MakeFeral();
			AddMostlyFeralComps();
			AddHumanlikeComps();


			var onMap = Pawn.Map != null;
			if (onMap)
			{
				Pawn.equipment?.DropAllEquipment(Pawn.GetCorrectPosition(), Pawn.Faction != Faction.OfPlayer);
			}
			else
			{
				Pawn.equipment?.DestroyAllEquipment();
			}

		}

		private void InitHumanlikeComps()
		{
			RemoveAnimalComps();
			AddHumanlikeComps();
		}

		void AddHumanlikeComps()
		{
			Pawn.drafter = Pawn.drafter ?? new Pawn_DraftController(Pawn);

			Pawn.equipment = Pawn.equipment ?? new Pawn_EquipmentTracker(Pawn);
			Pawn.apparel = Pawn.apparel ?? new Pawn_ApparelTracker(Pawn);
			Pawn.workSettings = Pawn.workSettings ?? new Pawn_WorkSettings(Pawn);
		}

		private void SetupFeralComps()
		{
			AddMostlyFeralComps();
			if (Pawn.drafter != null)
			{
				Pawn.drafter.Drafted = false;
				Pawn.drafter = null;
			}

			Pawn.ideo = null;
			Pawn.style = null;
			Pawn.styleObserver = null;

			IntVec3 pawnPosition = Pawn.Position;
			if (Pawn.Map != null)
			{

				Pawn.apparel?.DropAll(pawnPosition, Pawn.Faction != Faction.OfPlayer, false);
				Pawn.equipment?.DropAllEquipment(pawnPosition, Pawn.Faction == Faction.OfPlayer);
			}
			else
			{
				Pawn.apparel?.DestroyAll();
				Pawn.equipment?.DestroyAllEquipment();
			}

		}

		private void AddMostlyFeralComps()
		{
			Pawn.training = Pawn.training ?? new Pawn_TrainingTracker(Pawn);
		}

		private void RemoveAnimalComps()
		{
			Pawn.training = null;

		}

		private bool _subscribed;

		void InitEvents()
		{
			if (_subscribed) return;
			var sN = Tracker.SapienceNeed;
			if (sN == null) return;
			sN.SapienceLevelChanged += SapienceLevelChanged;
		}

		private bool _waiting;
		private const float MTB = 1.5f;
		private void SapienceLevelChanged(Need_Control sender, Pawn pawn, SapienceLevel oldLevel, SapienceLevel currentLevel)
		{
			if (currentLevel == SapienceLevel.Sapient)
			{
				_waiting = true;
			}
		}

		/// <summary>
		///     called when a pawn enters this sapience state
		/// </summary>
		public override void Enter()
		{
			base.Enter();
			InitEvents();

		}

		/// <summary>
		/// called when the pawn exits this state 
		/// </summary>
		public override void Exit()
		{
			base.Exit();
			if (_subscribed)
			{
				var sN = Tracker.SapienceNeed;
				if (sN == null) return;
				sN.SapienceLevelChanged -= SapienceLevelChanged;
				_subscribed = true;
			}
		}

		private bool _doPostInit;

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		/// this is always called before enter and after loading a pawn
		protected override void Init()
		{
			InitEvents();
			_doPostInit = true;
		}
	}
}