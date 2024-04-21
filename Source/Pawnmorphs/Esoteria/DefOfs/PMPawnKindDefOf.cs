// PMPawnKindDefOf.cs created by Iron Wolf for Pawnmorph on 01/05/2021 4:40 PM
// last updated 01/05/2021  4:40 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// 
	/// </summary>
	[DefOf]
	public static class PMPawnKindDefOf
	{

		static PMPawnKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMPawnKindDefOf));
		}

		/// <summary>
		/// The sheep
		/// </summary>
		[NotNull]
		public static PawnKindDef Sheep;
	}
}