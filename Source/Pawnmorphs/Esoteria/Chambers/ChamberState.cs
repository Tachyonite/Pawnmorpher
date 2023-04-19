// ChamberState.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 10:21 AM
// last updated 08/26/2019  10:22 AM

namespace Pawnmorph.Chambers
{
	/// <summary>
	///     enum for the different states a mutagenic chamber can be in
	/// </summary>
	public enum ChamberState
	{
		/// <summary>
		///     the chamber isn't doing anything
		/// </summary>
		Idle,

		///<summary>turning a pawn into an animal</summary>
		Transforming,

		///<summary>pawns are being merged in this chamber </summary>
		MergeInto,

		///<summary>the pawn is being merged into a different chamber </summary>
		MergeOutOf
	}
}