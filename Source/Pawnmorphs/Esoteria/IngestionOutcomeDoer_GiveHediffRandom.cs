using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_GiveHediffRandom : IngestionOutcomeDoer_MultipleTfBase
    {
        public float severity = -1f;
        public ChemicalDef toleranceChemical;
        public bool divideByBodySize = false;
        private HediffDef hediffDef;
        private static List<HediffDef> _scratchList = new List<HediffDef>(); 

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
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
                AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num);

            hediff.Severity = num;
            pawn.health.AddHediff(hediff, null, null);
        }
    }
}