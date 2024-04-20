// Centrifuge.cs created by Iron Wolf for Pawnmorph on 02/23/2021 5:23 PM
// last updated 02/23/2021  5:23 PM

using Pawnmorph.Buildings;
using UnityEngine;
using Verse;

namespace Pawnmorph.PlaceWorkers
{
	/// <summary>
	/// place worker for the centrifuge 
	/// </summary>
	/// <seealso cref="Verse.PlaceWorker" />
	public class Centrifuge : PlaceWorker
	{
		/// <summary>
		/// Draws the ghost.
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="center">The center.</param>
		/// <param name="rot">The rot.</param>
		/// <param name="ghostCol">The ghost col.</param>
		/// <param name="thing">The thing.</param>
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{


			float currentRadius = MutaniteCentrifuge.DANGER_RADIUS;
			if (currentRadius < 50f)
			{
				GenDraw.DrawRadiusRing(center, currentRadius);
			}


		}
	}
}