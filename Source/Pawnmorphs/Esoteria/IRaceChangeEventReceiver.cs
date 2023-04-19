// IRaceChangeEventReciever.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:54 AM
// last updated 09/22/2019  11:54 AM

using Verse;

namespace Pawnmorph
{
	/// <summary> Interface for things that receive race change event. </summary>
	public interface IRaceChangeEventReceiver
	{
		/// <summary>
		/// Called when the pawn's race changes.
		/// </summary>
		/// <param name="oldRace">The old race.</param>
		void OnRaceChange(ThingDef oldRace);
	}
}