// HediffGiver_Mutation.cs modified by Iron Wolf for Pawnmorph on 08/07/2019 10:19 AM
// last updated 08/07/2019  10:19 AM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class HediffGiver_Mutation : HediffGiver
    {
        public float mtbDays;
        public Gender gender;
        public int chance = 100;
        public TaleDef tale;
        private Dictionary<Hediff, bool> triggered = new Dictionary<Hediff, bool>();

        public new bool TryApply(Pawn pawn, List<Hediff> outAddedHediffs = null)
        {
            return PawnmorphHediffGiverUtility.TryApply(pawn, hediff, partsToAffect, canAffectAnyLivePart, countToAffect, outAddedHediffs);
        }
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (Rand.MTBEventOccurs(mtbDays, 60000f, 30f) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
                if ((gender == pawn.gender || (!triggered.TryGetValue(cause, false) && Rand.RangeInclusive(0, 100) <= chance)) && TryApply(pawn))
                {
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                    triggered[cause] = true;
                    
                    if (tale != null)
                    {
                        TaleRecorder.RecordTale(tale, new object[] { pawn });
                    }
                }
                else
                {
                    triggered[cause] = true;
                }

                if (cause.def.HasComp(typeof(HediffComp_Single))) //should either be given or triggered 
                {
                    pawn.health.RemoveHediff(cause);
                }
            }
        }
    }
}