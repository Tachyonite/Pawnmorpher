// Def_PMInteraction.cs modified by Iron Wolf for Pawnmorph on 08/30/2019 8:46 AM
// last updated 08/30/2019  8:46 AM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.Social
{
	/// <summary>
	/// def for pawnmorph specific interactions 
	/// </summary>
	/// <seealso cref="RimWorld.InteractionDef" />
	public class PMInteractionDef : InteractionDef
	{
		/// <summary>Get all Configuration Errors with this instance</summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors())
				yield return configError;

			if (!(Worker is PMInteractionWorkerBase))
				yield return "Worker type is not derived from " + nameof(PMInteractionWorkerBase);

			if (initiatorWeights == null && recipientWeights == null)
				yield return "Neither initator nor recipient weights are defined";
		}

		/// <summary>The weights applied based on the initator's mutations</summary>
		public PMInteractionWeightsDef initiatorWeights;

		/// <summary>The weights applied based on the recipients's mutations</summary>
		public PMInteractionWeightsDef recipientWeights;

		/// <summary>An additional multiplier on the weight of the interaction</summary>
		public float weightMultiplier = 1f;

		/// <summary>if both the initiator and recipient need to have non-zero weights for the resultant weight to be non zero </summary>
		public bool requiresBoth;

		/// <summary>
		/// Gets the modified interaction weight for the given initiator and recipient pawns 
		/// </summary>
		/// <param name="initiator">The initiator.</param>
		/// <param name="recipient">The recipient.</param>
		/// <returns></returns>
		public float GetInteractionWeight(Pawn initiator, Pawn recipient)
		{
			var initiatorWeight = initiatorWeights?.GetTotalWeight(initiator) ?? 0;
			var recipientWeight = recipientWeights?.GetTotalWeight(recipient) ?? 0;
			if (requiresBoth && (initiatorWeight <= 0 || recipientWeight <= 0))
				return 0;

			return (initiatorWeight + recipientWeight) * weightMultiplier;
		}
	}
}