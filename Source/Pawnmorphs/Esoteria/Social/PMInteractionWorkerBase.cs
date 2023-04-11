// MPInteractionWorkerBase.cs modified by Iron Wolf for Pawnmorph on 08/30/2019 8:50 AM
// last updated 08/30/2019  8:50 AM

using System;
using RimWorld;
using Verse;

namespace Pawnmorph.Social
{
	/// <summary>
	/// base class for all Pawnmorph interaction workers
	/// </summary>
	/// <seealso cref="RimWorld.InteractionWorker" />
	public abstract class PMInteractionWorkerBase : InteractionWorker
	{
		/// <summary>Gets the interaction definition.</summary>
		/// <value>The definition.</value>
		public PMInteractionDef Def
		{
			get
			{
				try
				{
					return (PMInteractionDef)interaction;
				}
				catch (InvalidCastException)
				{
					Log.Error($"could not cast def of type {interaction.GetType().Name} to {nameof(PMInteractionDef)}");
					throw;
				}
			}
		}



		/// <summary>
		/// Gets the base weight for the given initiator and recipient pawns 
		/// </summary>
		/// <param name="initiator">The initiator.</param>
		/// <param name="recipient">The recipient.</param>
		/// <returns></returns>
		protected float GetBaseWeight(Pawn initiator, Pawn recipient)
		{
			if (initiator == recipient)
				return 0;

			var initiatorWeight = Def.initiatorWeights?.GetTotalWeight(initiator) ?? 0;
			var recipientWeight = Def.recipientWeights?.GetTotalWeight(recipient) ?? 0;
			if (Def.requiresBoth && (initiatorWeight <= 0 || recipientWeight <= 0))
				return 0;

			return (initiatorWeight + recipientWeight) * Def.weightMultiplier;
		}
	}
}