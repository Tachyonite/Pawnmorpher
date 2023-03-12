// PMEffectDefOf.cs created by Iron Wolf for Pawnmorph on 10/21/2021 7:44 AM
// last updated 10/21/2021  7:44 AM

using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static class for commonly used effects 
	/// </summary>
	[DefOf]
	public static class PMEffecterDefOf
	{
		static PMEffecterDefOf() { DefOfHelper.EnsureInitializedInCtor(typeof(PMEffecterDefOf)); }

		/// <summary>
		/// The EffectorDef for cooking
		/// </summary>
		public static EffecterDef Cook;
	}

}