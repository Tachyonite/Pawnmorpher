// IFoodThoughtModifier.cs modified by Iron Wolf for Pawnmorph on 01/19/2020 4:47 PM
// last updated 01/19/2020  4:47 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// interface for things that affect what thoughts pawns can and cannot get from foods 
	/// </summary>
	public interface IFoodThoughtModifier
	{
		/// <summary>
		/// Gets the priority of this modifier 
		/// </summary>
		/// lower values are processed before higher priority ones 
		/// <value>
		/// The priority.
		/// </value>
		int Priority { get; }

		/// <summary>
		/// Modifies the thoughts from food.
		/// </summary>
		/// <param name="food">The food.</param>
		/// <param name="thoughts">The list of thoughts already added</param>
		void ModifyThoughtsFromFood([NotNull] Thing food, [NotNull] List<FoodUtility.ThoughtFromIngesting> thoughts);
	}
}