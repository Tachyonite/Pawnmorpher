using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class HediffGiver_AcceleratedOverrideList : HediffGiver
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance;
        private HediffDef hediffDef;
        private HediffDef newHediffDef;
        public float severity = -1f;
        public ChemicalDef toleranceChemical;
        public bool divideByBodySize = false;
        public float mtbDays;
        public bool once = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            bool pushed=false;
            if (pawn.health.hediffSet.hediffs.Any(x => hediffDefs.Any(y => y == x.def)))
            {

                return;

            }

            if (MP.IsInMultiplayer)
            {
                Rand.PushState(RandUtilities.MPSafeSeed);
                pushed = true;
            }

            if (Rand.RangeInclusive(0, 100) <= completeChance)
            {
                hediffDef = hediffDefsComplete.RandomElement();
            }
            else
            {
                hediffDef = hediffDefs.RandomElement();
            }

            newHediffDef = hediffDef;
            foreach (var hdg in newHediffDef.GetAllHediffGivers().OfType<HediffGiver_Mutation>())
            {
                hdg.mtbDays = 0.1f;
            }
            newHediffDef.CompProps<HediffCompProperties_SeverityPerDay>().severityPerDay = -2f;
            Hediff hediff = HediffMaker.MakeHediff(newHediffDef, pawn, null);

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
            pawn.health.RemoveHediff(cause);
            try
            {
                
            }
            catch
            {
            }
            finally
            {
                if (pushed)
                {
                    Rand.PopState();
                }
            }
        }
    }
}
