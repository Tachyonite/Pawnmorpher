// PrimalWishGiver.cs modified by Iron Wolf for Pawnmorph on 01/13/2020 7:05 PM
// last updated 01/13/2020  7:05 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Aspects
{

	/// <summary>
	/// aspect giver for the primal wish giver 
	/// </summary>
	/// <seealso cref="Pawnmorph.AspectGiver" />
	public class PrimalWishGiver : AspectGiver
	{
		/// <summary>
		///     Gets the aspects available to be given to pawns.
		/// </summary>
		/// <value>
		///     The available aspects.
		/// </value>
		public override IEnumerable<AspectDef> AvailableAspects
		{
			get { yield return AspectDefOf.PrimalWish; }
		}

		/// <summary>
		/// The normal chance to give the primal wish aspect 
		/// </summary>
		public float normalChance = 0.1f;

		/// <summary>
		/// the chance for mutation affinity pawns to get the aspect 
		/// </summary>
		public float mutationAffinityChance = 0.2f;



		/// <summary>
		/// Tries to give aspects to the given pawn 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="outList">if not null, all given aspects will be placed into the list</param>
		/// <returns>if any aspects were successfully given to the pawn</returns>
		public override bool TryGiveAspects(Pawn pawn, List<Aspect> outList = null)
		{
			if (!CheckPawnTraits(pawn.story.traits, AspectDefOf.PrimalWish)) return false;
			if (pawn.GetAspectTracker()?.Contains(AspectDefOf.PrimalWish) == true) return false; //don't give it twice
			float chance = pawn.story.traits.HasTrait(PMTraitDefOf.MutationAffinity) ? mutationAffinityChance : normalChance;
			if (Rand.Value < chance)
			{
				return ApplyAspect(pawn, AspectDefOf.PrimalWish, 0, outList);
			}

			return false;

		}
	}
}