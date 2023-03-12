// ConditionalAnimalisticHumanlike.cs created by Iron Wolf for Pawnmorph on 07/30/2021 1:00 PM
// last updated 07/30/2021  1:00 PM

using Verse;
using Verse.AI;

namespace Pawnmorph.ThinkNodes
{
	/// <summary>
	/// think node for animalistic pawns 
	/// </summary>
	/// <seealso cref="Verse.AI.ThinkNode_Conditional" />
	public class ConditionalAnimalisticHumanlike : ThinkNode_Conditional
	{
		/// <summary>
		/// if the condition is satisfied or not 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.RaceProps.Humanlike && pawn.IsAnimal();
		}
	}
}