// IngestionOutcomeDooer_Productive.cs modified by Iron Wolf for Pawnmorph on 10/05/2019 1:04 PM
// last updated 10/05/2019  1:04 PM

using RimWorld;
using Verse;

namespace Pawnmorph
{


	/// <summary>
	/// ingestion out come doer that adds an aspect to a pawn
	/// </summary>
	/// <seealso cref="RimWorld.IngestionOutcomeDoer" />
	public class IngestionOutcomeDoer_RemoveAspects : IngestionOutcomeDoer
	{
		/// <summary>Does the ingestion outcome special.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			var aspectT = pawn.GetAspectTracker();
			if (aspectT == null) return;

			foreach (Aspect aspect in aspectT)
				if (aspect.def.removedByReverter)
					aspectT.Remove(aspect); // It's ok to remove them in a foreach loop.


		}
	}
}
