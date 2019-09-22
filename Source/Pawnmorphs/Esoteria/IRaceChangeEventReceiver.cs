// IRaceChangeEventReciever.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:54 AM
// last updated 09/22/2019  11:54 AM

using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// interface for things that receive race change event
    /// </summary>
    public interface IRaceChangeEventReceiver
    {
        void OnRaceChange(ThingDef oldRace); 
    }
}