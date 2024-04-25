// EtherState.cs modified by Iron Wolf for Pawnmorph on 07/28/2019 2:05 PM
// last updated 07/28/2019  2:05 PM

namespace Pawnmorph
{
	/// <summary> Enum for the 3 possible states a pawn can be in (in relation to 'ether' hediffs). </summary>
	public enum EtherState
	{
		/// <summary>
		/// the pawn is not ether broken or bonded 
		/// </summary>
		None = 0,
		/// <summary> pawn is considered 'broken' and should receive no or small penalties to producing </summary>
		Broken,
		/// <summary> pawn is considered 'bonded' and should receive bonuses for producing </summary>
		Bond
	}
}