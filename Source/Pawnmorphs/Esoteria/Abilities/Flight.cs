using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Abilities
{
    internal class Flight : MutationAbility
    {
        protected override MutationAbilityType Type => MutationAbilityType.Target;
        protected override TargetingParameters TargetParameters => new TargetingParameters()
        {
            canTargetLocations = true,
            validator = ((TargetInfo x) => DropCellFinder.IsGoodDropSpot(x.Cell, x.Map, allowFogged: true, canRoofPunch: false))
        };

        public Flight(MutationAbilityDef def) : base(def)
        {
        }

        protected override bool OnTryCast(LocalTargetInfo? target)
        {
            Log.Message("Flight cast at " + target);
            return true;
        }

        protected override void OnInitialize()
        {
            Log.Message("Flight initialized");
            
        }

        protected override void OnTick()
        {
            if (casting)
            {
                StartCooldown();
            }
        }
    }
}
