using System;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// obsolete and should not be used 
    /// </summary>
    [Obsolete("use HediffGiver_Mutation")]
    public class HediffGiver_Esoteric_GenderChance : HediffGiver
    {
#pragma warning disable 1591
        public float mtbDays;
        public Gender gender;
        public int chance;
        private bool triggered = false;



        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (pawn.gender == gender || (Rand.RangeInclusive(0, 100) <= chance && !triggered) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && base.TryApply(pawn, null))
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                        if (cause.def.HasComp(typeof(HediffComp_Single)))
                        {
                            pawn.health.RemoveHediff(cause);
                        }
                    }
                }
                else {
                    triggered = false;
                }
            }
            catch
            {
            }
        }
#pragma warning restore
    }
}
