// IDescriptiveHediff.cs modified by Iron Wolf for Pawnmorph on 12/14/2019 7:31 PM
// last updated 12/14/2019  7:31 PM

namespace Pawnmorph
{
	/// <summary>
	/// interface for hediffs that have a custom description 
	/// </summary>
	public interface IDescriptiveHediff
	{
		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		string Description { get; }
	}
}