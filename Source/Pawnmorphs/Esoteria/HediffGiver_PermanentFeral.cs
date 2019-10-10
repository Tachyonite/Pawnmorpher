using System.Linq;
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
            if (TryApply(pawn, null))
            {
                var loader = Find.World.GetComponent<PawnmorphGameComp>();
                var inst = loader.GetTransformedPawnContaining(pawn)?.First;

                foreach (var instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>())
                {
                    ReactionsHelper.OnPawnPermFeral(instOriginalPawn, pawn); 
                }

                //remove the original and destroy the pawns 
                foreach (var instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>())
                {
                    instOriginalPawn.Destroy();
                }

                if (inst != null)
                {
                    loader.RemoveInstance(inst); 
                }

                if(inst != null || pawn.Faction == Faction.OfPlayer)
                    Find.LetterStack.ReceiveLetter("LetterHediffFromPermanentTFLabel".Translate(pawn.LabelShort).CapitalizeFirst(), "LetterHediffFromPermanentTF".Translate(pawn.LabelShort).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn, null, null);
            }
        }
    }
}
