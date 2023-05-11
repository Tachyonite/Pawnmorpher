// TagAnimal.cs created by Iron Wolf for Pawnmorph on 07/20/2021 6:37 PM
// last updated 07/20/2021  6:37 PM

using System.Collections.Generic;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using RimWorld;
using Verse;

namespace Pawnmorph.RecipeWorkers
{
	/// <summary>
	/// recipe worker for tagging animals 
	/// </summary>
	/// <seealso cref="RimWorld.Recipe_Surgery" />
	public class TagAnimal : Recipe_Surgery
	{
		private ChamberDatabase DB => Find.World.GetComponent<ChamberDatabase>();

		/// <summary>
		/// Applies the on pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="part">The part.</param>
		/// <param name="billDoer">The bill doer.</param>
		/// <param name="ingredients">The ingredients.</param>
		/// <param name="bill">The bill.</param>
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			if (pawn?.kindDef == null) return;
			DB.TryAddToDatabase(new AnimalGenebankEntry(pawn.kindDef));
		}

		/// <summary>
		/// check if the recipe is available now 
		/// </summary>
		/// <param name="thing">The thing.</param>
		/// <param name="part">The part.</param>
		/// <returns></returns>
		public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
		{
			if (!thing.def.IsValidAnimal()) return false;
			if (!(thing is Pawn pawn)) return false;
			return !pawn.kindDef.IsTagged();

		}
	}
}