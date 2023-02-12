// RelationshipInteractionRestriction.cs modified by Iron Wolf for Pawnmorph on 12/10/2019 6:01 PM
// last updated 12/10/2019  6:01 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// mod extension to add a restriction to a <see cref="InteractionDef"/> based on the relationship status of the recipient 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class RelationshipInteractionRestriction : DefModExtension
	{
		/// <summary>
		/// if true, then check the past human pawn for a relationship if the recipient is a former human 
		/// </summary>
		public bool checkFormerHuman;

		/// <summary>
		/// if true, the recipient must be a colonist 
		/// </summary>
		public bool mustBeColonist;

		/// <summary>
		/// The relationship 
		/// </summary>
		[NotNull]
		public Filter<PawnRelationDef> relationFilter;

		/// <summary>
		/// Gets all configuration errors with this instance.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors())
			{
				yield return configError;
			}

			if (relationFilter == null)
				yield return "relationship is not set";
		}
	}
}