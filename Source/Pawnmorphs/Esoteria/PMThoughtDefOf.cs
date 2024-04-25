// PMThoughtDefOf.cs modified by Iron Wolf for Pawnmorph on 09/28/2019 8:35 AM
// last updated 09/28/2019  8:35 AM

using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	/// <summary> DefOf class for commonly referenced ThoughtDefs. </summary>
	[DefOf]
	public static class PMThoughtDefOf
	{
		static PMThoughtDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMThoughtDefOf));
		}

		public static ThoughtDef SapientAnimalGotSomeSnuggling;

		public static ThoughtDef DefaultMorphTfMemory;

		/// <summary> Default thought for pawns that were a morph that reverts back to a human. </summary>
		public static ThoughtDef DefaultMorphRevertsToHuman;


		/// <summary>
		/// The former human taming success thought
		/// </summary>
		public static ThoughtDef FormerHumanTameThought;

		/// <summary>
		/// default thought for when a sapient animal sleeps on the ground 
		/// </summary>
		public static ThoughtDef SapientAnimalSleptOnGround;
		/// <summary>
		/// default thought for when a sapient animal is milked 
		/// </summary>
		public static ThoughtDef SapientAnimalMilked;

		/// <summary>
		/// The sapient animal hunting memory
		/// </summary>
		/// this is for hunting out of necessity not for the hunting mental break 
		public static ThoughtDef SapientAnimalHuntingMemory;


		/// <summary>
		/// The sapient animal hunting memory primal wish
		/// </summary>
		/// this is for hunting out of necessity not for the hunting mental break but for primal wish pawns only 
		public static ThoughtDef SapientAnimalHuntingMemoryPrimalWish;

		/// <summary>
		/// The default thought for former humans that have bad thoughts for eating meat of the same species they are 
		/// </summary>
		public static ThoughtDef FHDefaultCannibalThought_Direct;

		/// <summary>
		/// The fh default cannibal thought ingredient
		/// </summary>
		public static ThoughtDef FHDefaultCannibalThought_Ingredient;

		public static ThoughtDef FHDefaultCannibalGoodThought_Direct;
		public static ThoughtDef FHDefaultCannibalGoodThought_Ingredient;


		public static ThoughtDef PM_WitnessedAllyTf;
		public static ThoughtDef PM_WitnessedNonAllyTf;
		public static ThoughtDef PM_WitnessedRivalTf;
		public static ThoughtDef PM_WitnessedFriendTf;

		// Accepting and rejecting related former humans
		public static ThoughtDef PMFormerHumanAccepted_VeryClose;
		public static ThoughtDef PMFormerHumanAccepted_Close;
		public static ThoughtDef PMFormerHumanAccepted_Moderate;
		public static ThoughtDef PMFormerHumanAccepted_Distant;

		public static ThoughtDef PMFormerHumanRejected_VeryClose;
		public static ThoughtDef PMFormerHumanRejected_Close;
		public static ThoughtDef PMFormerHumanRejected_Moderate;
		public static ThoughtDef PMFormerHumanRejected_Distant;
	}
}