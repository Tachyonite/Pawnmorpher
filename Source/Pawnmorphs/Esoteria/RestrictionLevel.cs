// RestrictionLevel.cs created by Iron Wolf for Pawnmorph on 08/30/2021 6:39 PM
// last updated 08/30/2021  6:39 PM

namespace Pawnmorph
{
	/// <summary>
	/// enum represented how restricted a mutation category is when trying to record mutation in the database
	/// </summary>
	public enum RestrictionLevel
	{
		/// <summary>
		/// the category is unrestricted, and can spawn freely 
		/// </summary>
		UnRestricted,
		/// <summary>
		/// the mutations can be gotten though genomes of any restricted category 
		/// </summary>
		CategoryOnly,
		/// <summary>
		/// The mutations are always un recordable 
		/// </summary>
		Always
	}
}