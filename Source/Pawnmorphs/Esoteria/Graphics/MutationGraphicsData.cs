// MutationGraphicsData.cs created by Iron Wolf for Pawnmorph on 08/15/2021 12:52 PM
// last updated 08/15/2021  12:52 PM

using System.Xml;
using JetBrains.Annotations;

namespace Pawnmorph.GraphicSys
{
	/// <summary>
	/// simple class containing data about a specific set of mutations graphics 
	/// </summary>
	public class MutationGraphicsData
	{
		/// <summary>
		/// The path
		/// </summary>
		public string path;
		/// <summary>
		/// The anchor identifier
		/// </summary>
		public string anchorID;


		/// <summary>
		/// Loads the data from XML.
		/// </summary>
		/// <param name="xmlNode">The XML node.</param>
		[UsedImplicitly]
		public void LoadDataFromXmlCustom(XmlNode xmlNode)
		{
			anchorID = xmlNode.Name;
			path = xmlNode.FirstChild.Value;
		}
	}
}