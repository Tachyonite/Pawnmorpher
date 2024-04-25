// MeatLover.cs modified by Iron Wolf for Pawnmorph on 01/19/2020 4:58 PM
// last updated 01/19/2020  4:58 PM

using System.Collections.Generic;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph.Aspects
{
	/// <summary>
	/// controller for the meat lover aspect 
	/// </summary>
	/// <seealso cref="Pawnmorph.Aspect" />
	/// <seealso cref="Pawnmorph.Thoughts.IFoodThoughtModifier" />
	public class MeatLover : Aspect, IFoodThoughtModifier
	{
		//TODO good thought for wargmorph raw meat? 


		/// <summary>
		/// Modifies the thoughts from food.
		/// </summary>
		/// <param name="food">The food.</param>
		/// <param name="thoughts">The list of thoughts already added</param>
		public void ModifyThoughtsFromFood(Thing food, List<FoodUtility.ThoughtFromIngesting> thoughts)
		{
			var foodType = food.def.ingestible?.foodType ?? FoodTypeFlags.None;

			if ((foodType & (FoodTypeFlags.Meat | FoodTypeFlags.Corpse)) == 0) return; //only do this for meats 
			for (int i = thoughts.Count - 1; i >= 0; i--)
			{
				var td = thoughts[i];
				if (td.thought == ThoughtDefOf.AteRawFood)
					thoughts.RemoveAt(i); //remove raw food thoughts 
			}
		}
	}
}