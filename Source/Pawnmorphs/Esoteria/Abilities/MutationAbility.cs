using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.Abilities
{
    /// <summary>
    /// Abstract base class for defining abilities.
    /// </summary>
    public abstract class MutationAbility
    {
        private MutationAbilityDef _def;
        private int _currentCooldown = 0;

        /// <summary>
        /// Whether or not the ability currently being cast.
        /// </summary>
        protected bool casting = false;

        /// <summary>
        /// Whether or not the ability is currently active.
        /// </summary>
        protected bool active = false;

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
        public Pawn Pawn { get; private set; }

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

        public MutationAbility(MutationAbilityDef def)
        {
            _def = def;
            Texture2D icon = null;
            if (String.IsNullOrWhiteSpace(_def.iconPath) == false)
                icon = ContentFinder<Texture2D>.Get(_def.iconPath);

            switch (Type)
            {
                case MutationAbilityType.Toggle:
                    Gizmo = new Command_Toggle()
                    {
                        isActive = () => active,
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
            Gizmo.icon = icon;
            HPatches.GizmoPatches.HideGizmoOnMerged(Gizmo);
        }

        /// <summary>
        /// Initializes the ability with the specified pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        public void Initialize(Pawn pawn)
        {
            Pawn = pawn;
            OnInitialize();
        }

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public void Tick()
        {
            // Count down until cooldown is 1 tick remaining, then set enable gizmo and set it to 0.
            if (_currentCooldown > 1)
            {
                _currentCooldown--;
                if (_currentCooldown % 60 == 0)
                    Gizmo.disabledReason = $"Cooling down: {_currentCooldown/60}s";

                Log.Message("Cooling down: " + _currentCooldown);
                return;
            }
            else if (_currentCooldown-- > 0)
            {
                Log.Message("Finished cooling down");
                Gizmo.disabled = false;
                Gizmo.disabledReason = null;
                _currentCooldown = 0;
            }

            if (active || casting)
                OnTick();
        }

        private void TryCast(LocalTargetInfo? target)
        {
            if (OnTryCast(target))
            {
                casting = true;
                Gizmo.Disable("Casting...");
            }
        }

        /// <summary>
        /// Starts the ability cooldown.
        /// </summary>
        protected void StartCooldown()
        {
            casting = false;
            _currentCooldown = _def.cooldown;
            Gizmo.disabledReason = $"Cooling down: {_currentCooldown / 60}s";
            active = !active;
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
    }
}
