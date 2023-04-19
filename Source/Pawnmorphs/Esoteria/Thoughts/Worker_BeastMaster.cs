// Worker_BeastMaster.cs modified by Iron Wolf for Pawnmorph on 12/04/2019 8:48 PM
// last updated 12/04/2019  8:48 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	///     thought worker for the beast master aspect
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_BeastMaster : ThoughtWorker_BondedAnimalMaster
	{
		[NotNull] private readonly List<string> _animalNames = new List<string>();

		/// <summary>
		///     Gets the current thought state
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (def.IsValidFor(p))
			{
				return base.CurrentStateInternal(p);
			}
			return false;
		}

		/// <inheritdoc />
		protected override bool AnimalMasterCheck(Pawn p, Pawn animal)
		{
			return base.AnimalMasterCheck(p, animal) && animal.IsFormerHuman();
		}
	}
}