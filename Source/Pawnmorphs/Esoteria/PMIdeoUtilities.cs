// PMIdeoUtilities.cs created by Iron Wolf for Pawnmorph on 07/22/2021 5:14 PM
// last updated 07/22/2021  5:14 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static class containing additional ideology utilities 
	/// </summary>
	public static class PMIdeoUtilities
	{

		/// <summary>
		/// Tries to get a meme variant of a thing for the given pawn.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tuple">The tuple.</param>
		/// <param name="pawn">The pawn.</param>
		/// <param name="val">The value.</param>
		/// <param name="isValidFunc">The is valid function.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// tuple
		/// or
		/// pawn
		/// </exception>
		public static bool TryGetMemeVariant<T>([NotNull] this IEnumerable<ValueTuple<MemeDef, T>> tuple, [NotNull] Pawn pawn, out T val, [CanBeNull] Func<Pawn, T, bool> isValidFunc = null)
		{
			if (tuple == null) throw new ArgumentNullException(nameof(tuple));
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			var ideo = pawn.Ideo;
			if (ideo == null)
			{
				val = default;
				return false;
			}

			foreach ((MemeDef meme, T thing) in tuple)
			{
				if (isValidFunc?.Invoke(pawn, thing) == false) continue;
				if (ideo.HasMeme(meme))
				{
					val = thing;
					return true;
				}
			}

			val = default;
			return false;

		}


		/// <summary>
		/// Determines whether the ideology has a position on the given issue.
		/// </summary>
		/// <param name="ideo">The ideo.</param>
		/// <param name="issue">The issue.</param>
		/// <returns>
		///   <c>true</c> if the ideology has a position on the given issue; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasPositionOn([NotNull] this Ideo ideo, [NotNull] IssueDef issue)
		{
			if (ideo == null) throw new ArgumentNullException(nameof(ideo));
			if (issue == null) throw new ArgumentNullException(nameof(issue));

			foreach (Precept precept in ideo.PreceptsListForReading.MakeSafe())
			{
				if (precept?.def?.issue == issue) return true;
			}

			return false;
		}


	}
}