using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class HediffGiver_TF : HediffGiver
    {
        public List<PawnKindDef> pawnkinds; // The pawnKind of the animal to be transformed into.
        public TaleDef tale; // Tale to add to the tales.
        public TFGender forceGender = TFGender.Original; // The gender that will be forced (i.e. a ChookMorph will be forced female).
        public float forceGenderChance = 50f; // If forceGender is provided, this is the chance the gender will be forced.
        private bool triggered = false; // A flag to prevent us from checking endlessly.

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        // Whenever the timer expires.
        {
            if (!triggered && Rand.RangeInclusive(0, 100) <= LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance)
            {
                TransformerUtility.Transform(pawn, cause, hediff, pawnkinds, tale, forceGender, forceGenderChance);
            }
            else
            {
                triggered = true;
            }
        }
    }
}
