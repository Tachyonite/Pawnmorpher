// IDebugString.cs created by Iron Wolf for Pawnmorph on 02/29/2020 11:28 AM
// last updated 02/29/2020  11:28 AM

namespace Pawnmorph.DebugUtils
{
	/// <summary>
	/// interface for things that want to give more information about their state that won't fit in ToString
	/// </summary>
	public interface IDebugString
	{
		/// <summary>
		/// returns a full, detailed, representation of the object in string form 
		/// </summary>
		/// <returns></returns>
		string ToStringFull();
	}
}