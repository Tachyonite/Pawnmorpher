// LogFailMode.cs created by Iron Wolf for Pawnmorph on 08/29/2021 8:27 PM
// last updated 08/29/2021  8:27 PM

namespace Pawnmorph.DebugUtils
{
	/// <summary>
	///     enum for different modes functions can record errors
	/// </summary>
	public enum LogFailMode
	{
		/// <summary>
		/// fails  are ignored 
		/// </summary>
		Silent,
		/// <summary>
		/// fails are logged and ignored 
		/// </summary>
		Log,
		/// <summary>
		/// fails produce a warning and ignored 
		/// </summary>
		Warning,
		/// <summary>
		/// fails produce an error
		/// </summary>
		Error
	}
}