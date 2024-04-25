using System.Collections.Generic;
using RimWorld;
using Verse;

#pragma warning disable 01591

namespace Pawnmorph
{

	public class HediffGiver_Esoteric_RandomList : HediffGiver
	{
		public List<HediffDef> hediffDefs;
		public List<HediffDef> hediffDefsComplete;
		public float completeChance;
		private HediffDef hediffDef;
		public float severity = -1f;
		public ChemicalDef toleranceChemical;
		public bool divideByBodySize = false;
		public float mtbDays;
		public bool once = false;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (pawn.health.hediffSet.hediffs.Any(x => hediffDefs.Any(y => y == x.def))) return;

			if (Rand.RangeInclusive(0, 100) <= completeChance)
				hediffDef = hediffDefsComplete.RandomElement();
			else
				hediffDef = hediffDefs.RandomElement();

			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
			float num;

			if (severity > 0f)
				num = severity;
			else
				num = hediffDef.initialSeverity;

			if (divideByBodySize)
				num /= pawn.BodySize;

			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num, false);
			hediff.Severity = num;
			pawn.health.AddHediff(hediff);
		}
	}
}
