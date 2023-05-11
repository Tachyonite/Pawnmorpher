// MutationCauseUtility.cs created by Iron Wolf for Pawnmorph on 09/05/2021 9:21 AM
// last updated 09/05/2021  9:21 AM

using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static class containing mutation cause related utilities 
	/// </summary>
	public static class MutationCauseUtility
	{
		/// <summary>
		/// Adds the mutagen cause.
		/// </summary>
		/// <param name="causes">The causes.</param>
		/// <param name="mutagen">The mutagen.</param>
		/// <exception cref="System.ArgumentNullException">
		/// causes
		/// or
		/// mutagen
		/// </exception>
		public static void AddMutagenCause([NotNull] this MutationCauses causes, [NotNull] MutagenDef mutagen)
		{
			if (causes == null) throw new ArgumentNullException(nameof(causes));
			if (mutagen == null) throw new ArgumentNullException(nameof(mutagen));

			causes.Add(MutationCauses.MUTAGEN_PREFIX, mutagen);
		}


		/// <summary>
		/// Adds the mutagen cause.
		/// </summary>
		/// <param name="causes">The causes.</param>
		/// <param name="mutagen">The mutagen.</param>
		/// <returns>true if added, false if the def was added previously</returns>
		/// <exception cref="System.ArgumentNullException">
		/// causes
		/// or
		/// mutagen
		/// </exception>
		public static bool TryAddMutagenCause([NotNull] this MutationCauses causes, [NotNull] MutagenDef mutagen)
		{
			if (causes == null) throw new ArgumentNullException(nameof(causes));
			if (mutagen == null) throw new ArgumentNullException(nameof(mutagen));

			return causes.TryAddCause(MutationCauses.MUTAGEN_PREFIX, mutagen);
		}

		/// <summary>
		/// Tries to add a new cause.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="causes">The causes.</param>
		/// <param name="prefix">The prefix.</param>
		/// <param name="def">The definition.</param>
		/// <returns>true if added, false if the def was added previously</returns>
		/// <exception cref="System.ArgumentNullException">
		/// causes
		/// or
		/// def
		/// </exception>
		public static bool TryAddCause<T>([NotNull] this MutationCauses causes, string prefix, [NotNull] T def) where T : Def, new()
		{
			if (causes == null) throw new ArgumentNullException(nameof(causes));
			if (def == null) throw new ArgumentNullException(nameof(def));

			if (causes.HasDefCause(def)) return false;
			causes.Add(prefix, def);
			return true;
		}

		/// <summary>
		/// Tries to add the precept cause.
		/// </summary>
		/// <param name="causes">The causes.</param>
		/// <param name="precept">The precept.</param>
		/// <param name="prefix">The prefix.</param>
		/// <returns>if the precept was added, false if the precept was already a cause</returns>
		/// <exception cref="System.ArgumentNullException">
		/// causes
		/// or
		/// precept
		/// </exception>
		public static bool TryAddPrecept([NotNull] this MutationCauses causes, [NotNull] Precept precept,
										 string prefix = MutationCauses.PRECEPT_PREFIX)
		{
			if (causes == null) throw new ArgumentNullException(nameof(causes));
			if (precept == null) throw new ArgumentNullException(nameof(precept));

			if (causes.HasPreceptCause(precept)) return false;
			causes.Add(precept, prefix);
			return true;
		}
	}
}