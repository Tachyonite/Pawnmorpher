// MutagenUtilities.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:11 PM
// last updated 08/13/2019  4:11 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{

	/// <summary>
	/// collection of mutagen related utility functions 
	/// </summary>
	public static class MutagenUtilities
	{

		/// <summary>
		/// Clears the overlapping hediffs on the given pawn.
		/// </summary>
		/// <param name="mutationGiver">The mutation giver.</param>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="ArgumentNullException">
		/// mutationGiver
		/// or
		/// pawn
		/// </exception>
		public static void ClearOverlappingHediffs([NotNull] this HediffGiver_Mutation mutationGiver, [NotNull] Pawn pawn)
		{
			if (mutationGiver == null) throw new ArgumentNullException(nameof(mutationGiver));
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			var health = pawn.health;
			if (health?.hediffSet?.hediffs == null) return;
			List<Hediff_AddedMutation> hediffsToRemove = new List<Hediff_AddedMutation>(); //save the result, otherwise we'd invalidate the enumerator when we start removing them  
			foreach (BodyPartDef bodyPartDef in mutationGiver.GetPartsToAddTo())
			{
				var hediffs = health.hediffSet.hediffs.Where(h => h?.Part?.def == bodyPartDef).OfType<Hediff_AddedMutation>();
				hediffsToRemove.AddRange(hediffs); //don't start removing them until we have all mutation we need to remove
			}

			hediffsToRemove.RemoveDuplicates(); //don't want to remove a hediff more then once

			foreach (Hediff_AddedMutation hediffAddedMutation in hediffsToRemove)
			{
				health.RemoveHediff(hediffAddedMutation);
			}
		}

		/// <summary>
		/// Tries the apply aspects that might be given from this mutagen
		/// </summary>
		/// <param name="mutagen">The mutagen.</param>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="ArgumentNullException">
		/// mutagen
		/// or
		/// pawn
		/// </exception>
		public static void TryApplyAspects([NotNull] this MutagenDef mutagen, [NotNull] Pawn pawn)
		{
			if (mutagen == null) throw new ArgumentNullException(nameof(mutagen));
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));

			foreach (AspectGiver aspectGiver in mutagen.aspectGivers.MakeSafe())
			{
				aspectGiver.TryGiveAspects(pawn);
			}

		}

		/// <summary> Determines whether this instance can infect the specified pawn. </summary>
		/// <param name="mutationDef"> The mutation definition. </param>
		/// <param name="pawn"> The pawn. </param>
		/// <returns> <c>true</c> if this instance can infect the specified pawn; otherwise, <c>false</c>. </returns>
		/// <exception cref="System.ArgumentNullException"> mutationDef or pawn is null. </exception>
		public static bool CanInfect([NotNull] this HediffDef mutationDef, [NotNull] Pawn pawn)
		{
			if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));



			MutagenDef mutagenSource = mutationDef.GetMutagenDef();



			return mutagenSource.CanInfect(pawn);
		}

		/// <summary> Gets the mutagen associated with this tf hediff. </summary>
		/// <param name="morphTf"> The morph tf. </param>
		/// <exception cref="ArgumentNullException"> morphTf is null. </exception>
		[NotNull]
		public static MutagenDef GetMutagenDef([NotNull] this MorphTf morphTf)
		{
			if (morphTf == null) throw new ArgumentNullException(nameof(morphTf));


			var defExt = morphTf.def.GetModExtension<MutagenExtension>();
			return defExt?.mutagen ?? MutagenDefOf.defaultMutagen;
			// check for a custom def, then for the extension, then return the default 
		}

		/// <summary> Gets the mutagen associated with this tf hediff. </summary>
		/// <param name="morphTf"> The morph tf. </param>
		/// <exception cref="ArgumentNullException"> morphTf is null. </exception>
		[NotNull]
		public static MutagenDef GetMutagenDef([NotNull] this HediffDef morphTf)
		{
			if (morphTf == null) throw new ArgumentNullException(nameof(morphTf));

			var defExt = morphTf.GetModExtension<MutagenExtension>();
			return defExt?.mutagen ?? MutagenDefOf.defaultMutagen;
			// check for a custom def, then for the extension, then return the default 
		}
	}
}