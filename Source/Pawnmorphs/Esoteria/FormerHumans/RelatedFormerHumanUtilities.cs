using System;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Letters;
using RimWorld;
using Verse;

namespace Pawnmorph.FormerHumans
{
	/// <summary>
	/// Utilities for dealing with handling former humans that are related to colonists.
	/// </summary>
	public static class RelatedFormerHumanUtilities
	{
		private const string RELATED_WILD_FORMER_HUMAN_LETTER = "RelatedWildFormerHumanContent";
		private const string RELATED_WILD_FORMER_HUMAN_LETTER_LABEL = "RelatedWildFormerHumanLabel";

		private const string RELATED_SOLD_FORMER_HUMAN_LETTER = "RelatedSoldFormerHumanContent";
		private const string RELATED_SOLD_FORMER_HUMAN_LETTER_LABEL = "RelatedSoldFormerHumanLabel";


		/// <summary>
		/// Generates a notification letter if the given wild former human is related to any colonists
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		public static void WildNotifyIfRelated(Pawn formerHuman) =>
				NotifyIfRelated(formerHuman,
						RELATED_WILD_FORMER_HUMAN_LETTER,
						RELATED_WILD_FORMER_HUMAN_LETTER_LABEL);


		/// <summary>
		/// Generates a notification letter if the given for-sale former human is related to any colonists
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		public static void ForSaleNotifyIfRelated(Pawn formerHuman) =>
				NotifyIfRelated(formerHuman,
						RELATED_SOLD_FORMER_HUMAN_LETTER,
						RELATED_SOLD_FORMER_HUMAN_LETTER_LABEL);


		/// <summary>
		/// Generates a notification letter if the given former human is related to any colonists
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		/// <param name="letterContentID">The letter content identifier.</param>
		/// <param name="letterLabelID">The letter label identifier.</param>
		public static void NotifyIfRelated(Pawn formerHuman, string letterContentID, string letterLabelID)
		{
			(var colonist, var relation) = formerHuman.GetRelatedColonistAndRelation();
			// TODO should bonds be excluded from this?
			if (relation != null && relation != PawnRelationDefOf.Bond)
			{
				string relationLabel = relation.GetGenderSpecificLabel(formerHuman);

				TaggedString letterContent = letterContentID.Translate(formerHuman.Named("formerHuman"),
																	   colonist.Named("relatedPawn"),
																	   relationLabel.Named("relationship"));
				TaggedString letterLabel = letterLabelID.Translate(formerHuman.Named("formerHuman"),
																   colonist.Named("relatedPawn"),
																   relationLabel.Named("relationship"));
				Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent, formerHuman,
											   formerHuman.HostFaction);
			}
		}

		/// <summary>
		/// Generates an offer quest from this former human to join the colony
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		public static void OfferJoinColonyIfRelated([NotNull] Pawn formerHuman)
		{
			if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));

			StringBuilder debugString = new StringBuilder();

			// Don't let former humans offer to join if they're in trade ships,
			// dead, part of the colony, etc.
			if (!EligableToJoinColony(formerHuman))
			{
				DebugLogUtils.Pedantic($"{formerHuman.Name?.ToStringFull ?? formerHuman.Label} not eligable to join the colony");
				return;
			}

			(var colonist, var relation) = formerHuman.GetRelatedColonistAndRelation();

			if (relation != null)
				debugString.AppendLine($"found {relation.label} for {formerHuman.Name?.ToStringFull ?? formerHuman.Label}");
			else
				debugString.AppendLine($"no relation for ");

			// TODO should bonds be excluded from this?
			if (relation != null && relation != PawnRelationDefOf.Bond)
			{
				// TODO this doesn't work for newly-generated former humans.
				// Need to refactor the way they're generated to ensure the sapience tracker is in a good state
				SapienceLevel? qSapience = formerHuman.GetQuantizedSapienceLevel();
				debugString.AppendLine($"{formerHuman.Name?.ToStringFull ?? formerHuman.Label} sapience level is {qSapience}");

				if (qSapience <= SapienceLevel.Conflicted
				) //sapience level enum is in reverse order. Sapient < Feral 
				{
					debugString.AppendLine($"Generating sapient letter for {formerHuman.Name?.ToStringFull ?? formerHuman.Label}");
					ChoiceLetter_FormerHumanJoins.SendSapientLetterFor(formerHuman, colonist, relation);
				}
				else
				{
					debugString.AppendLine($"Generating feral letter for {formerHuman.Name?.ToStringFull ?? formerHuman.Label}");
					ChoiceLetter_FormerHumanJoins.SendFeralLetterFor(formerHuman, colonist, relation);
				}
			}

			DebugLogUtils.Pedantic(debugString.ToString());
		}

		/// <summary>
		/// Whether or not the former human is capable of joining the colony
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		public static bool EligableToJoinColony(Pawn formerHuman) // TODO this probably should go somewhere else after FormerHumanUtilities is refactored
		{
			return !formerHuman.DestroyedOrNull()
				&& !formerHuman.Dead
				&& formerHuman.Spawned
				&& formerHuman.Faction == null;
		}

		/// <summary>
		/// Causes the former human to join the colony
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		public static void JoinColony(Pawn formerHuman) // TODO this probably should go somewhere else after FormerHumanUtilities is refactored
		{
			//TODO add rescue thoughts and things here
			formerHuman.SetFaction(Faction.OfPlayer);
		}
	}
}
