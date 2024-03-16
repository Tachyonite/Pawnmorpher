// AnimalClassDefOf.cs modified by Iron Wolf for Pawnmorph on 01/10/2020 5:25 PM
// last updated 01/10/2020  5:25 PM

using RimWorld;
using Verse;

#pragma warning disable 1591

// ReSharper disable NotNullMemberIsNotInitialized

namespace Pawnmorph.DefOfs
{
	/// <summary>
	///     def of for body parts
	/// </summary>
	[DefOf]
	public static class PM_BodyPartDefOf
	{
		static PM_BodyPartDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartDefOf));
		}

		public static BodyPartDef Jaw;
		public static BodyPartDef Head;
		public static BodyPartDef Eye;
		public static BodyPartDef Hand;
	}
}