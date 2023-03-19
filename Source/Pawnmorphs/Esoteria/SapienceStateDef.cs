// SapienceStateDef.cs created by Iron Wolf for Pawnmorph on 04/24/2020 7:35 AM
// last updated 04/24/2020  7:35 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// def for specific state a pawns 'sapience/mind' can be in, such as FormerHuman, Animalistic, ect.
	/// </summary>
	/// <seealso cref="Verse.Def" />
	public class SapienceStateDef : Def
	{
		/// <summary>
		/// The state type
		/// </summary>
		public Type stateType;

		/// <summary>
		/// the hediff the pawn is given when in this sapience state
		/// </summary>
		[CanBeNull]
		public HediffDef forcedHediff;

		/// <summary>
		/// if a pawn in this state can go permanently feral
		/// </summary>
		public bool canGoPermanentlyFeral;

		/// <summary>
		/// Creates a new state instance.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public SapienceState CreateState()
		{
			try
			{
				var state = (SapienceState)Activator.CreateInstance(stateType);
				state.SetDef(this);
				return state;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"caught {e.GetType().Name} while trying to create new {stateType?.Name ?? "NULL"}!", e);
			}
		}

		/// <summary>
		/// Gets all configuration errors with this instance 
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors())
			{
				yield return configError;
			}

			if (stateType == null) yield return "no sapience type set!";
			else if (!typeof(SapienceState).IsAssignableFrom(stateType))
				yield return $"{stateType.Name} is not a subtype of {nameof(SapienceState)}!";

		}

	}
}