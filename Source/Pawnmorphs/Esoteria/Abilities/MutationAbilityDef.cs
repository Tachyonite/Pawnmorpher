using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Abilities
{
    /// <summary>
    /// Ability properties.
    /// </summary>
    /// <seealso cref="Verse.IExposable" />
    public class MutationAbilityDef : IExposable
    {
        /// <summary>
        /// The class that contains the logic for the ability. Must be a MutationAbility type.
        /// </summary>
        public Type abilityClass;

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
        /// Exposes the data for serialization and deserialization.
        /// </summary>
        public void ExposeData()
        {
            Scribe_Values.Look<Type>(ref abilityClass, nameof(abilityClass));
            Scribe_Values.Look<string>(ref label, nameof(label));
            Scribe_Values.Look<string>(ref description, nameof(description));
            Scribe_Values.Look<string>(ref iconPath, nameof(iconPath));
            Scribe_Values.Look<int>(ref cooldown, nameof(cooldown));
        }
    }
}
