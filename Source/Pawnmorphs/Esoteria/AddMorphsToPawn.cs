using Verse;

namespace Pawnmorph
{
    public class AddMorphsToPawn
    {
        public Pawn GetMorphsForPawnKind(Pawn pawn, PawnKindDef pawnKind)
        {
            /*
            if (MP.IsInMultiplayer)
            {
                Rand.PushState(RandUtilities.MPSafeSeed);
            }

            if (pawnKind.HasModExtension<MorphPawnKindExtension>()){

                MorphPawnKindExtension pKE = pawnKind.GetModExtension<MorphPawnKindExtension>();
                IEnumerable<MorphDef> mcd = MorphCategoryDefOf.Combat.AllMorphsInCategories;

                for (var i = 0; i < pKE.maxHediffs; i++)
                {
                    HediffDef hediff = mcd.RandomElement();
                    

                }

            }
            */
            return pawn;
        }
    }
}
