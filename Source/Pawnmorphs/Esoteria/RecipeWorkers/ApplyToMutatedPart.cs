// ApplyToMutatedPart.cs created by Iron Wolf for Pawnmorph on 07/30/2021 7:39 AM
// last updated 07/30/2021  7:39 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.RecipeWorkers
{
	/// <summary>
	/// recipe worker for applying stuff to mutated parts  
	/// </summary>
	/// <seealso cref="RimWorld.Recipe_Surgery" />
	public abstract class ApplyToMutatedPart : Recipe_Surgery
	{
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
			base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
			List<Hediff> hediffs = pawn?.health?.hediffSet?.hediffs;
			if (hediffs == null) return;
			IReadOnlyList<Thing> l = ingredients;
			l = l ?? Array.Empty<Thing>();
			Init(pawn, billDoer, l);
			foreach (Hediff hediff in hediffs)
			{
				if (hediff.Part != part || !(hediff is Hediff_AddedMutation mutation) || !CanApplyOnMutation(mutation, recipe)) continue;
				ApplyOnMutation(pawn, billDoer, mutation, l);
			}

			FinishEffects(pawn, billDoer, l);
		}

		/// <summary>
		/// called when this instance is about to apply effects on mutations.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="billDoer">The bill doer.</param>
		/// <param name="ingredients">The ingredients.</param>
		protected virtual void Init(Pawn p, [CanBeNull] Pawn billDoer, [NotNull] IReadOnlyList<Thing> ingredients)
		{

		}

		/// <summary>
		/// called after all effects have been applied on the pawn 
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="billDoer">The bill doer.</param>
		/// <param name="ingredients">The ingredients.</param>
		protected virtual void FinishEffects(Pawn p, [CanBeNull] Pawn billDoer, [NotNull] IReadOnlyList<Thing> ingredients) { }

		/// <summary>
		/// applies the effect onto the given mutation. can be called multiple times on the same pawn 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="billDoer">The bill doer.</param>
		/// <param name="mutation">The mutation.</param>
		/// <param name="ingredients">The ingredients.</param>
		protected abstract void ApplyOnMutation([NotNull] Pawn pawn, [CanBeNull] Pawn billDoer, [NotNull] Hediff_AddedMutation mutation, [NotNull] IReadOnlyList<Thing> ingredients);


		/// <summary>
		/// Gets the parts to apply on.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="recipe">The recipe.</param>
		/// <returns></returns>
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			HashSet<BodyPartRecord> recordsRead = new HashSet<BodyPartRecord>();
			var hediffs = pawn?.health?.hediffSet?.hediffs;
			if (hediffs == null) yield break;

			foreach (Hediff_AddedMutation mutation in hediffs.OfType<Hediff_AddedMutation>())
			{
				var record = mutation.Part;
				if (record == null || recordsRead.Contains(record)) continue;
				if (!CanApplyOnMutation(mutation, recipe)) continue;
				recordsRead.Add(record);
				yield return record;
			}
		}

		/// <summary>
		/// Determines whether this instance with can be applied on the given mutation 
		/// </summary>
		/// <param name="mutation">The mutation.</param>
		/// <param name="recipe">The recipe.</param>
		/// <returns>
		///   <c>true</c> if this instance be applied on the given mutation otherwise, <c>false</c>.
		/// </returns>
		protected virtual bool CanApplyOnMutation([NotNull] Hediff_AddedMutation mutation, [NotNull] RecipeDef recipe)
		{
			return true;
		}
	}
}