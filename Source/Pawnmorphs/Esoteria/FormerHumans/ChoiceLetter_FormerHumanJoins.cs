using System.Collections.Generic;
using Pawnmorph.Letters;
using RimWorld;
using Verse;

namespace Pawnmorph.FormerHumans
{
    /// <summary>
    /// A choice letter for a former human attempting to join the colony
    /// </summary>
    public class ChoiceLetter_FormerHumanJoins : ChoiceLetter
    {
        public const string LABEL = "PMLetterFormerHumanJoinLabel";
        public const string TITLE = "PMLetterFormerHumanJoinTitle";
        public const string TEXT = "PMLetterFormerHumanJoin";

        private Pawn formerHuman;
        private Pawn relative;
        private PawnRelationDef relation;

        /// <summary>
        /// Sends a join request for the given former human
        /// </summary>
        /// <param name="formerHuman">Former human.</param>
        /// <param name="relative">Relative.</param>
        /// <param name="relation">Relation.</param>
        public static void SendLetterFor(Pawn formerHuman, Pawn relative, PawnRelationDef relation)
        {
            var label = LABEL.Translate(formerHuman.Named("PAWN")).AdjustedFor(formerHuman, "PAWN");
            var text = TEXT.Translate(formerHuman.Named("PAWN"));

            ChoiceLetter_FormerHumanJoins letter = (ChoiceLetter_FormerHumanJoins)
                    LetterMaker.MakeLetter(label, text, PMLetterDefOf.PMFormerHumanJoinRequest, new LookTargets(formerHuman));

            letter.title = TITLE.Translate(formerHuman.Named("PAWN"));
            letter.radioMode = true;
            letter.formerHuman = formerHuman;
            letter.relative = relative;
            letter.relation = relation;
            letter.StartTimeout(60000);

            Find.LetterStack.ReceiveLetter(letter, null);
        }

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref formerHuman, nameof(formerHuman));
            Scribe_References.Look(ref relative, nameof(relative));
            Scribe_Defs.Look(ref relation, nameof(relation));
        }
    }
}
