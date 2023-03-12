// RestrictionExtension.cs modified by Iron Wolf for Pawnmorph on 11/09/2019 8:58 AM
// last updated 11/09/2019  8:58 AM

using System;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	///     abstract base class for all def extensions that restrict the use of a def from pawns based on some sort of criteria
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public abstract class RestrictionExtension : DefModExtension
	{
		/// <summary>
		///     if true, inverts the normal behavior of this instance
		/// </summary>
		public bool invert;

		/// <summary>
		///     checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     if the def can be used with the given pawn
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public bool PassesRestriction([NotNull] Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			bool v = PassesRestrictionImpl(pawn);
			return invert ? !v : v; //if invert is true, invert the return of PassesRestrictionImpl 
		}

		/// <summary>
		///     checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     if the def can be used with the given pawn
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected abstract bool PassesRestrictionImpl([NotNull] Pawn pawn);
	}
}