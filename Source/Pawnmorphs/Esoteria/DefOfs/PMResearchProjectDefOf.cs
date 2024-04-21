// PMResearchProjectDefOf.cs created by Iron Wolf for Pawnmorph on 09/15/2021 6:57 AM
// last updated 09/15/2021  6:57 AM

using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static def of class containing research project defs
	/// </summary>
	[DefOf]
	public static class PMResearchProjectDefOf
	{
		static PMResearchProjectDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMResearchProjectDefOf));
		}

		/// <summary>
		/// The injectors research 
		/// </summary>
		public static ResearchProjectDef Injectors;


	}
}