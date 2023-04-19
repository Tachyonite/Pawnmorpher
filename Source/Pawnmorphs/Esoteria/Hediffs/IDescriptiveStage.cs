// IDescriptiveStage.cs modified by Iron Wolf for Pawnmorph on 12/28/2019 7:21 AM
// last updated 12/28/2019  7:21 AM

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// interface for a hediff stage that can override the hediff's main label and description entirely 
	/// </summary>
	public interface IDescriptiveStage
	{
		/// <summary>
		/// Gets the description override.
		/// </summary>
		/// <value>
		/// The description override.
		/// </value>
		string DescriptionOverride { get; }
		/// <summary>
		/// Gets the label override.
		/// </summary>
		/// <value>
		/// The label override.
		/// </value>
		string LabelOverride { get; }
	}
}