// FormerHumanFoodSettings.cs modified by Iron Wolf for Pawnmorph on 01/07/2020 7:20 PM
// last updated 01/07/2020  7:20 PM

using RimWorld;

namespace Pawnmorph.FormerHumans
{
	/// <summary>
	/// simple POD for former human food settings 
	/// </summary>
	public class FoodThoughtSettings
	{
		/// <summary>
		/// The thought received for when the former human eats meat of the same species they are 
		/// </summary>
		public ThoughtDef cannibalThought;

		/// <summary>
		/// The thought received for when the former human eats meat of the same species they are and have the cannibal trait
		/// </summary>
		public ThoughtDef cannibalThoughtGood;

		/// <summary>
		/// The thought received for when a former human eats a meal with meat of the same species they are as an ingredient and have the cannibal trait 
		/// </summary>
		public ThoughtDef cannibalThoughtIngredientGood;

		/// <summary>
		/// The thought received for when a former human eats a meal with meat of the same species they are as an ingredient
		/// </summary>
		public ThoughtDef cannibalThoughtIngredient;



	}
}