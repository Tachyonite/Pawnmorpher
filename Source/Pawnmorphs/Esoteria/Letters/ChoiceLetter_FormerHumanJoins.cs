using System.Collections.Generic;
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
            var label = labelId.Translate(formerHuman).AdjustedFor(formerHuman, "formerHuman");
            var text = textId.Translate(formerHuman);

            QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(formerHuman), ref text);
            TaggedString _;
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref _, formerHuman);


            ChoiceLetter_FormerHumanJoins letter = (ChoiceLetter_FormerHumanJoins)
                    LetterMaker.MakeLetter(label, text, PMLetterDefOf.PMFormerHumanJoinRequest, new LookTargets(formerHuman));

            letter.title = titleId.Translate(formerHuman);
            letter.radioMode = true;
            letter.formerHuman = formerHuman;
            letter.relative = relative;
            letter.relation = relation;
            letter.StartTimeout(60000);

            Find.LetterStack.ReceiveLetter(letter, null);
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
                    // TODO translate
                    DiaOption diaOption = new DiaOption("RansomDemand_Accept".Translate()) 
                    {
                        action = delegate
                        {
                            formerHuman.SetFaction(Faction.OfPlayer);
                            CameraJumper.TryJump(formerHuman.Position, formerHuman.Map);
                            Find.LetterStack.RemoveLetter(this);
                        },
                        resolveTree = true
                    };
                    yield return diaOption;
                    yield return Option_Reject;
                    yield return Option_Postpone;
                }
            }
        }

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
                return !formerHuman.DestroyedOrNull()
                    && !formerHuman.Dead
                    && formerHuman.Faction != Faction.OfPlayer;
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
    }
}
