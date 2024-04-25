// TFTransferable.cs created by Iron Wolf for Pawnmorph on 06/26/2020 2:41 PM
// last updated 06/26/2020  2:41 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// mod extension to mark something like a HediffDef or TraitDef so it will be preserved
	/// when the pawn transforms to/from an animal 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class TFTransferable : DefModExtension
	{


		/// <summary>
		/// The race filter
		/// </summary>
		[CanBeNull]
		public Filter<ThingDef> raceFilter;


		/// <summary>
		/// Determines whether this instance with the specified dir can transfer onto the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this instance with the specified dir can transfer onto the given pawn ; otherwise, <c>false</c>.
		/// </returns>
		public bool CanTransfer([NotNull] Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return raceFilter?.PassesFilter(pawn.def) != false;
		}

	}
}