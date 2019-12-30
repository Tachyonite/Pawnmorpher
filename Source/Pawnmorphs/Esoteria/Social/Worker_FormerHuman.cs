// Worker_FormerHuman.cs modified by Iron Wolf for Pawnmorph on 12/10/2019 6:08 PM
// last updated 12/10/2019  6:09 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Social
{
    /// <summary>
    ///     interaction worker for former human recipients
    /// </summary>
    /// <seealso cref="RimWorld.InteractionWorker" />
    public class Worker_FormerHuman : InteractionWorker
    {
        private const float BASE_INTERACTION_CHANCE = 99999;

        /// <summary>
        ///     Gets the base interaction chance.
        /// </summary>
        /// <value>
        ///     The base interaction chance.
        /// </value>
        protected virtual float BaseInteractionChance => BASE_INTERACTION_CHANCE;


        /// <summary>
        /// called when the initiator interacts with the specified recipient.
        /// </summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <param name="extraSentencePacks">The extra sentence packs.</param>
        /// <param name="letterText">The letter text.</param>
        /// <param name="letterLabel">The letter label.</param>
        /// <param name="letterDef">The letter definition.</param>
        public override void Interacted(Pawn initiator,  Pawn recipient, [NotNull] List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel,
                                        out LetterDef letterDef)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            var saLevel = recipient?.GetQuantizedSapienceLevel();
            if (saLevel == null) return;
            var saVariants = interaction.GetModExtension<SapientRulePackVariant>();
            var rulePackVariant = saVariants?.GetRulePackVariant(saLevel.Value);//check if any variants are attached, if so add them to extra rule packs 
            if (rulePackVariant != null)
            {
                extraSentencePacks.Add(rulePackVariant);
            }
        }

        /// <summary>
        /// gets the random selection weight for the initiator and recipient interacting 
        /// </summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        public override float RandomSelectionWeight([NotNull] Pawn initiator, Pawn recipient)
        {


            FormerHumanStatus? fHumanStatus = recipient.GetFormerHumanStatus();
            if (fHumanStatus == null) return 0; //only allow for former human recipients 
            Log.Message($"Checking interaction of {initiator.Name} -> {recipient.Name}");

            Filter<FormerHumanStatus> fHumanRestriction =
                interaction.GetModExtension<FormerHumanRestriction>()?.filter ?? new Filter<FormerHumanStatus>();
            var relationshipInteractionRestriction = interaction.GetModExtension<RelationshipInteractionRestriction>();
            Filter<PawnRelationDef> relationRestriction = relationshipInteractionRestriction?.relationFilter;
            bool mustBeColonist = relationshipInteractionRestriction?.mustBeColonist ?? false;
            if (!fHumanRestriction.PassesFilter(fHumanStatus.Value))
                return 0; //make sure they're at the correct stage for the interaction 

            Pawn oHuman = FormerHumanUtilities.GetOriginalPawnOfFormerHuman(recipient);
            if (relationRestriction != null) //check any relationships if applicable 
            {
                if (oHuman == null)
                {
                    if (mustBeColonist) return 0; //if there is no original pawn they can't be a colonist 

                    if (!relationRestriction.isBlackList)
                        return
                            0; //if the filter is a white list a blank original pawn can't pass it because there is no relationship 
                    return BaseInteractionChance;
                }


                foreach (PawnRelationDef pawnRelationDef in initiator.GetRelations(oHuman))
                    if (relationRestriction.PassesFilter(pawnRelationDef))
                        return BaseInteractionChance;

                return 0; //none passed 
            }

            return BaseInteractionChance;
        }
    }
}