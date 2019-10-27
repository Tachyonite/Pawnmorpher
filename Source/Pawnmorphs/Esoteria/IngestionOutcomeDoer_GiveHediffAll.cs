using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_GiveHediffAll : IngestionOutcomeDoer_MultipleTfBase
    {
        public float completeChance;
        public float severity = -1f;
        public ChemicalDef toleranceChemical;
        public bool divideByBodySize = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            StringBuilder builder = new StringBuilder();
            foreach (HediffDef h in AllCompleteDefs.Concat(AllPartialDefs))
            {
                if (!h.CanInfect(pawn)) continue;
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
    }
}
