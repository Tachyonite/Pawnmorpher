// SapienceStateDefOf.cs created by Iron Wolf for Pawnmorph on 04/25/2020 11:17 AM
// last updated 04/25/2020  11:17 AM

using JetBrains.Annotations;
using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class SapienceStateDefOf
	{
		static SapienceStateDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SapienceStateDefOf));
		}

		[NotNull]

		public static SapienceStateDef FormerHuman;

		[NotNull]
		public static SapienceStateDef Animalistic;

		[NotNull]
		public static SapienceStateDef MergedPawn;
	}
}