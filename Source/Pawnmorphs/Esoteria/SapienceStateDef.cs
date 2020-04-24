// SapienceStateDef.cs created by Iron Wolf for Pawnmorph on 04/24/2020 7:35 AM
// last updated 04/24/2020  7:35 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def for specific state a pawns 'sapience/mind' can be in, such as FormerHuman, Animalistic, ect.
    /// </summary>
    /// <seealso cref="Verse.Def" />
    public class SapienceStateDef : Def
    {
        /// <summary>
        /// The state type
        /// </summary>
        public Type stateType;

        /// <summary>
        /// Creates a new state instance.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public SapienceState CreateState()
        {
            return (SapienceState) Activator.CreateInstance(stateType); 
        }

        /// <summary>
        /// Gets all configuration errors with this instance 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError; 
            }

            if (stateType == null) yield return "no sapience type set!"; 
            else if (!typeof(SapienceState).IsAssignableFrom(stateType))
                yield return $"{stateType.Name} is not a subtype of {nameof(SapienceState)}!"; 

        }

    }
}