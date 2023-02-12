// Worker_JelousMorph.cs created by Iron Wolf for Pawnmorph on 07/15/2021 7:32 AM
// last updated 07/15/2021  7:32 AM

using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought worker for jealous mutation affinity pawns 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_JealousMorph : ThoughtWorker
	{
		/// <summary>
		/// Currents the current thought state of the given pawn .
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.IsFormerHuman()) return false;
			if (!p.def.IsHybridRace()) return false;
			var mT = p.Map?.GetComponent<MorphTracker>();
			if (mT == null) return false;
			return (!p.IsMorph() && mT.AnyMorphs);
		}
	}
}