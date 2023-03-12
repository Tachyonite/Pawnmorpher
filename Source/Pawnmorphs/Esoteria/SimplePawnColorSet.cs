using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// Stores color data needed to color a pawn.
	/// </summary>
	public class SimplePawnColorSet : IExposable
	{
		/// <summary> Color slots </summary>
		public enum PawnColorSlot
		{
			/// <summary> Skin color 1 </summary>
			SkinFirst,
			/// <summary> Skin color 2 </summary>
			SkinSecond,
			/// <summary> Hair color 1 </summary>
			HairFirst,
			/// <summary> Hair color 2 </summary>
			HairSecond
		}

		/// <summary> Skin color 1 </summary>
		public Color? skinColor = null;
		/// <summary> Skin color 2 </summary>
		public Color? skinColorTwo = null;
		/// <summary> Hair color 1 </summary>
		public Color? hairColor = null;
		/// <summary> Hair color 2 </summary>
		public Color? hairColorTwo = null;

		/// <summary> Called during IExposable's ExposeData to serialize data. </summary>
		public void ExposeData()
		{
			Scribe_Values.Look<Color?>(ref skinColor, "skinColor");
			Scribe_Values.Look<Color?>(ref skinColorTwo, "skinColorTwo");
			Scribe_Values.Look<Color?>(ref hairColor, "hairColor");
			Scribe_Values.Look<Color?>(ref hairColorTwo, "hairColorTwo");
		}
	}
}
