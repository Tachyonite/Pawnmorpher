// IFoodPreferenceAdjustor.cs created by Iron Wolf for Pawnmorph on 08/16/2021 6:13 PM
// last updated 08/16/2021  6:13 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// interface for something that can adjust a pawns food preference for a particular thing 
	/// </summary>
	public interface IFoodPreferenceAdjustor
	{
		/// <summary>
		/// Gets the priority.
		/// </summary>
		/// used for determining if multiple adjustors are present. 'lower' values override higher ones 
		/// <value>
		/// The priority.
		/// </value>
		int Priority { get; }

		/// <summary>
		/// Adjusts the preferability.
		/// </summary>
		/// <param name="eater">The eater.</param>
		/// <param name="thingToEat">The thing to eat.</param>
		/// <returns>the preferability,  null if no adjustment is needed</returns>
		FoodPreferability? AdjustPreferability([NotNull] Pawn eater, [NotNull] Thing thingToEat);

		/// <summary>
		/// Adjusts the preferability.
		/// </summary>
		/// <param name="eater">The eater.</param>
		/// <param name="foodType">Type of the food.</param>
		/// <returns>null if no adjustment is needed</returns>
		FoodPreferability? AdjustPreferability([NotNull] Pawn eater, FoodTypeFlags foodType);


		/// <summary>
		/// get the minimum hunger level to hunt for the given eater.
		/// </summary>
		/// note this will not make a pawn that doesn't normally hunt, hunt 
		/// <param name="eater">The eater.</param>
		/// <returns>the hunger level the eater will hunt at, null if they shouldn't or use the default level </returns>
		HungerCategory? MinHungerToHunt([NotNull] Pawn eater);


	}
}