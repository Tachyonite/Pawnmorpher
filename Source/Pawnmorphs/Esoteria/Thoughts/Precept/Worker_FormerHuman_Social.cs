// Worker_FormerHuman.cs created by Iron Wolf for Pawnmorph on 08/02/2021 5:52 PM
// last updated 08/02/2021  5:52 PM

using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts.Precept
{
	/// <summary>
	/// thought worker for a pawn viewing another pawn who's a former human 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker_Precept_Social" />
	public class Worker_FormerHuman_Social : ThoughtWorker_Precept_Social
	{

		/// <summary>
		/// if this pawn should have the thought about the other given pawn 
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="otherPawn">The other pawn.</param>
		/// <returns></returns>
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			if (p == null) return false;
			return otherPawn?.IsFormerHuman() == true;
		}
	}

	/// <summary>
	/// thought worker for a pawn viewing another pawn who's been tf'd into one of the ideos venerated animals 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker_Precept_Social" />
	public class Worker_VeneratedFormerHuman_Social : ThoughtWorker_Precept_Social
	{

		/// <summary>
		/// if this pawn should have the thought about the other given pawn 
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="otherPawn">The other pawn.</param>
		/// <returns></returns>
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			var ideo = p?.Ideo;
			if (ideo == null) return false;
			return otherPawn?.IsFormerHuman() == true && ideo.VeneratedAnimals.MakeSafe().Contains(otherPawn.def);
		}
	}
}