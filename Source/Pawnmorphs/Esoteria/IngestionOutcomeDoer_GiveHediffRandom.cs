using System.Collections.Generic;
using Multiplayer.API;
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


        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {
                if (MP.IsInMultiplayer)
                {
                    Rand.PushState(RandUtilities.MPSafeSeed);
                }

                float completeChance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().partialChance;
                if (Rand.RangeInclusive(0, 100) <= completeChance)
                    hediffDef = AllCompleteDefs.RandElement();
                else
                    hediffDef = AllPartialDefs.RandElement(); //use randElement as that doesn't make an extra copy

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
            catch
            {
            }
            finally
            {
                if (MP.IsInMultiplayer)
                {
                    Rand.PopState();
                }
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
                foreach (HediffDef h in AllCompleteDefs)
                {
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

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {
                if (MP.IsInMultiplayer)
                {
                    Rand.PushState(RandUtilities.MPSafeSeed);
                }

                if (!pawn.health.hediffSet.hediffs.Any(x => hediffDefs.Contains(x.def)))
                {
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
                    if (divideByBodySize) num /= pawn.BodySize;
                    AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num);
                    hediff.Severity = num;
                    pawn.health.AddHediff(hediff, null, null);
                }
            }
            catch
            {
            }
            finally
            {
                if (MP.IsInMultiplayer)
                {
                    Rand.PopState();
                }
            }
        }
    }
}