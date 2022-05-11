﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Abilities
{
    internal class TerrifyingRoar : MutationAbility
    {
        protected override MutationAbilityType Type => MutationAbilityType.Action;

        public TerrifyingRoar() : base()
        {
        }

        public TerrifyingRoar(MutationAbilityDef def) : base(def)
        {
        }



        protected override void OnInitialize()
        {

        }

        protected override string OnIsDisabled()
        {
            return null;
        }

        protected override void OnTick()
        {
            if (state == MutationAbilityState.Casting)
            {
                foreach (Pawn otherPawn in Pawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (otherPawn == Pawn)
                        continue;

                    if (otherPawn.Position.DistanceTo(Pawn.Position) < 10)
                        otherPawn.mindState.StartFleeingBecauseOfPawnAction(Pawn);
                }

                StartCooldown();
            }
        }



        protected override bool OnTryCast(LocalTargetInfo? target)
        {
            return true;
        }

        protected override void OnExposeData()
        {
        }
    }
}