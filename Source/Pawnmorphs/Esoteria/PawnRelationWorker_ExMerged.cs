using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// the relationship worker for the "ExMerged" relationship 
	/// </summary>
	/// <seealso cref="RimWorld.PawnRelationWorker" />
	public class PawnRelationWorker_ExMerged : PawnRelationWorker
	{
		/// <summary>The merge mate relationship def</summary>
		public PawnRelationDef MergeMate = DefDatabase<PawnRelationDef>.GetNamed("ExMerged");

		/// <summary>the chance for two pawns to be given this relationship</summary>
		/// <param name="generated">The generated.</param>
		/// <param name="other">The other.</param>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, ex: true) * BaseGenerationChanceFactor(generated, other, request);
		}

		/// <summary>Creates the relationship between the 2 given pawns</summary>
		/// <param name="generated">The generated.</param>
		/// <param name="other">The other.</param>
		/// <param name="request">The request.</param>
		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			generated.relations.AddDirectRelation(MergeMate, other);
		}
	}
}
