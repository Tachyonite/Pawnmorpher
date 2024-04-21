// AspectDefOf.cs modified by Iron Wolf for Pawnmorph on 09/29/2019 12:58 PM
// last updated 09/29/2019  12:58 PM

using JetBrains.Annotations;
using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	/// <summary> DefOf class for commonly referenced Aspects. </summary>
	[DefOf]
	public static class AspectDefOf
	{
		/// <summary>
		/// aspect that represents the pawns 'EtherState'
		/// </summary>
		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef EtherState;

		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef MutagenInfused;

		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef PlantAffinity;

		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef RareMutant;

		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef PrimalWish;

		[NotNull]
		public static AspectDef SplitMind;

		// ReSharper disable once NotNullMemberIsNotInitialized
		static AspectDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(AspectDefOf));
		}
	}
}