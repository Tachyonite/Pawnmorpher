using System;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Interface for all hediffs that can possibly cause mutations
    /// </summary>
    public interface IMutationHediff
    {
        /// <summary>
        /// Whether or not this hediff is currently blocking race checks
        /// </summary>
        /// <value><c>true</c> if blocks race check; otherwise, <c>false</c>.</value>
        bool BlocksRaceCheck { get; }
    }
}
