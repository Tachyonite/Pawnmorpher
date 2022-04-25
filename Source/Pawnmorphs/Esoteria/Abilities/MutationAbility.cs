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

        public Gizmo Gizmo { get; private set; }
        public Pawn Pawn { get; private set; }
        public int Cooldown => _def.cooldown;
        public int CurrentCooldown => currentCooldown;

        private int currentCooldown = 0;
        protected bool casting = false;



        public MutationAbility()
        {
            Texture2D icon = null;
            if (String.IsNullOrWhiteSpace(_def.iconPath) == false)
                icon = ContentFinder<Texture2D>.Get(_def.iconPath);

            Gizmo = new Command_Action()
            {
                action = OnClick,
                defaultLabel = _def.label,
                defaultDesc = _def.description,
                icon = icon,
            };
        }

        public void Initialize(Pawn pawn)
        {
            Pawn = pawn;
            OnInitialize();
        }

        public void Tick()
        {
            // Count down until cooldown is 1 tick remaining, then set enable gizmo and set it to 0.
            if (currentCooldown-- > 1)
            {
                if (currentCooldown % 60 == 0)
                    Gizmo.disabledReason = $"Cooling down: {currentCooldown/60}s";

                return;
            }
            else if (currentCooldown == 1)
            {
                Gizmo.disabled = false;
                Gizmo.disabledReason = null;
                currentCooldown = 0;
            }

            OnTick();
        }

        private void OnClick()
        {
            if (_def.targeted)
            {
                TryCast(null);
                return;
            }

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    TryCast(UI.MouseCell());
                }
                if (Event.current.button == 1)
                {
                    DebugTools.curTool = null;
                }
                Event.current.Use();
            }
        }

        private void TryCast(IntVec3? target)
        {
            if (OnCast(target))
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
        }

        protected abstract void OnInitialize();
        protected abstract void OnTick();
        protected abstract bool OnCast(IntVec3? target);
    }
}
