using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

//Depricated, use HedifGiver_Mutaions instead.
namespace Pawnmorph
{
    [Obsolete("Use HediffGiver_Mutation")]
    public class HediffGiver_Esoteric_Fixed : HediffGiver
    {
        public float mtbDays;
        public List<BodyPartDef> fixedParts;
        public TaleDef tale;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && PawnmorphHediffGiverUtility.TryApply(pawn, hediff, fixedParts) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                    if (cause.def.HasComp(typeof(HediffComp_Single)))
                    {
                        pawn.health.RemoveHediff(cause);
                    }
                    if (tale != null)
                    {
                        TaleRecorder.RecordTale(tale, new object[]{pawn});
                    }
                }
            }
            catch { }
        }
    }
}
