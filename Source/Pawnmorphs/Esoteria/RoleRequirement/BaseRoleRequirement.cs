// BaseRoleRequirement.cs created by Iron Wolf for Pawnmorph on 07/23/2021 3:58 PM
// last updated 07/23/2021  3:58 PM

using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.RoleRequirement
{
	/// <summary>
	/// base role requirement containing useful features for pawnmorpher 
	/// </summary>
	/// <seealso cref="RimWorld.RoleRequirement" />
	public abstract class BaseRoleRequirement : RimWorld.RoleRequirement
	{
		/// <summary>
		///     if the condition is
		/// </summary>
		public bool invert;


		/// <summary>
		///     Mets the specified p.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="role">The role.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     p
		///     or
		///     role
		/// </exception>
		public sealed override bool Met([NotNull] Pawn p, [NotNull] Precept_Role role)
		{
			if (p == null) throw new ArgumentNullException(nameof(p));
			if (role == null) throw new ArgumentNullException(nameof(role));
			return Met_Internal(p, role) ^ invert;
		}

		/// <summary>
		///     determine if the given pawn meets the conditions of this requirement for the given role.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="role">The role.</param>
		/// <returns></returns>
		protected abstract bool Met_Internal([NotNull] Pawn p, [NotNull] Precept_Role role);

	}
}