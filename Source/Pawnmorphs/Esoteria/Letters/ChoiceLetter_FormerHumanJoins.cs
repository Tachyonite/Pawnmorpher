using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace Pawnmorph.Letters
{
    /// <summary>
    /// A choice letter for a former human attempting to join the colony
    /// </summary>
    public class ChoiceLetter_FormerHumanJoins : ChoiceLetter
    {
        private const string LABEL = "PMLetterFormerHumanJoinLabel";
        private const string TITLE = "PMLetterFormerHumanJoinTitle";
        private const string TEXT = "PMLetterFormerHumanJoinContent";

        private const string FERAL_LABEL = "PMLetterFormerHumanJoinLabel";
        private const string FERAL_TITLE = "PMLetterFormerHumanJoinTitle";
        private const string FERAL_TEXT = "PMLetterFormerHumanJoinContent";

        private Pawn formerHuman;
        private Pawn relative;
        private PawnRelationDef relation;

        /// <summary>
        /// Sends a join request for the given sapient former human
        /// </summary>
        /// <param name="formerHuman">Former human.</param>
        /// <param name="relative">Relative.</param>
        /// <param name="relation">Relation.</param>
        public static void SendSapientLetterFor(Pawn formerHuman, Pawn relative, PawnRelationDef relation)
        {
            SendLetterFor(formerHuman, relative, relation, LABEL, TITLE, TEXT);
        }

        /// <summary>
        /// Sends a join request for the given feral former human
        /// </summary>
        /// <param name="formerHuman">Former human.</param>
        /// <param name="relative">Relative.</param>
        /// <param name="relation">Relation.</param>
        public static void SendFeralLetterFor(Pawn formerHuman, Pawn relative, PawnRelationDef relation)
        {
            SendLetterFor(formerHuman, relative, relation, FERAL_LABEL, FERAL_TITLE, FERAL_TEXT);
        }

        /// <summary>
        /// Sends a join request for the given former human
        /// </summary>
        /// <param name="formerHuman">Former human.</param>
        /// <param name="relative">Relative.</param>
        /// <param name="relation">Relation.</param>
        /// <param name="labelId">ID of the label string.</param>
        /// <param name="titleId">ID of the title string.</param>
        /// <param name="textId">ID of the text string</param>
        public static void SendLetterFor(Pawn formerHuman, Pawn relative, PawnRelationDef relation, string labelId, string titleId, string textId)
        {
            var label = labelId.Translate(formerHuman.Named("formerHuman"),
                                          relative.Named("relatedPawn"),
                                          relation.GetGenderSpecificLabel(formerHuman).Named("relationship"));
            var title = titleId.Translate(formerHuman.Named("formerHuman"),
                                        relative.Named("relatedPawn"),
                                        relation.GetGenderSpecificLabel(formerHuman).Named("relationship"));
            var text = textId.Translate(formerHuman.Named("formerHuman"),
                                        relative.Named("relatedPawn"),
                                        relation.GetGenderSpecificLabel(formerHuman).Named("relationship"));
            // Resolve the grammar rules
            label = label.AdjustedFor(formerHuman, "formerHuman");
            title = title.AdjustedFor(formerHuman, "formerHuman");
            text = text.AdjustedFor(formerHuman, "formerHuman");

            QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(formerHuman), ref text);
            TaggedString _;
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref _, formerHuman);


            ChoiceLetter_FormerHumanJoins letter = (ChoiceLetter_FormerHumanJoins)
                    LetterMaker.MakeLetter(label, text, PMLetterDefOf.PMFormerHumanJoinRequest, new LookTargets(formerHuman));

            letter.title = title;
            letter.radioMode = true;
            letter.formerHuman = formerHuman;
            letter.relative = relative;
            letter.relation = relation;
            letter.StartTimeout(60000);

            Find.LetterStack.ReceiveLetter(letter, null);
        }
        /// <summary>
        /// Whether this letter can be dismissed with a right click
        /// </summary>
        /// <value><c>true</c> if can dismiss with right click; otherwise, <c>false</c>.</value>
        public override bool CanDismissWithRightClick => false;

        /// <summary>
        /// Whether this letter is still valid to be shown on the stack
        /// </summary>
        /// <value><c>true</c> if can show in letter stack; otherwise, <c>false</c>.</value>
        public override bool CanShowInLetterStack
        {
            get
            {
                if (!base.CanShowInLetterStack)
                    return false;
                return FormerHumanStillJoinable();
            }
        }

        /// <summary>
        /// The possible choices of the letter
        /// </summary>
        /// <value>The choices.</value>
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                }
                else
                {
                    if (lookTargets.IsValid())
                        yield return Option_JumpToLocationAndPostpone;

                    yield return Option_Accept;
                    yield return Option_RejectWithCharityConfirmation;
                    yield return Option_Postpone;
                }
            }
        }

        private DiaOption Option_Accept
        {
            get
            {
                DiaOption diaOption = new DiaOption("AcceptButton".Translate())
                {
                    action = delegate
                    {
                        formerHuman.SetFaction(Faction.OfPlayer);
                        CameraJumper.TryJump(formerHuman.Position, formerHuman.Map);
                        Find.LetterStack.RemoveLetter(this);

                        if (ModsConfig.IdeologyActive)
                            HistoryEventDefOf.CharityFulfilled_WandererJoins.SendEvent();
                    },
                    resolveTree = true
                };

                if (!FormerHumanStillJoinable())
                    diaOption.Disable(null);
                return diaOption;
            }
        }

        private DiaOption Option_RejectWithCharityConfirmation => new DiaOption("RejectLetter".Translate())
        {
            action = delegate {
                void Reject()
                {
                    Find.LetterStack.RemoveLetter(this);
                }

                if (!ModsConfig.IdeologyActive)
                {
                    Reject();
                }
                else
                {
                    // Confirm if any pawn has a charity ideology
                    IEnumerable<Pawn> source = IdeoUtility.AllColonistsWithCharityPrecept();
                    if (source.Any())
                    {
                        string dialogText = "";
                        foreach (IGrouping<Ideo, Pawn> item in from c in source
                                                               group c by c.Ideo)
                        {
                            dialogText += "\n  - " + "BelieversIn".Translate(item.Key.name.Colorize(item.Key.TextColor), item.Select((Pawn f) => f.NameShortColored.Resolve()).ToCommaList());
                        }
                        var confirm = Dialog_MessageBox.CreateConfirmation("ConfirmationCharityJoiner".Translate(dialogText), Reject);
                        Find.WindowStack.Add(confirm);
                    }
                    else
                    {
                        Reject();
                    }
                }
            },
            resolveTree = true
        };

        /// <summary>
        /// Called after the letter is removed
        /// </summary>
        public override void Removed()
        {
            base.Removed();

            // Make sure we always send the charity signal and add thoughts, regardless of why the letter was removed
            // If you ignore their request for help and they get eaten by a predator, that still counts as rejecting them :D
            if (formerHuman.Faction == Faction.OfPlayer)
            {
                // Gain a thought for helping family
                ThoughtDef thought;
                switch (GetCloseness())
                {
                    case RelationCloseness.VeryClose:
                        thought = PMThoughtDefOf.PMFormerHumanAccepted_VeryClose;
                        break;
                    case RelationCloseness.Close:
                        thought = PMThoughtDefOf.PMFormerHumanAccepted_Close;
                        break;
                    case RelationCloseness.Moderate:
                        thought = PMThoughtDefOf.PMFormerHumanAccepted_Moderate;
                        break;
                    default:
                        thought = PMThoughtDefOf.PMFormerHumanAccepted_Distant;
                        break;
                }
                relative?.needs?.mood?.thoughts?.memories?.TryGainMemory(thought, formerHuman);

                if (ModsConfig.IdeologyActive)
                    HistoryEventDefOf.CharityFulfilled_WandererJoins.SendEvent();
            }
            else
            {
                // Gain a thought for not helping family
                ThoughtDef thought;
                switch (GetCloseness())
                {
                    case RelationCloseness.VeryClose:
                        thought = PMThoughtDefOf.PMFormerHumanRejected_VeryClose;
                        break;
                    case RelationCloseness.Close:
                        thought = PMThoughtDefOf.PMFormerHumanRejected_Close;
                        break;
                    case RelationCloseness.Moderate:
                        thought = PMThoughtDefOf.PMFormerHumanRejected_Moderate;
                        break;
                    default:
                        thought = PMThoughtDefOf.PMFormerHumanRejected_Distant;
                        break;
                }
                relative?.needs?.mood?.thoughts?.memories?.TryGainMemory(thought, formerHuman);

                if (ModsConfig.IdeologyActive)
                    HistoryEventDefOf.CharityRefused_WandererJoins.SendEvent();
            }

        }

        /// <summary>
        /// Exposes the data to/from XML for saving.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref formerHuman, nameof(formerHuman));
            Scribe_References.Look(ref relative, nameof(relative));
            Scribe_Defs.Look(ref relation, nameof(relation));
        }

        /// <summary>
        /// Whether the pawn is still eligable to join
        /// </summary>
        /// <returns><c>true</c>, if human still joinable was formered, <c>false</c> otherwise.</returns>
        private bool FormerHumanStillJoinable()
        {
            return !formerHuman.DestroyedOrNull()
                    && !formerHuman.Dead
                    && formerHuman.Faction != Faction.OfPlayer;
        }

        /// <summary>
        /// Gets the closeness of the relationship.
        /// </summary>
        /// <returns>The closeness.</returns>
        private RelationCloseness GetCloseness()
        {

            if (relation == PawnRelationDefOf.Child
                || relation == PawnRelationDefOf.Spouse
                || relation == PawnRelationDefOf.Fiance)
                return RelationCloseness.VeryClose;

            if (relation == PawnRelationDefOf.Parent
                || relation == PawnRelationDefOf.Sibling
                || relation == PawnRelationDefOf.Lover)
                return RelationCloseness.Close;

            if (relation == PawnRelationDefOf.Grandparent
                || relation == PawnRelationDefOf.Grandchild
                || relation == PawnRelationDefOf.UncleOrAunt
                || relation == PawnRelationDefOf.NephewOrNiece
                || relation == PawnRelationDefOf.Cousin
                || relation == PawnRelationDefOf.HalfSibling)
                return RelationCloseness.Moderate;

            return RelationCloseness.Distant;
        }

        // How close a relative is, used for thought severity
        private enum RelationCloseness
        {
            Distant,
            Moderate,
            Close,
            VeryClose
        }
    }
}
