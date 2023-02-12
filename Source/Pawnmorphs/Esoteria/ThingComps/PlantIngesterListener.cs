// PlantIngesterListener.cs created by Iron Wolf for Pawnmorph on 08/16/2021 5:07 PM
// last updated 08/16/2021  5:07 PM

using Verse;
using static RimWorld.HistoryEventArgsNames;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// comp that listens for when a plant or tree is consumed to send the former human grazed event 
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class PlantIngesterListener : ThingComp
	{
		/// <summary>
		/// called after the parent is ingested
		/// </summary>
		/// <param name="ingester">The ingester.</param>
		public override void PostIngested(Pawn ingester)
		{
			base.PostIngested(ingester);
			if (ingester?.IsFormerHuman() == true) //only send event if ingester is a former human, cut down on spam 
				PMHistoryEventDefOf.FormerHumanGrazed.SendEvent(ingester.Named(Doer), parent.Named(Victim)); //should there be one for morphs to? 

		}
	}
}