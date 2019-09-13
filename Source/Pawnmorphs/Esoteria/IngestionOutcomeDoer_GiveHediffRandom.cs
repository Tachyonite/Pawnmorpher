using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_GiveHediffRandom : IngestionOutcomeDoer_MultipleTfBase
    {
        private HediffDef hediffDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

         
        private static List<HediffDef> _scratchList = new List<HediffDef>(); 
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
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
            catch
            {
            }
        }
    }

    public class IngestionOutcomeDoer_GiveHediffAll : IngestionOutcomeDoer_MultipleTfBase
    {
        public float completeChance;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {
                StringBuilder builder = new StringBuilder(); 
                foreach (HediffDef h in AllCompleteDefs.Concat(AllPartialDefs))
                {
                    if(!h.CanInfect(pawn)) continue;
                    builder.AppendLine($"adding {h.defName}");

                    Hediff hediff = HediffMaker.MakeHediff(h, pawn);

                    float num;
                    if (severity > 0f)
                        num = severity;
                    else
                        num = h.initialSeverity;
                    if (divideByBodySize) num /= pawn.BodySize;
                    AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num);
                    hediff.Severity = num;
                    pawn.health.AddHediff(hediff, null, null);
                }
                Log.Message(builder.ToString());
            }
            catch
            {
            }
        }
    }


    public class IngestionOutcomeDoer_GiveHediffIfNonePresent : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance;
        private HediffDef hediffDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;
        private List<HediffDef> _scratchList = new List<HediffDef>(); 
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {
                if (!pawn.health.hediffSet.hediffs.Any(x => hediffDefs.Contains(x.def)))
                {
                    _scratchList.Clear();

                    if (Rand.RangeInclusive(0, 100) <= completeChance)
                        _scratchList.AddRange(hediffDefsComplete.Where(h => h.CanInfect(pawn)));
                    else
                        _scratchList.AddRange( hediffDefs.Where(h => h.CanInfect(pawn)));

                    if (_scratchList.Count == 0) return; 
                    hediffDef = _scratchList.RandElement(); 

                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    float num;
                    if (severity > 0f)
                        num = severity;
                    else
                        num = hediffDef.initialSeverity;
                    if (divideByBodySize) num /= pawn.BodySize;
                    AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num);
                    hediff.Severity = num;
                    pawn.health.AddHediff(hediff, null, null);
                }
            }
            catch
            {
            }
        }
    }
}