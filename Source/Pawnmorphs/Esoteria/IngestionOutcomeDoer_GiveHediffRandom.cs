using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Interfaces;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// ingestion outcome doer that adds a random hediff 
	/// </summary>
	/// <seealso cref="Pawnmorph.IngestionOutcomeDoer_MultipleTfBase" />
	public class IngestionOutcomeDoer_GiveHediffRandom : IngestionOutcomeDoer_MultipleTfBase
	{
		/// <summary>The severity</summary>
		public float severity = -1f;
		/// <summary>The tolerance chemical</summary>
		public ChemicalDef toleranceChemical;
		/// <summary>The divide by body size</summary>
		public bool divideByBodySize = false;

		private HediffDef hediffDef;
		private static List<HediffDef> _scratchList = new List<HediffDef>();
		/// <summary>Does the ingestion outcome special.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			float completeChance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().partialChance;
			_scratchList.Clear();

			if (Rand.RangeInclusive(0, 100) <= completeChance)
				_scratchList.AddRange(AllCompleteDefs.Where(h => h.CanInfect(pawn)));
			else
				_scratchList.AddRange(AllPartialDefs.Where(h => h.CanInfect(pawn)));

			if (_scratchList.Count == 0) return;
			hediffDef = _scratchList.RandElement();

			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
			float num;
			if (severity > 0f)
				num = severity;
			else
				num = hediffDef.initialSeverity;

			if (divideByBodySize)
				AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num, false);

			hediff.Severity = num;


			if (hediff is ICaused caused)
				caused.Causes.TryAddCause(string.Empty, ingested.def);

			pawn.health.AddHediff(hediff, null, null);
		}
	}
}