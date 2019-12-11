// Worker_FormerHuman.cs modified by Iron Wolf for Pawnmorph on 12/10/2019 6:08 PM
// last updated 12/10/2019  6:09 PM

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