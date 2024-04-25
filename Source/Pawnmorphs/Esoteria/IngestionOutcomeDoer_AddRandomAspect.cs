// IngestionOutcomeDooer_Productive.cs modified by Iron Wolf for Pawnmorph on 10/05/2019 1:04 PM
// last updated 10/05/2019  1:04 PM

using System.Collections.Generic;
using Pawnmorph.Aspects;
using RimWorld;
using Verse;
using static Pawnmorph.Aspects.RandomGiver;

namespace Pawnmorph
{
	/// <summary>
	/// ingestion out come doer that adds an aspect to a pawn
	/// </summary>
	/// <seealso cref="RimWorld.IngestionOutcomeDoer" />
	public class IngestionOutcomeDoer_AddRandomAspect : IngestionOutcomeDoer
	{
		/// <summary>The aspects to add</summary>
		public List<Entry> entries = new List<Entry>();

		/// <summary>Does the ingestion outcome special.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			if (entries.Count == 0)
				return;

			var giver = new RandomGiver();
			giver.entries = entries;
			giver.GiveOneAspect(pawn);
		}
	}
}
