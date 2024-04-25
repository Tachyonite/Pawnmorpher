// MutagenDefOf.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:07 PM
// last updated 08/13/2019  4:07 PM

using JetBrains.Annotations;
using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class MutagenDefOf
	{
		[NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public static MutagenDef defaultMutagen;
		[NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public static MutagenDef MergeMutagen;

		[NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public static MutagenDef PM_ChamberMutagen;

		[NotNull][UsedImplicitly] public static MutagenDef PM_ChaobulbHarvest;

		[NotNull][UsedImplicitly] public static MutagenDef PM_FalloutMutagen;

		[NotNull][UsedImplicitly] public static MutagenDef PM_MutaniteMutagen;

		// ReSharper disable once NotNullMemberIsNotInitialized
		static MutagenDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MutagenDefOf));
		}
	}
}