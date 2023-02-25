// Chaothrumbo.cs created by Iron Wolf for Pawnmorph on 06/26/2021 10:09 AM
// last updated 06/26/2021  10:09 AM

using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph.Things
{
	/// <summary>
	/// class for the chaothrumbo with observation thoughts 
	/// </summary>
	/// <seealso cref="Verse.Pawn" />
	public class Chaothrumbo : Pawn, IObservedThoughtGiver
	{
		private static ThoughtDef _observationDef;


		/// <summary>
		/// Gives the observed thought.
		/// </summary>
		/// <returns></returns>
		public Thought_Memory GiveObservedThought()
		{
			var mem = (Memory_FactionObservation)ThoughtMaker.MakeThought(ObservationDef);

			mem.ObservedThing = this;
			return mem;

		}


		private static ThoughtDef ObservationDef
		{
			get
			{
				if (_observationDef == null) _observationDef = DefDatabase<ThoughtDef>.GetNamed("PM_CThrumboAmbient");

				return _observationDef;
			}
		}

		/// <summary>
		/// Gives the observed thought.
		/// </summary>
		/// <param name="observer">The observer.</param>
		/// <returns></returns>
		public Thought_Memory GiveObservedThought(Pawn observer)
		{
			var mem = (Memory_FactionObservation)ThoughtMaker.MakeThought(ObservationDef); //Note: we can separate out the different memories and get rid of the special memory 
																						   //this may not work without further patching to PawnObserver.ObserveSurroundingThings() 
			mem.ObservedThing = this;
			return mem;
		}

		/// <summary>
		/// Gives the observed history event.
		/// </summary>
		/// <param name="observer">The observer.</param>
		/// <returns></returns>
		public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
		{
			return null; //TODO figure out wtf HistoryEventDefs are 
		}
	}
}