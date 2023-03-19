// InteractionWorkers.cs modified by Iron Wolf for Pawnmorph on 08/30/2019 9:14 AM
// last updated 08/30/2019  9:14 AM

using RimWorld;
using Verse;

namespace Pawnmorph.Social
{
	/// <summary>
	/// Abstract base class for all PMInteractionWorkers that work like base-game
	/// interactions
	/// </summary>
	public abstract class PMInteractionWorker_BaseGame : PMInteractionWorkerBase
	{
		/// <summary>
		/// The base interaction worker this def is based on
		/// </summary>
		/// <value>The base worker.</value>
		/// <seealso cref="Pawnmorph.Social.PMInteractionWorkerBase" />
		protected abstract InteractionWorker BaseWorker { get; }

		/// <summary>
		/// Gets the random selection weight for this interaction.
		/// </summary>
		/// <param name="initiator">The initiator.</param>
		/// <param name="recipient">The recipient.</param>
		/// <returns>The selection weight.</returns>
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return BaseWorker.RandomSelectionWeight(initiator, recipient)
					* Def.GetInteractionWeight(initiator, recipient);
		}
	}

	/// <summary>
	/// Interaction worker that functions like chitchat worker 
	/// </summary>
	public class InteractionWorker_Chitchat : PMInteractionWorker_BaseGame
	{
		private readonly InteractionWorker baseWorker = new RimWorld.InteractionWorker_Chitchat();

		/// <summary>
		/// The base interaction worker this def is based on
		/// </summary>
		/// <value>The base worker.</value>
		protected override InteractionWorker BaseWorker => baseWorker;
	}

	/// <summary>
	/// Interaction worker that functions like InteractionWorker_DeepTalk
	/// </summary>
	public class InteractionWorker_DeepTalk : PMInteractionWorker_BaseGame
	{
		private readonly InteractionWorker baseWorker = new RimWorld.InteractionWorker_DeepTalk();

		/// <summary>
		/// The base interaction worker this def is based on
		/// </summary>
		/// <value>The base worker.</value>
		protected override InteractionWorker BaseWorker => baseWorker;
	}

	/// <summary>
	/// Interaction worker that functions like InteractionWorker_KindWords
	/// </summary>
	public class InteractionWorker_KindWords : PMInteractionWorker_BaseGame
	{
		private readonly InteractionWorker baseWorker = new RimWorld.InteractionWorker_KindWords();

		/// <summary>
		/// The base interaction worker this def is based on
		/// </summary>
		/// <value>The base worker.</value>
		protected override InteractionWorker BaseWorker => baseWorker;
	}

	/// <summary>
	/// Interaction worker that functions like InteractionWorker_Slight 
	/// </summary>
	public class InteractionWorker_Slight : PMInteractionWorker_BaseGame
	{
		private readonly InteractionWorker baseWorker = new RimWorld.InteractionWorker_Slight();

		/// <summary>
		/// The base interaction worker this def is based on
		/// </summary>
		/// <value>The base worker.</value>
		protected override InteractionWorker BaseWorker => baseWorker;
	}

	/// <summary>
	/// Interaction worker that works like base InteractionWorker_Insult
	/// </summary>
	public class InteractionWorker_Insult : PMInteractionWorker_BaseGame
	{
		private readonly InteractionWorker baseWorker = new RimWorld.InteractionWorker_Insult();

		/// <summary>
		/// The base interaction worker this def is based on
		/// </summary>
		/// <value>The base worker.</value>
		protected override InteractionWorker BaseWorker => baseWorker;
	}

}