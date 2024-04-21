// PMSoundDefOf.cs created by Iron Wolf for Pawnmorph on 10/21/2021 7:26 AM
// last updated 10/21/2021  7:26 AM

using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static def of class for common SoundDefs 
	/// </summary>
	[DefOf]
	public static class PMSoundDefOf
	{
		static PMSoundDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMSoundDefOf));
		}
		/// <summary>
		/// sound of using an injector 
		/// </summary>
		public static SoundDef Ingest_Inject;

		/// <summary>
		/// The recipe cook meal sound 
		/// </summary>
		public static SoundDef Recipe_CookMeal;

	}
}