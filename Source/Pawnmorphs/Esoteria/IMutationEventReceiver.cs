// IMutationEventReceiver .cs created by Iron Wolf for Pawnmorph on 09/14/2019 8:28 AM
// last updated 09/14/2019  8:28 AM

namespace Pawnmorph
{
    /// <summary>
    /// interface for thing comps that want to receive events when the pawn gains or loses mutation 
    /// </summary>
    public interface IMutationEventReceiver
    {
        void MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker);

        void MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker); 

    }

}