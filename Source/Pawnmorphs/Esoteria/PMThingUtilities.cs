// PMThingUtilities.cs modified by Iron Wolf for Pawnmorph on 02/05/2020 7:28 PM
// last updated 02/05/2020  7:28 PM

using System;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static class for various thing/pawn related utilities
	/// </summary>
	public static class PMThingUtilities
	{
		/// <summary>
		/// Gets the correct position, taking account of whether or not this thing is held by something 
		/// </summary>
		/// <param name="thing">The thing.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">thing</exception>
		public static IntVec3 GetCorrectPosition([NotNull] this Thing thing)
		{
			if (thing == null) throw new ArgumentNullException(nameof(thing));

			return thing.holdingOwner == null ? thing.Position : thing.PositionHeld;
		}

		/// <summary>
		/// Gets a debug label for the given thing. useful for debug printing 
		/// </summary>
		/// <param name="thing">The thing.</param>
		/// <returns></returns>
		public static string GetDebugLabel([NotNull] Thing thing)
		{
			return $"[{thing.Label},{thing.def.defName},{thing.thingIDNumber}]";
		}

		/// <summary>
		/// Gets the correct map, taking account of whether or not this thing is held by something 
		/// </summary>
		/// <param name="thing">The thing.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">thing</exception>
		[CanBeNull]
		public static Map GetCorrectMap([NotNull] this Thing thing)
		{
			if (thing == null) throw new ArgumentNullException(nameof(thing));
			return thing.Map ?? thing.MapHeld;
		}
	}
}