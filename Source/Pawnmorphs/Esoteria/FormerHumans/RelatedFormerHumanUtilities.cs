﻿using Pawnmorph.Letters;
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
        public static void OfferJoinColonyIfRelated(Pawn formerHuman)
        {
            (var colonist, var relation) = formerHuman.GetRelatedColonistAndRelation();
            // TODO should bonds be excluded from this?
            if (relation != null && relation != PawnRelationDefOf.Bond)
            {
                if (formerHuman.GetQuantizedSapienceLevel() > SapienceLevel.Conflicted)
                    ChoiceLetter_FormerHumanJoins.SendSapientLetterFor(formerHuman, colonist, relation);
                else
                    ChoiceLetter_FormerHumanJoins.SendFeralLetterFor(formerHuman, colonist, relation);
            }
        }
    }
}