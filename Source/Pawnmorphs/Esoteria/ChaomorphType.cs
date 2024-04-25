// ChaomorphType.cs created by Iron Wolf for Pawnmorph on 09/26/2020 5:34 PM
// last updated 09/26/2020  5:34 PM

namespace Pawnmorph
{
	//TODO is this ok to be an enum or should we make it a def? 
	//who else would need to make different kinds of chaomorphs? 

	/// <summary>
	/// the types of chaomorphs 
	/// </summary>
	public enum ChaomorphType
	{
		/// <summary>
		/// a regular chaomorph 
		/// </summary>
		Chaomorph,
		/// <summary>
		/// result of merging 2 pawns 
		/// </summary>
		Merge,
		/// <summary>
		/// special category for use with exotic choamorphs that should be handled separately 
		/// </summary>
		Special
	}
}