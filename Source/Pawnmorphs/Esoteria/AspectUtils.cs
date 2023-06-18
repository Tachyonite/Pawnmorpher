// AspectUtils.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:30 AM
// last updated 09/22/2019  11:30 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using Prepatcher;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// a collection of aspect related utilities 
	/// </summary>
	public static class AspectUtils
	{
		/// <summary>
		/// get the aspect tracker from this pawn 
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		[CanBeNull]
		[PrepatcherField]
		[InjectComponent]
		public static AspectTracker GetAspectTracker([NotNull] this Pawn pawn)
		{
			return CompCacher<AspectTracker>.GetCompCached(pawn);
		}

		/// <summary> Get the total production multiplier for the given mutation. </summary>
		public static float GetProductionBoost([NotNull] this IEnumerable<Aspect> aspects, HediffDef mutation)
		{
			float accum = 0;
			foreach (Aspect aspect in aspects)
			{
				accum += aspect.GetBoostOffset(mutation);
			}

			return accum;
		}

		/// <summary>
		/// Tries the apply aspects from this instance 
		/// </summary>
		/// <param name="morphDef">The morph hediff definition. this should be a 'transformative' hediff like 'wolfmorph', but in theory any hediffDef will do</param>
		/// <param name="pawn">The pawn.</param>
		public static void TryApplyAspectsFrom([NotNull] HediffDef morphDef, [NotNull] Pawn pawn)
		{
			var mutagen = morphDef.GetModExtension<MutagenExtension>()?.mutagen ?? MutagenDefOf.defaultMutagen;
			var giverExtensions = morphDef.modExtensions.MakeSafe().OfType<AspectGiverExtension>();

			TryApplyAspectsFrom(mutagen, pawn);

			foreach (AspectGiverExtension aspectGiverExtension in giverExtensions)
			{
				aspectGiverExtension.TryApply(pawn);
			}


		}
		/// <summary>
		/// Tries the apply aspects from this instance 
		/// </summary>
		/// <param name="mutagen">The mutagen.</param>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="ArgumentNullException">
		/// mutagen
		/// or
		/// pawn
		/// </exception>
		public static void TryApplyAspectsFrom([NotNull] MutagenDef mutagen, [NotNull] Pawn pawn)
		{
			if (mutagen == null) throw new ArgumentNullException(nameof(mutagen));
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));

			var givers = mutagen.aspectGivers.MakeSafe();
			foreach (AspectGiver aspectGiver in givers)
			{
				aspectGiver.TryGiveAspects(pawn);
			}

		}

		/// <summary>
		/// Determines whether this instance can receive rare mutations 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this instance can receive rare mutations  otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		public static bool CanReceiveRareMutations([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return pawn.GetAspectTracker()?.GetAspect(AspectDefOf.RareMutant) != null;
		}

		/// <summary>
		/// Determines whether this instance can grow mutagenic plants.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this instance can grow mutagenic plants; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		public static bool CanGrowMutagenicPlants([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return pawn.GetAspectTracker()?.GetAspect(AspectDefOf.PlantAffinity) != null;
		}
	}
}