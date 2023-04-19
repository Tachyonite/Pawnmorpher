using Pawnmorph.FormerHumans;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// Comp for Pawn Things that can make their associated pawn a former human
	/// </summary>
	public class Comp_CanBeFormerHuman : ThingComp, IMentalStateRecoveryReceiver
	{
		private bool triggered = false;

		private CompProperties_CanBeFormerHuman Props => props as CompProperties_CanBeFormerHuman;
		private Pawn Pawn => parent as Pawn;

		/// <summary>
		/// Called after the parent thing is spawned
		/// </summary>
		/// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (respawningAfterLoad) return;

			if (parent?.def?.IsChaomorph() == true)
				LessonAutoActivator.TeachOpportunity(PMConceptDefOf.Chaomorphs, OpportunityType.GoodToKnow);
		}

		/// <summary>
		/// Called every tick
		/// </summary>
		public override void CompTick() //doing the check here and not on post spawn because post spawn is called before the manhunter mental state is added 
		{
			base.CompTick();

			// Make the animal a former human on the first tick rather than on spawning
			//
			if (!triggered)
			{
				triggered = true;

				if (ShouldMakeFormerHuman())
				{
					bool isManhunter = Pawn.MentalStateDef == MentalStateDefOf.Manhunter
									|| Pawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent;

					// Strong bias towards feral sapience for former humans spawned "in the wild".
					// Reasoning: They have spent so much time wandering and living in the wild
					// before they encounter your colony that their grip on sapience is much more likely than not slipping.
					float sapience = RandUtilities.generateBetaRandom(1.5f, 4.5f);
					FormerHumanUtilities.MakeAnimalSapient(Pawn, sapience, !isManhunter);
					if (isManhunter)
						// TODO this will only ever fire once, even if the pawn shows up again later
						RelatedFormerHumanUtilities.WildNotifyIfRelated(Pawn);

					FormerHumanUtilities.InvalidateIntelligence(Pawn);
				}
			}
		}

		/// <summary>
		/// Whether or not this pawn can be a former human
		/// </summary>
		/// <returns><c>true</c>, if the pawn is eligable, <c>false</c> otherwise.</returns>
		private bool CanBeFormerHuman()
		{
			var pawn = Pawn;
			if (!PawnmorpherMod.Settings.enableWildFormers)
				return false;

			if (parent.def.IsValidFormerHuman() == false)
				return false;

			// Don't make animals belonging to any faction former humans
			if (pawn.Faction != null)
				return false;

			// Don't make animals with existing relationships to other animals former humans
			if (pawn.relations?.DirectRelations
					.Any(r => r.def == PawnRelationDefOf.Child
						   || r.def == PawnRelationDefOf.Parent) ?? false)
				return false;

			// Don't let manhunters be former humans
			if (Pawn.MentalStateDef == MentalStateDefOf.Manhunter
			 || Pawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent)
				return false;

			// Make sure the animal is old enough to be a former human
			if (TransformerUtility.ConvertAge(pawn, ThingDefOf.Human.race) < FormerHumanUtilities.MIN_FORMER_HUMAN_AGE)
				return false;

			return true;
		}

		/// <summary>
		/// Whether to make this pawn a former human or not
		/// </summary>
		/// <returns><c>true</c>, if the pawn should be made a former human, <c>false</c> otherwise.</returns>
		private bool ShouldMakeFormerHuman()
		{
			// Don't make a pawn former human twice
			if (Pawn.IsFormerHuman())
				return false;

			// Always-former-human animals skip the can-be check
			if (Props?.Always == true)
				return true;

			// Check if the animal is suitable to be a former human
			if (!CanBeFormerHuman())
				return false;

			return Rand.Value < PawnmorpherMod.Settings.formerChance;
		}

		/// <summary>
		/// Exposes the comp data to be saved/loaded from XML
		/// </summary>
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref triggered, nameof(triggered));
		}

		/// <summary>
		/// Called when the pawn recovered from the given mental state.
		/// </summary>
		/// <param name="mentalState">State of the mental.</param>
		public void OnRecoveredFromMentalState(MentalState mentalState)
		{
			// Let former-human manhunters attempt to join the colony after they recover from manhunting
			if (mentalState.def == MentalStateDefOf.ManhunterPermanent || mentalState.def == MentalStateDefOf.Manhunter)
			{
				RelatedFormerHumanUtilities.OfferJoinColonyIfRelated(Pawn);
			}
		}
	}
}