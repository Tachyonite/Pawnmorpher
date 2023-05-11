// SapienceStateRestriction.cs created by Iron Wolf for Pawnmorph on 04/25/2020 4:28 PM
// last updated 04/25/2020  4:28 PM

using System;
using System.Collections.Generic;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// restriction so limit defs to pawns in certain sapience states 
	/// </summary>
	/// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
	public class SapienceStateRestriction : RestrictionExtension
	{
		/// <summary>
		/// Configurations the errors.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors())
			{
				yield return configError;
			}
			if (state == null) yield return "no sapience state def set!";
		}

		/// <summary>
		/// The state the pawn must be in 
		/// </summary>
		public SapienceStateDef state;

		/// <summary>
		/// The sapience filter
		/// </summary>
		public Filter<SapienceLevel> sapienceFilter;

		/// <summary>
		///     checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     if the def can be used with the given pawn
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected override bool PassesRestrictionImpl(Pawn pawn)
		{
			var stateDef = pawn.GetSapienceState()?.StateDef;
			if (stateDef != state) return false;
			return sapienceFilter?.PassesFilter(pawn.GetQuantizedSapienceLevel() ?? SapienceLevel.PermanentlyFeral) != false;
		}
	}
}