// TaggedBodyAddon.cs created by Iron Wolf for Pawnmorph on 08/15/2021 12:50 PM
// last updated 08/15/2021  12:50 PM

using AlienRace;

namespace Pawnmorph.GraphicSys
{
	/// <summary>
	/// subclass of har's body addon class to allow tagging unique addons for injection later
	/// </summary>
	/// <seealso cref="AlienRace.AlienPartGenerator.BodyAddon" />
	public class TaggedBodyAddon : AlienPartGenerator.BodyAddon
	{
		/// <summary>
		/// The anchor identifier
		/// </summary>
		public string anchorID;
	}
}