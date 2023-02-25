// ApplyHaltingCream.cs created by Iron Wolf for Pawnmorph on 07/30/2021 7:52 AM
// last updated 07/30/2021  7:52 AM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.RecipeWorkers
{

	/// <summary>
	/// recipe worker for applying halting cream 
	/// </summary>
	/// <seealso cref="Pawnmorph.RecipeWorkers.ApplyToMutatedPart" />
	public class ApplyHaltingCream : ApplyToMutatedPart
	{
		private const float MIN_BOUND = 0f;
		private const float MAX_BOUND = 6f;

		/// <summary>
		/// Gets the parts to apply on.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="recipe">The recipe.</param>
		/// <returns></returns>
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			if (pawn?.health?.hediffSet == null) return Enumerable.Empty<BodyPartRecord>();
			return base.GetPartsToApplyOn(pawn, recipe).Where(r => r.def.IsSkinCovered(r, pawn.health.hediffSet));
		}

		/// <summary>
		/// applies the effect onto the given mutation. can be called multiple times on the same pawn 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="billDoer">The bill doer.</param>
		/// <param name="mutation">The mutation.</param>
		/// <param name="ingredients">The ingredients.</param>
		protected override void ApplyOnMutation(Pawn pawn, Pawn billDoer, Hediff_AddedMutation mutation, IReadOnlyList<Thing> ingredients)
		{
			var medSkill = billDoer?.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? 0;

			var haltComp = mutation.SeverityAdjust;
			if (haltComp == null) return;

			float sk = (medSkill - MIN_BOUND) / (MAX_BOUND - MIN_BOUND);
			sk = Mathf.Clamp(sk, 0, 1);
			sk = MathUtilities.SmoothStep(0, 1, sk);
			sk = sk * 0.5f + 0.1f; //10% chance to 60% chance to halt a mutation depending on level 


			if (!haltComp.Halted)
				haltComp.Halted = Rand.Value < sk;



		}
	}
}