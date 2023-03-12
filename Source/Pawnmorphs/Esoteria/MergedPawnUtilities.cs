// MergedPawnUtilities.cs created by Iron Wolf for Pawnmorph on 05/09/2020 1:04 PM
// last updated 05/09/2020  1:04 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     static class containing merged pawn related utilities
	/// </summary>
	public static class MergedPawnUtilities
	{
		/// <summary>
		///     Transfers the aspects to the given meld.
		/// </summary>
		/// <param name="originalPawns">The original pawns.</param>
		/// <param name="mergedPawn">The merged pawn.</param>
		private static void TransferAspects([NotNull] IEnumerable<Pawn> originalPawns, [NotNull] Pawn mergedPawn)
		{
			AspectTracker mAT = mergedPawn.GetAspectTracker();
			if (mAT == null) return;
			var aspectDefs = new List<AspectDef>();


			foreach (Pawn originalPawn in originalPawns)
			{
				AspectTracker at = originalPawn?.GetAspectTracker();
				if (at == null) continue;

				foreach (Aspect aspect in at.Aspects)
					if (aspect.def.transferToAnimal && !aspectDefs.Contains(aspect.def))
						aspectDefs.Add(aspect.def);
			}

			foreach (AspectDef aspectDef in aspectDefs) mAT.Add(aspectDef);
		}

		/// <summary>
		/// Transfers traits, aspects, and relationships to a merged pawn.
		/// </summary>
		/// <param name="originals">The originals.</param>
		/// <param name="mergedPawn">The merged pawn.</param>
		/// <exception cref="ArgumentNullException">
		/// originals
		/// or
		/// mergedPawn
		/// </exception>
		public static void TransferToMergedPawn([NotNull] IReadOnlyList<Pawn> originals, [NotNull] Pawn mergedPawn)
		{
			if (originals == null) throw new ArgumentNullException(nameof(originals));
			if (mergedPawn == null) throw new ArgumentNullException(nameof(mergedPawn));
			TransferAspects(originals, mergedPawn);
			TransferTraits(originals, mergedPawn);
			PawnTransferUtilities.MergeSkills(originals, mergedPawn);
			//disabled for now, no way to undo this when the meld is reverted 
			//PawnTransferUtilities.TransferRelations(originals, mergedPawn, r => r != PawnRelationDefOf.Bond); 

		}


		private static void TransferTraits([NotNull] IEnumerable<Pawn> originalPawns, [NotNull] Pawn mergedPawn)
		{
			TraitSet mTraits = mergedPawn.story?.traits;
			if (mTraits == null) return;

			var traits = new List<TraitDef>();

			foreach (Pawn originalPawn in originalPawns)
			{
				List<Trait> pTraits = originalPawn.story?.traits?.allTraits;
				if (pTraits == null) continue;
				foreach (Trait pTrait in pTraits)
				{
					if (traits.Contains(pTrait.def) || !FormerHumanUtilities.MutationTraits.Contains(pTrait.def)) continue;
					traits.Add(pTrait.def);
				}
			}

			//check for split mind 
			//if (traits.Contains(PMTraitDefOf.MutationAffinity) && traits.Contains(TraitDefOf.BodyPurist))
			//{
			//    traits.Remove(PMTraitDefOf.MutationAffinity);
			//    traits.Remove(TraitDefOf.BodyPurist);

			//    AspectTracker at = mergedPawn.GetAspectTracker();
			//    at?.Add(AspectDefOf.SplitMind);
			//}

			foreach (TraitDef traitDef in traits) mTraits.GainTrait(new Trait(traitDef, 0, true));
		}
	}
}