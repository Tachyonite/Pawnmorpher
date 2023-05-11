// LogLevel.cs created by Iron Wolf for Pawnmorph on 02/29/2020 10:31 AM
// last updated 02/29/2020  10:31 AM

namespace Pawnmorph.DebugUtils
{
	/// <summary>
	/// enum to control how much logging should be done 
	/// </summary>
	public enum LogLevel
	{
		/// <summary>
		/// only log errors 
		/// </summary>
		Error = 0,
		/// <summary>
		/// log warnings and errors 
		/// </summary>
		Warnings,
		/// <summary>
		/// log messages, warnings and errors 
		/// </summary>
		Messages,
		/// <summary>
		/// log everything 
		/// </summary>
		Pedantic
	}
}