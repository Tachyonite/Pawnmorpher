// PMTraitDefOf.cs created by Iron Wolf for Pawnmorph on 09/16/2019 12:48 PM
// last updated 09/16/2019  12:48 PM

using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	/// <summary> Static class containing references to commonly used Traits. </summary>
	[DefOf]
	public static class PMTraitDefOf
	{
		static PMTraitDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMTraitDefOf));
		}

		public static TraitDef MutationAffinity;
		public static TraitDef PM_PridefulTrait;

		public static TraitDef Cannibal;
	}
}