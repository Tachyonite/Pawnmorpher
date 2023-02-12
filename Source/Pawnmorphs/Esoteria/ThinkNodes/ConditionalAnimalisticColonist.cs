// ConditionalAnimalisticColonist.cs created by Iron Wolf for Pawnmorph on 05/09/2020 7:58 AM
// last updated 05/09/2020  7:58 AM

using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.ThinkNodes
{
	/// <summary>
	///     conditional think node for animalistic pawns in the player faction
	/// </summary>
	/// <seealso cref="Verse.AI.ThinkNode_Conditional" />
	public class ConditionalAnimalisticColonist : ThinkNode_Conditional
	{
		/// <summary>
		///     checks if the specified pawn is valid for this node.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override bool Satisfied(Pawn pawn)
		{
			bool debugLog = pawn.jobs?.debugLog == true;

			if (pawn.Faction != Faction.OfPlayer)
			{
				if (debugLog)
					Log.Message($"{pawn.Name} is not a part of the player's faction");
				return false;
			}

			if (!pawn.RaceProps.Humanlike)
			{
				if (debugLog)
					Log.Message($"{pawn.Name} is not a humanlike race!");

				return false;
			}

			if (pawn.GetIntelligence() != Intelligence.Animal)
			{
				if (debugLog) Log.Message($"{pawn.Name} is not animalistic!");

				return false;
			}

			if (pawn.training == null)
			{
				if (debugLog) Log.Message($"{pawn.Name} does not have a training tracker!");

				return false;
			}

			return true;
		}
	}
}