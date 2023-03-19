// IMentalStateRecoveryReciever.cs created by Iron Wolf for Pawnmorph on 03/03/2020 5:52 PM
// last updated 03/03/2020  5:53 PM

using JetBrains.Annotations;
using Verse.AI;

namespace Pawnmorph
{
	/// <summary>
	/// interface for things that receive notifications when a pawn recovers from a mental state 
	/// </summary>
	public interface IMentalStateRecoveryReceiver
	{
		/// <summary>
		/// Called when the pawn recovered from the given mental state.
		/// </summary>
		/// <param name="mentalState">State of the mental.</param>
		void OnRecoveredFromMentalState([NotNull] MentalState mentalState);
	}
}