using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// Getter for the mutanite value of items.
	/// </summary>
	public class IngredientValueGetter_Mutanite : IngredientValueGetter
	{
		/// <summary>
		/// Get the mutanite value of an item.
		/// </summary>
		/// <param name="t">The item.</param>
		/// <returns>The value.</returns>
		public override float ValuePerUnitOf(ThingDef t)
		{
			return t.GetStatValueAbstract(PMStatDefOf.MutaniteConcentration);
		}

		/// <summary>
		/// Description for the ingredients required for a recipe.
		/// </summary>
		/// <param name="r">The recipe.</param>
		/// <param name="ing">The count of ingredients.</param>
		/// <returns>The description</returns>
		public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
		{
			return "BillRequiresMutanite".Translate(ing.GetBaseCount().ToStringPercent());
		}
	}
}
