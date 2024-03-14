// IngestionOutcomeDoer_BoostSeverity.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/11/2019 2:38 PM
// last updated 09/11/2019  2:38 PM

using System;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// ingestion outcome doer for adding severity to specific hediffs 
	/// </summary>
	public class IngestionOutcomeDoer_BoostSeverity : IngestionOutcomeDoer
	{
		/// <summary>filter for hediffs to boost severity about</summary>
		public Filter<HediffDef> hediffFilter = new Filter<HediffDef>();
		/// <summary>filter for hediff types to boost severity for</summary>
		public Filter<Type> hediffTypes = new Filter<Type>();
		///if a hediff must pass through all filters, otherwise they must pass through any filter 
		public bool mustPassAll;
		/// <summary>The severity to add</summary>
		public float severityToAdd;
		bool PassesFilters(Hediff hediff)
		{
			var pass = hediffFilter.PassesFilter(hediff.def);
			if (pass && !mustPassAll) return true;
			if (mustPassAll && !pass) return false;
			return hediffTypes.PassesFilter(hediff.GetType());
		}

		/// <summary>Does the ingestion outcome special.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			var hediffs = pawn.health.hediffSet.hediffs.Where(PassesFilters);
			foreach (var hediff in hediffs)
			{
				hediff.Severity += severityToAdd;
			}
		}
	}
}