// MutationRestriction.cs modified by Iron Wolf for Pawnmorph on 11/09/2019 8:40 AM
// last updated 11/09/2019  8:40 AM

using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	///  def extension for making something restricted based on mutations 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class MutationRestriction : RestrictionExtension
	{
		/// <summary>
		/// The mutation filter 
		/// </summary>
		public Filter<HediffDef> mutationFilter;

		/// <summary>
		/// checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		/// if the def can be used with the given pawn
		/// </returns>
		protected override bool PassesRestrictionImpl(Pawn pawn)
		{
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				if (!(mutationFilter?.PassesFilter(hediff.def) ?? true)) return false;
			}

			return true;
		}
	}
}