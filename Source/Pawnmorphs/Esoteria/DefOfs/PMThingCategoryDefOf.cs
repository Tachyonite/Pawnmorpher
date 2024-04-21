// PMThingCategoryDefOf.cs modified by Iron Wolf for Pawnmorph on 01/22/2020 5:43 PM
// last updated 01/22/2020  5:43 PM

using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class PMThingCategoryDefOf
	{
		public static ThingCategoryDef Injector;
		public static ThingCategoryDef RawMutagen;
		public static ThingCategoryDef Serum;
		public static ThingCategoryDef Mutapill;

        /// <summary> Not in base ThingCategoryDefOf </summary>
        public static ThingCategoryDef Textiles;


		public static ThingCategoryDef PM_MutationGenome;
	}
}