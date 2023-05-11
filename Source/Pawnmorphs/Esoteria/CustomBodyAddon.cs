// CustomBodyAddon.cs modified by Iron Wolf for Pawnmorph on 11/02/2019 6:31 PM
// last updated 11/02/2019  6:31 PM

using AlienRace;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// custom body addon that only shows up on specific body types 
	/// </summary>
	/// <seealso cref="AlienRace.AlienPartGenerator.BodyAddon" />
	public class CustomBodyAddon : AlienPartGenerator.BodyAddon
	{
		/// <summary>
		/// filter that specifies what kind of body types this addon will be drawn on 
		/// </summary>
		public Filter<BodyTypeDef> bodyFilter;

		/// <summary>
		/// Determines whether this instance can draw on the given pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this instance can be draw on the specified pawn; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanDrawAddon(Pawn pawn)
		{
			if (!base.CanDrawAddon(pawn)) return false;
			return bodyFilter == null || bodyFilter.PassesFilter(pawn.story.bodyType);
		}
	}
}