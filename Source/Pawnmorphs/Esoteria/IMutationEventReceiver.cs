// IMutationEventReceiver .cs created by Iron Wolf for Pawnmorph on 09/14/2019 8:28 AM
// last updated 09/14/2019  8:28 AM

namespace Pawnmorph
{
    /// <summary> Interface for thing comps that want to receive events when the pawn gains or loses mutation. </summary>
    public interface IMutationEventReceiver
    {
        /// <summary>called when a mutation is added</summary>
        /// <param name="mutation">The mutation.</param>
        /// <param name="tracker">The tracker.</param>
        void MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker);
        /// <summary>called when a mutation is removed</summary>
        /// <param name="mutation">The mutation.</param>
        /// <param name="tracker">The tracker.</param>
        void MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker);
    }
}