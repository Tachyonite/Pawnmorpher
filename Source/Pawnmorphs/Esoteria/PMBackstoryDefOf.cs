// PMBackstoryDefOf.cs created by Iron Wolf for Pawnmorph on 01/05/2021 4:49 PM
// last updated 01/05/2021  4:49 PM

using JetBrains.Annotations;
using RimWorld;

namespace Pawnmorph
{
	/// <summary>
	/// 
	/// </summary>
	[DefOf]
	public static class PMBackstoryDefOf
	{
		// ReSharper disable once NotNullMemberIsNotInitialized
		static PMBackstoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMBackstoryDefOf));
		}

		/// <summary>
		/// The pm sheep chef
		/// </summary>
		[NotNull]
		public static BackstoryDef PM_SheepChef;
	}
}