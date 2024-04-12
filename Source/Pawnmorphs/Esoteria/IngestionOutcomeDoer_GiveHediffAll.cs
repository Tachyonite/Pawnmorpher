using System.Linq;
using Pawnmorph.Interfaces;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// ingestion outcome doer that gives all hediffs 
	/// </summary>
	/// <seealso cref="Pawnmorph.IngestionOutcomeDoer_MultipleTfBase" />
	public class IngestionOutcomeDoer_GiveHediffAll : IngestionOutcomeDoer_MultipleTfBase
	{
		/// <summary>the chance to give the complete tf instead of the partial</summary>
		public float completeChance;
		/// <summary>The severity to set the hediff at</summary>
		public float severity = -1f;
		/// <summary>The tolerance chemical</summary>
		public ChemicalDef toleranceChemical;
		/// <summary>if true, the severity to set by is divided by the pawns body size</summary>
		public bool divideByBodySize = false;
		/// <summary>Does the ingestion outcome special.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			foreach (HediffDef h in AllCompleteDefs.Concat(AllPartialDefs))
			{
				if (!h.CanInfect(pawn)) continue;

				Hediff hediff = HediffMaker.MakeHediff(h, pawn);

				float num;
				if (severity > 0f)
					num = severity;
				else
					num = h.initialSeverity;
				if (divideByBodySize) num /= pawn.BodySize;
				AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num, false);
				hediff.Severity = num;

				if (hediff is ICaused caused)
					caused.Causes.TryAddCause(string.Empty, ingested.def);

				pawn.health.AddHediff(hediff, null, null);
			}
		}
	}
}
