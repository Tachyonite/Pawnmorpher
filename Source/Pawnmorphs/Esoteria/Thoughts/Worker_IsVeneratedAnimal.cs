// Worker_IsVeneratedAnimal.cs created by Iron Wolf for Pawnmorph on 07/21/2021 7:10 AM
// last updated 07/21/2021  7:10 AM

using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	///     thought worker for the Is Venerated animal thought
	/// </summary>
	/// <seealso cref="Pawnmorph.Thoughts.Worker_FormerHuman" />
	public class Worker_IsVeneratedAnimal : Worker_FormerHuman
	{
		/// <summary>
		///     gets the current thought state for the pawn
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.IdeologyActive) return false;

			Ideo ideo = p?.ideo?.Ideo;
			if (ideo == null) return false;
			if (ideo.IsVeneratedAnimal(p))
			{
				int? stage = GetStage(p);
				return stage == null ? false : ThoughtState.ActiveAtStage(stage.Value);
			}

			return false;
		}
	}
}