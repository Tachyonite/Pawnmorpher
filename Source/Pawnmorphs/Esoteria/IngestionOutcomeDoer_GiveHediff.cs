// IngestionOutcomeDoer_GiveHediff.cs created by Iron Wolf for Pawnmorph on 08/12/2021 9:10 AM
// last updated 08/12/2021  9:10 AM

using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// ingestion outcome doer to add a simple hediff ignoring body size 
	/// </summary>
	/// <seealso cref="RimWorld.IngestionOutcomeDoer" />
	public class IngestionOutcomeDoer_GiveHediff : IngestionOutcomeDoer
	{

		/// <summary>The hediff</summary>
		public HediffDef hediffDef;

		/// <summary>The initial severity</summary>
		public float severity = -1;



		/// <summary>Does the ingestion outcome special.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			severity = severity < 0 ? hediffDef.initialSeverity : severity;
			var h = HediffMaker.MakeHediff(hediffDef, pawn);
			h.Severity = severity;

			MutationCauses causes = null;
			if (h is Hediff_MutagenicBase mutagenicBase)
				causes = mutagenicBase.Causes;
			else if (h is Hediff_AddedMutation mutation)
				causes = mutation.Causes;

			pawn?.health?.AddHediff(h);

			if (causes != null)
				causes.TryAddCause(string.Empty, ingested.def);
		}

	}
}