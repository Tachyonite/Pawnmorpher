// HThoughtDefOf.cs modified by Iron Wolf for Pawnmorph on 08/03/2019 3:09 PM
// last updated 08/03/2019  3:09 PM

using RimWorld;

#pragma warning disable 01591
namespace Pawnmorph.Hybrids
{
	/// <summary>
	/// static container for hybrid related thought defs
	/// </summary>
	[DefOf]
	public static class HThoughtDefOf
	{
		//food thoughts 
		public static ThoughtDef MorphAteAnimalMeatDirect;
		public static ThoughtDef MorphAteAnimalMeatAsIngredient;

		static HThoughtDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HThoughtDefOf));
		}
	}
}