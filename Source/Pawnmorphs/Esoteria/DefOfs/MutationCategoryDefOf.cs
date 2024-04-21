// MutationCategoryDefOf.cs created by Iron Wolf for Pawnmorph on 08/03/2020 4:55 PM
// last updated 08/03/2020  4:55 PM

using RimWorld;

namespace Pawnmorph
{
	/// <summary>
	/// static class that contains various Mutation Category defs 
	/// </summary>
	[DefOf]
	public static class MutationCategoryDefOf
	{
		static MutationCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MutationCategoryDefOf));
		}
	}
}