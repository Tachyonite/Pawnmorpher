using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Thoughts;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class HediffGiver_PermanentFeral : HediffGiver
    {
        public float mtbDays;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (base.TryApply(pawn, null))
            {
                var loader = Find.World.GetComponent<PawnmorphGameComp>();
                var pm = loader.retrieve(pawn);
                var original = pm?.origin;
                if (original != null)
                {
                    ReactionsHelper.OnPawnPermFeral(original, pawn);
                }



                Find.LetterStack.ReceiveLetter("LetterHediffFromPermanentTFLabel".Translate(pawn.LabelShort).CapitalizeFirst(), "LetterHediffFromPermanentTF".Translate(pawn.LabelShort).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn, null, null);
            }
        }
    }
}
