// BackstoryDefOf.cs modified by Iron Wolf for Pawnmorph on 11/29/2019 7:35 AM
// last updated 11/29/2019  7:35 AM

using JetBrains.Annotations;
using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class PMStyleDefOf
	{
		[NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public static BeardDef PM_BeardHidden;

		[NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public static HairDef PM_HairHidden;


		static PMStyleDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMStyleDefOf));
		}
	}
}