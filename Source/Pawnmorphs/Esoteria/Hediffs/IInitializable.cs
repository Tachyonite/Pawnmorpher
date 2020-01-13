// IInitializable.cs modified by Iron Wolf for Pawnmorph on 01/13/2020 5:46 PM
// last updated 01/13/2020  5:46 PM

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// interface for a hediff stage or hediff giver that needs to be initialized 
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Gets all Configuration errors in this instance.
        /// </summary>
        /// <returns></returns>
        [NotNull] IEnumerable<string> ConfigErrors(); 
    }
}