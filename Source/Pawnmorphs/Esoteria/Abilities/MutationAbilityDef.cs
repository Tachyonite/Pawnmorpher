using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.Abilities
{
    public class MutationAbilityDef
    {
        /// <summary>
        /// The class that contains the logic for the ability. Must be a MutationAbility type.
        /// </summary>
        public Type abilityType;

        /// <summary>
        /// The ability caption.
        /// </summary>
        public string label;

        /// <summary>
        /// The ability description.
        /// </summary>
        public string description;

        /// <summary>
        /// Path to the icon that should be displayed for the ability button.
        /// </summary>
        public string iconPath;

        /// <summary>
        /// The total cooldown in ticks.
        /// </summary>
        public int cooldown;

        /// <summary>
        /// Determines if the ability is targeted or not.
        /// </summary>
        public bool targeted = false;
    }
}
