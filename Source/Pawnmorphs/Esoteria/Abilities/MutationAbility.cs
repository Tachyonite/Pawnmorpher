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

        public Command Gizmo { get; private set; }
        public Pawn Pawn { get; private set; }
        public int Cooldown => _def.cooldown;
        public MutationAbilityDef AbilityDef => _def;
        public int CurrentCooldown => currentCooldown;

        private int currentCooldown = 0;
        protected bool casting = false;
        protected bool active = false;


        abstract protected MutationAbilityType Type { get; }
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

        public void Initialize(Pawn pawn)
        {
            Pawn = pawn;
            OnInitialize();
        }

        public void Tick()
        {
            // Count down until cooldown is 1 tick remaining, then set enable gizmo and set it to 0.
            if (currentCooldown > 1)
            {
                currentCooldown--;
                if (currentCooldown % 60 == 0)
                    Gizmo.disabledReason = $"Cooling down: {currentCooldown/60}s";

                Log.Message("Cooling down: " + currentCooldown);
                return;
            }
            else if (currentCooldown-- > 0)
            {
                Log.Message("Finished cooling down");
                Gizmo.disabled = false;
                Gizmo.disabledReason = null;
                currentCooldown = 0;
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

        protected void StartCooldown()
        {
            casting = false;
            currentCooldown = _def.cooldown;
            Gizmo.disabledReason = $"Cooling down: {currentCooldown / 60}s";
            active = !active;
        }

        protected abstract void OnInitialize();
        protected abstract void OnTick();
        protected abstract bool OnTryCast(LocalTargetInfo? target);
    }
}
