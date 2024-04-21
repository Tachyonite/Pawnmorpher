// TfRelationDefOf.cs modified by Iron Wolf for Pawnmorph on 08/17/2019 11:00 AM
// last updated 08/17/2019  11:00 AM

using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	/// <summary> Static class containing transformation related pawn relation defs. </summary>
	[DefOf]
	public static class TfRelationDefOf
	{
		static TfRelationDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TfRelationDefOf));
		}

		public static PawnRelationDef MergeMate;
		public static PawnRelationDef ExMerged;
	}
}