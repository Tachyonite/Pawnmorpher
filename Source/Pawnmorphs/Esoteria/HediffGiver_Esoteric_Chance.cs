using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class HediffGiver_Esoteric_Chance : HediffGiver
    {
        public float mtbDays;
        public int chance = 50;
        private bool triggered = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if ((Rand.RangeInclusive(0, 100) <= chance && !triggered) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
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
                else
                {
                    triggered = true;
                }
            }
            catch
            {
            }
        }
    }
}
