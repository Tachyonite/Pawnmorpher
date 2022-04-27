using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.Abilities
{
    public enum MutationAbilityType
    {
        /// <summary>
        /// Toggled ability.
        /// </summary>
        Toggle = 1,

        /// <summary>
        /// Targeted ability.
        /// </summary>
        Target = 2,

        /// <summary>
        /// Instant ability.
        /// </summary>
        Action = 3,
    }
}
