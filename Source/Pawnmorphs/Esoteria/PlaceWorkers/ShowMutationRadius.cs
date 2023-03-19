// ShowMutationRadius.cs created by Iron Wolf for Pawnmorph on 02/22/2021 5:50 PM
// last updated 02/22/2021  5:50 PM

using UnityEngine;
using Verse;

namespace Pawnmorph.PlaceWorkers
{
	/// <summary>
	/// place worker for showing the current radius of a mutagenic ship 
	/// </summary>
	/// <seealso cref="Verse.PlaceWorker" />
	public class ShowMutationRadius : PlaceWorker
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
			base.DrawGhost(def, center, rot, ghostCol, thing);

			var mutComp = thing?.TryGetComp<CompMutagenicRadius>();
			if (mutComp != null)
			{
				float currentRadius = mutComp.Radius;
				if (currentRadius < 50f)
				{
					GenDraw.DrawRadiusRing(center, currentRadius);
				}
			}

		}
	}
}