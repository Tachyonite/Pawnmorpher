// MorphRestriction.cs modified by Iron Wolf for Pawnmorph on 11/09/2019 8:42 AM
// last updated 11/09/2019  8:42 AM

using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension for making something restricted based on 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class MorphRestriction : RestrictionExtension
	{
		/// <summary>
		/// The morph filter
		/// </summary>
		public Filter<MorphDef> morphFilter;

		/// <summary>
		/// checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		/// if the def can be used with the given pawn
		/// </returns>
		protected override bool PassesRestrictionImpl(Pawn pawn)
		{
			var morph = pawn.def.GetMorphOfRace();
			bool isBlackList = morphFilter?.isBlackList ?? true;
			if (morph == null) return isBlackList; //if it's a black list then no morph should pass fine 
			return morphFilter?.PassesFilter(morph) ?? true;
		}

	}
}