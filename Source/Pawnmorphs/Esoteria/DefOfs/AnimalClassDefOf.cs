// AnimalClassDefOf.cs modified by Iron Wolf for Pawnmorph on 01/10/2020 5:25 PM
// last updated 01/10/2020  5:25 PM

using RimWorld;

#pragma warning disable 1591

// ReSharper disable NotNullMemberIsNotInitialized

namespace Pawnmorph
{
	/// <summary>
	///     def of for animal classifications
	/// </summary>
	[DefOf]
	public static class AnimalClassDefOf
	{
		static AnimalClassDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(AnimalClassDefOf));
		}

		public static AnimalClassDef Animal;

		public static AnimalClassDef Canid;
		public static AnimalClassDef Reptile;
		public static AnimalClassDef Bovid;
		public static AnimalClassDef Cervid;

		public static AnimalClassDef Powerful;
	}
}