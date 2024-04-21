// PMNeedDefOf.cs created by Iron Wolf for Pawnmorph on 04/22/2020 5:40 PM
// last updated 04/22/2020  5:40 PM

using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class PMNeedDefOf
	{
		public static NeedDef SapientAnimalControl;

		public static NeedDef Beauty;

		public static NeedDef Joy;

		public static NeedDef Comfort;

		public static NeedDef Mood;

		static PMNeedDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMNeedDefOf));
		}
	}
}