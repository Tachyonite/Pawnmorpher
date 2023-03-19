namespace Pawnmorph
{
	/// <summary>
	/// the tf gender switch options 
	/// </summary>
	public enum TFGender : byte
	{
		/// <summary>no explicit option set</summary>
		None,
		/// <summary>always set the animal gender to male</summary>
		Male,
		/// <summary>always set the animal gender to female</summary>
		Female,
		/// <summary>make the animal's gender the opposite of the pawn's</summary>
		Switch,
		/// <summary>make the animal's gender the same as the pawn's</summary>
		Original
	}
}
