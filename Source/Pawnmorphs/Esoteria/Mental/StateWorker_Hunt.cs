// StateWorker_Hunt.cs modified by Iron Wolf for Pawnmorph on 12/15/2019 4:57 PM
// last updated 12/15/2019  4:57 PM

using Pawnmorph.DefExtensions;
using Verse;
using Verse.AI;

namespace Pawnmorph.Mental
{
	/// <summary>
	/// mental state worker for the 'hunting' former human break 
	/// </summary>
	/// <seealso cref="Verse.AI.MentalStateWorker" />
	public class StateWorker_Hunt : MentalStateWorker
	{
		/// <summary>
		/// States the can occur.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public override bool StateCanOccur(Pawn pawn)
		{
			return def.IsValidFor(pawn) && FormerHumanUtilities.FindRandomPreyFor(pawn) != null;
		}
	}
}