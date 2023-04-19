// ConditionalFactionSapientAnimal.cs created by Iron Wolf for Pawnmorph on 06/23/2020 5:30 PM
// last updated 06/23/2020  5:30 PM

using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.ThinkNodes
{
	/// <summary>
	/// conditional node for sapient animals that are part of a faction other then the player's
	/// </summary>
	/// <seealso cref="Verse.AI.ThinkNode_Conditional" />
	public class ConditionalFactionSapientAnimal : ThinkNode_Conditional
	{
		/// <summary>
		/// The sapience cutoff
		/// </summary>
		public SapienceLevel cutoff;

		/// <summary>
		/// check if the pawn satisfies this condition.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.IsFormerHuman()
				&& pawn.GetQuantizedSapienceLevel() <= cutoff
				&& pawn.Faction != null
				&& pawn.Faction != Faction.OfPlayer;
		}
	}
}