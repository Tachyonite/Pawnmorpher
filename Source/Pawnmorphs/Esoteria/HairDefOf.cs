// HairDefOf.cs created by Iron Wolf for Pawnmorph on 03/21/2020 7:11 PM
// last updated 03/21/2020  7:11 PM

using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class HairDefOf
	{
		public static HairDef Shaved;

		static HairDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HairDefOf));
		}
	}
}