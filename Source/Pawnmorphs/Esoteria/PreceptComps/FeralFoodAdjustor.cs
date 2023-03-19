// FeralFoodAdjustor.cs created by Iron Wolf for Pawnmorph on 08/16/2021 6:19 PM
// last updated 08/16/2021  6:19 PM

using RimWorld;
using Verse;

namespace Pawnmorph.PreceptComps
{
	/// <summary>
	///     precept comp that adjusts a former human's preference to their feral counterpart
	/// </summary>
	/// <seealso cref="RimWorld.PreceptComp" />
	public class FeralFoodAdjustor : PreceptComp, IFoodPreferenceAdjustor
	{
		/// <summary>
		/// Gets the priority.
		/// </summary>
		/// used for determining if multiple adjustors are present. 'lower' values override higher ones 
		/// <value>
		/// The priority.
		/// </value>
		public int Priority => 1;

		/// <summary>
		///     Adjusts the preferability.
		/// </summary>
		/// <param name="eater">The eater.</param>
		/// <param name="thingToEat">The thing to eat.</param>
		/// <returns>
		///     the preferability,  null if no adjustment is needed
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public FoodPreferability? AdjustPreferability(Pawn eater, Thing thingToEat)
		{
			return AdjustPreferability(eater, thingToEat.def.ingestible?.foodType ?? 0);
		}

		/// <summary>
		///     Adjusts the preferability.
		/// </summary>
		/// <param name="eater">The eater.</param>
		/// <param name="foodType">Type of the food.</param>
		/// <returns>null if no adjustment is needed</returns>
		public FoodPreferability? AdjustPreferability(Pawn eater, FoodTypeFlags foodType)
		{
			if (!eater.IsFormerHuman()) return null;

			FoodTypeFlags eaterPref = eater.RaceProps.foodType;
			if ((foodType & FoodTypeFlags.Corpse) != 0 && (eaterPref & FoodTypeFlags.Corpse) != 0)
				return FoodPreferability.RawTasty;

			FoodTypeFlags grazeTag = FoodTypeFlags.Plant | FoodTypeFlags.DendrovoreAnimal | FoodTypeFlags.Seed;
			if ((foodType & grazeTag & eaterPref) != 0) return FoodPreferability.RawTasty;

			return null;
		}

		/// <summary>
		/// get the minimum hunger level to hunt for the given eater.
		/// </summary>
		/// note this will not make a pawn that doesn't normally hunt, hunt 
		/// <param name="eater">The eater.</param>
		/// <returns>the hunger level the eater will hunt at, null if they shouldn't or use the default level </returns>
		public HungerCategory? MinHungerToHunt(Pawn eater)
		{
			if (!eater.IsFormerHuman()) return null;
			if (!eater.RaceProps.predator) return HungerCategory.Hungry;
			return null;
		}
	}
}