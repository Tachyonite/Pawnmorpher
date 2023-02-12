// ApplyAdapationCream.cs created by Iron Wolf for Pawnmorph on 08/03/2021 1:45 PM
// last updated 08/03/2021  1:45 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.RecipeWorkers
{
	/// <summary>
	/// recipe worker for applying adaption cream 
	/// </summary>
	/// <seealso cref="Pawnmorph.RecipeWorkers.ApplyToMutatedPart" />
	public class ApplyAdaptationCream : ApplyToMutatedPart
	{

		/// <summary>
		/// applies the effect onto the given mutation. can be called multiple times on the same pawn 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="billDoer">The bill doer.</param>
		/// <param name="mutation">The mutation.</param>
		/// <param name="ingredients">The ingredients.</param>
		protected override void ApplyOnMutation(Pawn pawn, Pawn billDoer, Hediff_AddedMutation mutation, IReadOnlyList<Thing> ingredients)
		{
			mutation.SeverityAdjust.SeverityOffset += 1f;
		}

		/// <summary>
		/// Determines whether this instance with can be applied on the given mutation 
		/// </summary>
		/// <param name="mutation">The mutation.</param>
		/// <param name="recipe">The recipe.</param>
		/// <returns>
		///   <c>true</c> if this instance be applied on the given mutation otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanApplyOnMutation(Hediff_AddedMutation mutation, RecipeDef recipe)
		{
			return mutation.SeverityAdjust != null && mutation.SeverityAdjust.SeverityOffset < 1;
		}
	}
}