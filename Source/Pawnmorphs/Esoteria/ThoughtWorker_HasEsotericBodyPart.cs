using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// thought worker that depends on if the pawn has a specific number of mutations 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class ThoughtWorker_HasEsotericBodyPart : ThoughtWorker
	{
		/// <summary>returns the current thought state of the pawn</summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			HediffSet hs = p.health.hediffSet;
			int num = 0;
			List<Hediff> hediffs = hs.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_AddedMutation)
				{
					num++;
				}
			}

			if (num > 0)
			{
				return ThoughtState.ActiveAtStage(num - 1);
			}
			return false;
		}
	}
}
