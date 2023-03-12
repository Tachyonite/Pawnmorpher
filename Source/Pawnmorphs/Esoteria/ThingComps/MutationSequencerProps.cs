// MutationSequencerProps.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 11/14/2020  8:32 AM

using RimWorld;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// properties for the sequencer comp 
	/// </summary>
	/// <seealso cref="RimWorld.CompProperties_Scanner" />
	public class MutationSequencerProps : CompProperties_Scanner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MutationSequencerProps"/> class.
		/// </summary>
		public MutationSequencerProps()
		{
			compClass = typeof(MutationSequencerComp);
		}

	}
}