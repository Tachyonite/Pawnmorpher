// Giver_SapientAnimalJoy.cs modified by Iron Wolf for Pawnmorph on 12/19/2019 7:57 PM
// last updated 12/19/2019  7:57 PM

using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	/// job giver to give sapient animals joy jobs 
	/// </summary>
	/// <seealso cref="RimWorld.JobGiver_GetJoy" />
	public class Giver_SapientAnimalJoy : JobGiver_GetJoy
	{
		/// <summary>
		/// Tries to give the pawn a job from joy giver definition
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
		{
			if (!def.IsValidFor(pawn)) return null;

			var joyNeed = pawn.needs?.joy?.CurLevelPercentage ?? 1;
			if (joyNeed > 0.75f) return null; //make sure we don't assign the need constantly  

			return base.TryGiveJobFromJoyGiverDefDirect(def, pawn);
		}

		/// <summary>
		/// Gets the priority.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public override float GetPriority(Pawn pawn)
		{

			var joyNeed = pawn.needs?.joy?.CurLevelPercentage ?? 1;
			if (joyNeed > 0.75f) return 0; //make sure we don't assign the need constantly  
			return base.GetPriority(pawn);
		}
	}
}