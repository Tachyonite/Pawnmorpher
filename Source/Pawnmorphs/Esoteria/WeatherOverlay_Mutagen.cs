// WeatherOverlay_Mutagen.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 09/08/2019  9:49 AM

using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// class for the mutagenic weather overlay 
	/// </summary>
	/// <seealso cref="Verse.SkyOverlay" />
	[StaticConstructorOnStartup]
	public class WeatherOverlay_Mutagen : SkyOverlay
	{
		private static readonly Material FalloutOverlayWorld = MatLoader.LoadMat("Weather/SnowOverlayWorld");
		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherOverlay_Mutagen"/> class.
		/// </summary>
		public WeatherOverlay_Mutagen()
		{
			worldOverlayMat = FalloutOverlayWorld;
			worldOverlayPanSpeed1 = 0.0008f;
			worldPanDir1 = new Vector2(-0.25f, -1f);
			worldPanDir1.Normalize();
			worldOverlayPanSpeed2 = 0.0012f;
			worldPanDir2 = new Vector2(-0.24f, -1f);
			worldPanDir2.Normalize();
		}
	}
}