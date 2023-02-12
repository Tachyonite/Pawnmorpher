// ColonyRelation.cs created by Iron Wolf for Pawnmorph on 07/25/2021 4:18 PM
// last updated 07/25/2021  4:18 PM

namespace Pawnmorph
{
	/// <summary>
	/// enum for how a pawn relates to the colony 
	/// </summary>
	public enum ColonyRelation
	{
		/// <summary>
		/// 'wild' pawns. ie they have no special relation to the colony 
		/// </summary>
		Wild,
		/// <summary>
		/// a colonist 
		/// </summary>
		Colonist,
		/// <summary>
		/// a prisoner of the colony 
		/// </summary>
		Prisoner,
		/// <summary>
		/// an ally/guest of the colony 
		/// </summary>
		Ally,
		/// <summary>
		/// a slave of the colony 
		/// </summary>
		Slave,
		/// <summary>
		/// a guilty prisoner of the colony 
		/// </summary>
		PrisonerGuilty
	}
}