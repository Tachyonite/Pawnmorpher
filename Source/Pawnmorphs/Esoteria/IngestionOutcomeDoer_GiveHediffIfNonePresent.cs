using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_GiveHediffIfNonePresent : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance;
        public float severity = -1f;
        public ChemicalDef toleranceChemical;
        public bool divideByBodySize = false;

        private List<HediffDef> _scratchList = new List<HediffDef>();
        private HediffDef _hediffDef;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
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
                AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num);
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null);
            }
        }
    }
}
