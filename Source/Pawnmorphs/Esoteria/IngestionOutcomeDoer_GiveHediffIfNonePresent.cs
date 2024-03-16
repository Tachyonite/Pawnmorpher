using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Interfaces;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// ingestion outcome doer that gives hediffs if none are present 
	/// </summary>
	/// <seealso cref="RimWorld.IngestionOutcomeDoer" />
	public class IngestionOutcomeDoer_GiveHediffIfNonePresent : IngestionOutcomeDoer
	{
		/// <summary>list of partial hediffs to add</summary>
		public List<HediffDef> hediffDefs;
		/// <summary>
		/// list of complete hediffs to add
		/// </summary>
		public List<HediffDef> hediffDefsComplete;
		/// <summary>The chance to add a hediff from the complete list rather then the partial list</summary>
		public float completeChance;
		/// <summary>The severity</summary>
		public float severity = -1f;
		/// <summary>The tolerance chemical</summary>
		public ChemicalDef toleranceChemical;
		/// <summary>if true the starting severity is modified by the pawns body size</summary>
		public bool divideByBodySize = false;

		private List<HediffDef> _scratchList = new List<HediffDef>();
		private HediffDef _hediffDef;

		/// <summary>Does the ingestion outcome special.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			if (!pawn.health.hediffSet.hediffs.Any(x => hediffDefs.Contains(x.def)))
			{
				_scratchList.Clear();

				if (Rand.RangeInclusive(0, 100) <= completeChance)
					_scratchList.AddRange(hediffDefsComplete.Where(h => h.CanInfect(pawn)));
				else
					_scratchList.AddRange(hediffDefs.Where(h => h.CanInfect(pawn)));

				if (_scratchList.Count == 0) return;
				_hediffDef = _scratchList.RandElement();

				Hediff hediff = HediffMaker.MakeHediff(_hediffDef, pawn);
				float num;
				if (severity > 0f)
					num = severity;
				else
					num = _hediffDef.initialSeverity;
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
