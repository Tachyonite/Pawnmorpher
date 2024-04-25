// InteractionGroupExtension.cs modified by Iron Wolf for Pawnmorph on 12/11/2019 5:47 PM
// last updated 12/11/2019  5:47 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// extension to add onto a interaction def to list possible alternatives 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class InteractionGroupExtension : DefModExtension
	{
		/// <summary>
		/// the possible interactions to use 
		/// </summary>
		[NotNull]
		public List<InteractionDef> interactions = new List<InteractionDef>();

		/// <summary>
		/// Tries to get alternative for.
		/// </summary>
		/// <param name="initiator">The initiator.</param>
		/// <param name="recipient">The recipient.</param>
		/// <returns>the alternative interaction, null if one couldn't be picked</returns>
		[CanBeNull]
		public InteractionDef TryGetAlternativeFor([NotNull] Pawn initiator, [NotNull] Pawn recipient)
		{
			if (interactions.Count == 0) return null;
			InteractionDef alt;
			interactions.Where(i => i.IsValidFor(recipient)).TryRandomElementByWeight(i => i.Worker.RandomSelectionWeight(initiator, recipient), out alt);
			return alt;
		}



	}
}