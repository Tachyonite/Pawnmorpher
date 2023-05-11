// FormerHumanRestrction.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 3:00 PM
// last updated 12/07/2019  3:00 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
	public class FormerHumanRestriction : RestrictionExtension
	{
		/// <summary>
		/// The filter for specific kinds of former humans 
		/// </summary>
		[NotNull]
		public Filter<SapienceLevel> filter = new Filter<SapienceLevel>();

		/// <summary>
		/// The race filter
		/// </summary>
		[NotNull] public Filter<ThingDef> raceFilter = new Filter<ThingDef>();

		/// <summary>
		/// if true, then to use the attached def the pawn must be a former human 
		/// </summary>
		public bool mustBeFormerHuman = true;

		/// <summary>
		/// checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		/// if the def can be used with the given pawn
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected override bool PassesRestrictionImpl(Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			var fHumanStatus = pawn.GetQuantizedSapienceLevel();
			if (fHumanStatus == null)
			{
				return !mustBeFormerHuman;
			}

			return filter.PassesFilter(fHumanStatus.Value) && raceFilter.PassesFilter(pawn.def);
		}
	}
}