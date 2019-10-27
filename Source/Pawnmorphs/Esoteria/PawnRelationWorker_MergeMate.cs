using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class PawnRelationWorker_MergeMate : PawnRelationWorker
    {
        public PawnRelationDef MergeMate = DefDatabase<PawnRelationDef>.GetNamed("MergeMate");

        public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, ex: true) * BaseGenerationChanceFactor(generated, other, request);
        }

        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            generated.relations.AddDirectRelation(MergeMate, other);
        }
    }
}
