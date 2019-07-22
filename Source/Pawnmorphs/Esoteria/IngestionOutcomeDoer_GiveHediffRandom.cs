using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;
using Multiplayer.API;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_GiveHediffRandom : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        private HediffDef hediffDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {
                float completeChance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().partialChance;
                if (Rand.RangeInclusive(0, 100) <= completeChance)
                {
                    hediffDef = hediffDefsComplete.RandomElement();
                }
                else
                {
                    hediffDef = hediffDefs.RandomElement();
                }

                Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, null);
                float num;
                if (severity > 0f)
                {
                    num = severity;
                }
                else
                {
                    num = hediffDef.initialSeverity;
                }
                if (divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref num);
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }
            catch { }
        }
    }

    public class IngestionOutcomeDoer_GiveHediffAll : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {
                foreach (HediffDef h in hediffDefs)
                {

                    Hediff hediff = HediffMaker.MakeHediff(h, pawn, null);
                    float num;
                    if (this.severity > 0f)
                    {
                        num = this.severity;
                    }
                    else
                    {
                        num = h.initialSeverity;
                    }
                    if (this.divideByBodySize)
                    {
                        num /= pawn.BodySize;
                    }
                    AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
                    hediff.Severity = num;
                    pawn.health.AddHediff(hediff, null, null, null);
                }
            }
            catch { }
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

                if (!pawn.health.hediffSet.hediffs.Any(x => hediffDefs.Contains(x.def)))
                {
                    if (Rand.RangeInclusive(0, 100) <= completeChance)
                    {

                        hediffDef = hediffDefsComplete.RandomElement();

                    }
                    else
                    {
                        hediffDef = hediffDefs.RandomElement();

                    }

                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, null);
                    float num;
                    if (this.severity > 0f)
                    {
                        num = this.severity;
                    }
                    else
                    {
                        num = hediffDef.initialSeverity;
                    }
                    if (this.divideByBodySize)
                    {
                        num /= pawn.BodySize;
                    }
                    AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
                    hediff.Severity = num;
                    pawn.health.AddHediff(hediff, null, null, null);
                }


            }
            catch { }
        }
    }
}
