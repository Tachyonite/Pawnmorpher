using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.Abilities
{
	/// <summary>
	/// Abstract base class for defining abilities.
	/// </summary>
	public abstract class MutationAbility : IExposable
	{
		private MutationAbilityDef _def;
		private int _currentCooldown = 0;
		private Pawn _pawn;

		/// <summary>
		/// The current state of the ability.
		/// </summary>
		protected MutationAbilityState state = MutationAbilityState.None;

		/// <summary>
		/// Gets the ability definition.
		/// </summary>
		public MutationAbilityDef AbilityDef => _def;

		/// <summary>
		/// Gets the ability Gizmo.
		/// </summary>
		public Command Gizmo { get; private set; }

		/// <summary>
		/// Gets the pawn this ability is attached to.
		/// </summary>
		public Pawn Pawn => _pawn;

		/// <summary>
		/// Gets the total cooldown in ticks.
		/// </summary>
		public int Cooldown => _def.cooldown;

		/// <summary>
		/// Gets the current cooldown in ticks.
		/// </summary>
		public int CurrentCooldown => _currentCooldown;


		/// <summary>
		/// Gets the ability type. Used to create gizmo.
		/// </summary>
		abstract protected MutationAbilityType Type { get; }

		/// <summary>
		/// Gets the target parameters when using targeted gizmo.
		/// </summary>
		virtual protected RimWorld.TargetingParameters TargetParameters { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationAbility"/> class.
		/// </summary>
		/// <param name="def">The definition.</param>
		public MutationAbility(MutationAbilityDef def)
		{
			_def = def;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationAbility"/> class.
		/// </summary>
		public MutationAbility()
		{

		}

		private void LoadTexture()
		{
			if (String.IsNullOrWhiteSpace(_def.iconPath) == false)
				Gizmo.icon = ContentFinder<Texture2D>.Get(_def.iconPath);
		}

		/// <summary>
		/// Initializes the ability with the specified pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="def">The ability def.</param>
		public void Initialize(Pawn pawn, MutationAbilityDef def = null)
		{
			_pawn = pawn;

			if (def != null)
				_def = def;

			switch (Type)
			{
				case MutationAbilityType.Toggle:
					Gizmo = new Command_Toggle()
					{
						isActive = () => state == MutationAbilityState.Active,
						toggleAction = () => TryCast(null),
					};
					break;

				case MutationAbilityType.Target:
					Gizmo = new Command_Target()
					{
						action = (LocalTargetInfo target) => TryCast(target),
						targetingParams = TargetParameters ?? new RimWorld.TargetingParameters()
						{
							canTargetLocations = true,
						}
					};
					break;

				case MutationAbilityType.Action:
					Gizmo = new Command_Action()
					{
						action = () => TryCast(null),
					};
					break;

				default:
					Log.Error("Ability missing type.");
					Gizmo = new Command_Action();
					return;
			}

			Gizmo.defaultLabel = _def.label;
			Gizmo.defaultDesc = _def.description;
			HPatches.GizmoPatches.HideGizmoOnMerged(Gizmo, Pawn);

			LongEventHandler.ExecuteWhenFinished(LoadTexture);

			OnInitialize();

			if (pawn.Spawned)
				IsDisabled();
		}

		/// <summary>
		/// Exposes the data for serialization and deserialization.
		/// </summary>
		public void ExposeData()
		{
			Scribe_References.Look(ref _pawn, nameof(_pawn));
			Scribe_Deep.Look(ref _def, nameof(_def));
			Scribe_Values.Look(ref _currentCooldown, nameof(_currentCooldown));
			OnExposeData();
		}

		/// <summary>
		/// Triggered on expose data.
		/// </summary>
		protected abstract void OnExposeData();

		/// <summary>
		/// Ticks this instance.
		/// </summary>
		public void Tick()
		{
			if (Pawn == null || Gizmo == null)
				return;

			if (state == MutationAbilityState.None || Type == MutationAbilityType.Toggle)
			{
				if (Pawn.IsHashIntervalTick(120) && Gizmo.Visible)
					IsDisabled();

				return;
			}

			if (state == MutationAbilityState.Cooldown)
			{
				// Count down until cooldown is 1 tick remaining, then set enable gizmo and set it to 0.
				if (_currentCooldown > 1)
				{
					_currentCooldown--;
					if (_currentCooldown % 60 == 0)
						UpdateCooldownText();

					return;
				}
				else if (_currentCooldown-- > 0)
				{
					IsDisabled();
					_currentCooldown = 0;
					state = MutationAbilityState.None;
				}
				return;
			}

			OnTick();
		}

		private void IsDisabled()
		{
			string disabledReason = OnIsDisabled();
			if (String.IsNullOrWhiteSpace(disabledReason))
				Gizmo.Disabled = false;
			else
			{
				Gizmo.Disable(disabledReason);
				state = MutationAbilityState.None;
			}
		}

		private void TryCast(LocalTargetInfo? target)
		{
			if (OnTryCast(target))
			{
				state = MutationAbilityState.Casting;
				Gizmo.Disable("Casting...");
			}
		}

		/// <summary>
		/// Starts the ability cooldown.
		/// </summary>
		protected void StartCooldown()
		{
			state = MutationAbilityState.Cooldown;
			_currentCooldown = _def.cooldown;
			UpdateCooldownText();
		}

		private void UpdateCooldownText()
		{
			if (_currentCooldown > Utilities.TimeMetrics.TICKS_PER_HOUR)
				Gizmo.disabledReason = $"Cooldown_Hours".Translate(_currentCooldown / Utilities.TimeMetrics.TICKS_PER_HOUR);
			else
				Gizmo.disabledReason = $"Cooldown_Seconds".Translate(_currentCooldown / Utilities.TimeMetrics.TICKS_PER_REAL_SECOND);
		}

		/// <summary>
		/// Called when ability is initialized.
		/// </summary>
		protected abstract void OnInitialize();


		/// <summary>
		/// Called when ability is ticked.
		/// </summary>
		protected abstract void OnTick();


		/// <summary>
		/// Called when ability is being cast. Return bool on whether cast succeeded or not.
		/// </summary>
		protected abstract bool OnTryCast(LocalTargetInfo? target);

		/// <summary>
		/// Called every so often to validate whether or not the skill is available.
		/// </summary>
		protected abstract string OnIsDisabled();
	}
}
