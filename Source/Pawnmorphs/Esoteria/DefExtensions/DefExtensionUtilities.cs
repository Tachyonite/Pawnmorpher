// DefExtensionUtilities.cs modified by Iron Wolf for Pawnmorph on 11/09/2019 8:56 AM
// last updated 11/09/2019  8:56 AM

using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// static class for commonly used def extension functions 
	/// </summary>
	public static class DefExtensionUtilities
	{
		/// <summary>
		/// Determines whether this def is valid for the specified pawn.
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="pawn">The pawn.</param>
		/// <param name="mustPassAll">if set to <c>true</c> if the pawn must pass all restrictions.</param>
		/// <returns>
		///   <c>true</c> if this is valid for the specified pawn; otherwise, <c>false</c>.
		/// </returns>
		/// this function checks for all defExtensions that inherit from <see cref="RestrictionExtension" /> and check the pawn against them
		public static bool IsValidFor([NotNull] this Def def, Pawn pawn, bool mustPassAll = true)
		{
			var restrictions = def.modExtensions?.OfType<RestrictionExtension>() ?? Enumerable.Empty<RestrictionExtension>();

			foreach (RestrictionExtension restriction in restrictions)
			{
				if (!restriction.PassesRestriction(pawn))
				{
					if (mustPassAll) //if we must pass all of them and one fails return false 
						return false;
				}
				else if (!mustPassAll) return true; //if one passes and we don't have to pass all of them return true 
			}

			return true;
		}
	}
}