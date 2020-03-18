// Worker_FormerHumanRecruitAttempt.cs modified by Iron Wolf for Pawnmorph on 12/22/2019 8:59 AM
// last updated 12/22/2019  8:59 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Social
{
    /// <summary>
    ///     interaction worker for recruiting former humans
    /// </summary>
    /// <seealso cref="RimWorld.InteractionWorker_RecruitAttempt" />
    public class Worker_FormerHumanRecruitAttempt : InteractionWorker
    { //TODO clean this up and keep only whats relevant for former humans
        private const float BaseResistanceReductionPerInteraction = 1f;
        private const float MaxMoodForWarning = 0.4f;
        private const float MaxOpinionForWarning = -0.01f;
        private const float WildmanWildness = 0.2f;
        private const float TameChanceFactor_Bonded = 4f;
        private const float ChanceToDevelopBondRelationOnTamed = 0.01f;
        private const int MenagerieTaleThreshold = 5;

        [NotNull] private static readonly List<RulePackDef> _scratchList = new List<RulePackDef>();

        private static readonly SimpleCurve ResistanceImpactFactorCurve_Mood = new SimpleCurve
        {
            new CurvePoint(0.0f, 0.2f),
            new CurvePoint(0.5f, 1f),
            new CurvePoint(1f, 1.5f)
        };

        private static readonly SimpleCurve ResistanceImpactFactorCurve_Opinion = new SimpleCurve
        {
            new CurvePoint(-100f, 0.5f),
            new CurvePoint(0.0f, 1f),
            new CurvePoint(100f, 1.5f)
        };

        private static readonly SimpleCurve TameChanceFactorCurve_Wildness = new SimpleCurve
        {
            new CurvePoint(1f, 0.0f),
            new CurvePoint(0.5f, 1f),
            new CurvePoint(0.0f, 2f)
        };

        /// <summary>
        /// performs the interaction between the initiator and recipient.
        /// </summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <param name="extraSentencePacks">The extra sentence packs.</param>
        /// <param name="letterText">The letter text.</param>
        /// <param name="letterLabel">The letter label.</param>
        /// <param name="letterDef">The letter definition.</param>
        /// <param name="lookTargets">The look targets.</param>
        public override void Interacted(
            Pawn initiator,
            Pawn recipient,
            List<RulePackDef> extraSentencePacks,
            out string letterText,
            out string letterLabel,
            out LetterDef letterDef,
            out LookTargets lookTargets)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
            bool flag1 = recipient.AnimalOrWildMan() && !recipient.IsPrisoner;
            float x1 = recipient.relations != null ? recipient.relations.OpinionOf(initiator) : 0.0f;
            bool flag2 = initiator.InspirationDef == InspirationDefOf.Inspired_Recruitment
                      && !flag1
                      && recipient.guest.interactionMode != PrisonerInteractionModeDefOf.ReduceResistance;
            if (DebugSettings.instantRecruit)
                recipient.guest.resistance = 0.0f;
            var resistanceReduce = 0.0f;
            if (!flag1 && recipient.guest.resistance > 0.0 && !flag2)
            {
                float num =
                    Mathf.Min(1f * initiator.GetStatValue(StatDefOf.NegotiationAbility) * ResistanceImpactFactorCurve_Mood.Evaluate(recipient.needs.mood == null ? 1f : recipient.needs.mood.CurInstantLevelPercentage) * ResistanceImpactFactorCurve_Opinion.Evaluate(x1),
                              recipient.guest.resistance);
                float resistance = recipient.guest.resistance;
                recipient.guest.resistance = Mathf.Max(0.0f, recipient.guest.resistance - num);
                resistanceReduce = resistance - recipient.guest.resistance;
                var text = (string) "TextMote_ResistanceReduced".Translate((NamedArgument) resistance.ToString("F1"),
                                                                           (NamedArgument) recipient
                                                                                          .guest.resistance.ToString("F1"));
                if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.400000005960464)
                    text = text + ("\n(" + "lowMood".Translate() + ")");
                if (recipient.relations != null && recipient.relations.OpinionOf(initiator) < -0.00999999977648258)
                    text = text + ("\n(" + "lowOpinion".Translate() + ")");
                MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
                if (recipient.guest.resistance == 0.0)
                {
                    TaggedString taggedString =
                        "MessagePrisonerResistanceBroken".Translate((NamedArgument) recipient.LabelShort,
                                                                    (NamedArgument) initiator.LabelShort,
                                                                    initiator.Named("WARDEN"), recipient.Named("PRISONER"));
                    if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
                        taggedString += " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
                    Messages.Message(taggedString, recipient, MessageTypeDefOf.PositiveEvent);
                }
            }
            else
            {
                float num;
                if (flag1)
                {
                    if (initiator.InspirationDef == InspirationDefOf.Inspired_Taming)
                    {
                        num = 1f;
                        initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Taming);
                    }
                    else
                    {
                        float statValue = initiator.GetStatValue(StatDefOf.TameAnimalChance);
                        float x2 = recipient.IsWildMan() ? 0.2f : recipient.RaceProps.wildness;
                        num = statValue * TameChanceFactorCurve_Wildness.Evaluate(x2);
                        if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
                            num *= 4f;
                    }
                }
                else
                {
                    num = flag2 || DebugSettings.instantRecruit ? 1f : recipient.RecruitChanceFinalByPawn(initiator);
                }

                if (Rand.Chance(num))
                {
                    if (!flag1)
                        recipient.guest.ClearLastRecruiterData();
                    DoRecruit(initiator, recipient, num, out letterLabel, out letterText, true,
                              false);
                    if (!letterLabel.NullOrEmpty())
                        letterDef = LetterDefOf.PositiveEvent;
                    lookTargets = new LookTargets((TargetInfo) (Thing) recipient, (TargetInfo) (Thing) initiator);
                    if (flag2)
                        initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Recruitment);
                    extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
                }
                else
                {
                    var text = (string) (flag1
                                             ? "TextMote_TameFail".Translate((NamedArgument) num.ToStringPercent())
                                             : "TextMote_RecruitFail".Translate((NamedArgument) num.ToStringPercent()));
                    if (!flag1)
                    {
                        if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.400000005960464)
                            text = text + ("\n(" + "lowMood".Translate() + ")");
                        if (recipient.relations != null && recipient.relations.OpinionOf(initiator) < -0.00999999977648258)
                            text = text + ("\n(" + "lowOpinion".Translate() + ")");
                    }

                    MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
                    extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
                }
            }

            if (flag1)

            {
                PostInteracted(initiator, recipient, extraSentencePacks);
                return;
            }

            PostInteracted(initiator, recipient, extraSentencePacks);
            recipient.guest.SetLastRecruiterData(initiator, resistanceReduce);
        }


        void PostInteracted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            if (extraSentencePacks == null) return;
            SapienceLevel? sapientLevel = recipient?.GetQuantizedSapienceLevel();
            if (sapientLevel == null) return;
            RulePackDef variant;
            if (extraSentencePacks.Count != 0)
            {
                _scratchList.Clear();
                _scratchList.AddRange(extraSentencePacks);
                extraSentencePacks.Clear(); //we need to substitute any variants if any exist 

                foreach (RulePackDef rulePackDef in _scratchList)
                    if (rulePackDef.TryGetSapientDefVariant(recipient, out variant))
                        extraSentencePacks.Add(variant); //add the variant if a variant is found 
                    else
                        extraSentencePacks.Add(rulePackDef); //add the original if no variant is found 
            }

            //now get additions from the interaction def 
            if (interaction.TryGetSapientDefVariant(recipient, out variant)) extraSentencePacks.Add(variant);

            if (recipient.Faction == Faction.OfPlayer) recipient.TryGainMemory(PMThoughtDefOf.FormerHumanTameThought);
        }

        /// <summary>
        ///     gets the selection weight
        /// </summary>
        /// <param name="initiator"></param>
        /// <param name="recipient"></param>
        /// <returns></returns>
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (!interaction.IsValidFor(recipient)) return 0;

            return base.RandomSelectionWeight(initiator, recipient);
        }

        /// <summary>
        /// recruits the sapient animal
        /// </summary>
        /// <param name="recruiter">The recruiter.</param>
        /// <param name="recruitee">The recruitee.</param>
        /// <param name="recruitChance">The recruit chance.</param>
        /// <param name="letterLabel">The letter label.</param>
        /// <param name="letter">The letter.</param>
        /// <param name="useAudiovisualEffects">if set to <c>true</c> [use audiovisual effects].</param>
        /// <param name="sendLetter">if set to <c>true</c> [send letter].</param>
        public static void DoRecruit(
            Pawn recruiter,
            Pawn recruitee,
            float recruitChance,
            out string letterLabel,
            out string letter,
            bool useAudiovisualEffects = true,
            bool sendLetter = true)
        {
            letterLabel = null;
            letter = null;
            recruitChance = Mathf.Clamp01(recruitChance);
            string str = recruitee.LabelIndefinite();
            if (recruitee.guest != null)
                recruitee.guest.SetGuestStatus(null);
            bool flag = recruitee.Name != null;
            if (recruitee.Faction != recruiter.Faction)
                recruitee.SetFaction(recruiter.Faction, recruiter);
            if (recruitee.IsHumanlike())
            {
                if (useAudiovisualEffects)
                {
                    letterLabel = "LetterLabelMessageRecruitSuccess".Translate() + ": " + recruitee.LabelShortCap;
                    if (sendLetter)
                        Find.LetterStack.ReceiveLetter((TaggedString) letterLabel,
                                                       "MessageRecruitSuccess".Translate((NamedArgument) recruiter,
                                                                                         (NamedArgument) recruitee,
                                                                                         (NamedArgument) recruitChance
                                                                                            .ToStringPercent(),
                                                                                         recruiter.Named("RECRUITER"),
                                                                                         recruitee.Named("RECRUITEE")),
                                                       LetterDefOf.PositiveEvent, recruitee);
                }

                TaleRecorder.RecordTale(RimWorld.TaleDefOf.Recruited, (object) recruiter, (object) recruitee);
                recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
                if (recruitee.needs.mood != null)
                    recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
                QuestUtility.SendQuestTargetSignals(recruitee.questTags, "Recruited", recruitee.Named("SUBJECT"));
            }
            else
            {
                if (useAudiovisualEffects)
                {
                    if (!flag)
                        Messages.Message("MessageTameAndNameSuccess".Translate((NamedArgument) recruiter.LabelShort, (NamedArgument) str, (NamedArgument) recruitChance.ToStringPercent(), (NamedArgument) recruitee.Name.ToStringFull, recruiter.Named("RECRUITER"), recruitee.Named("RECRUITEE")).AdjustedFor(recruitee),
                                         recruitee, MessageTypeDefOf.PositiveEvent);
                    else
                        Messages.Message("MessageTameSuccess".Translate((NamedArgument) recruiter.LabelShort, (NamedArgument) str, (NamedArgument) recruitChance.ToStringPercent(), recruiter.Named("RECRUITER")),
                                         recruitee, MessageTypeDefOf.PositiveEvent);
                    if (recruiter.Spawned && recruitee.Spawned)
                        MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map,
                                            "TextMote_TameSuccess".Translate((NamedArgument) recruitChance.ToStringPercent()),
                                            8f);
                }

                recruiter.records.Increment(RecordDefOf.AnimalsTamed);
                RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
                if (Rand.Chance(Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness)) || recruitee.IsWildMan())
                    TaleRecorder.RecordTale(RimWorld.TaleDefOf.TamedAnimal, (object) recruiter, (object) recruitee);
                if (PawnsFinder.AllMapsWorldAndTemporary_Alive.Count(p =>
                    {
                        if (p.playerSettings != null)
                            return p.playerSettings.Master == recruiter;
                        return false;
                    })
                 >= 5)
                    TaleRecorder.RecordTale(RimWorld.TaleDefOf.IncreasedMenagerie, (object) recruiter, (object) recruitee);
            }

            if (recruitee.caller == null)
                return;
            recruitee.caller.DoCall();
        }


        /// <summary>
        /// recruits the sapient animal
        /// </summary>
        /// <param name="recruiter">The recruiter.</param>
        /// <param name="recruitee">The recruitee.</param>
        /// <param name="recruitChance">The recruit chance.</param>
        /// <param name="useAudiovisualEffects">if set to <c>true</c> [use audiovisual effects].</param>
        public static void DoRecruit(
            Pawn recruiter,
            Pawn recruitee,
            float recruitChance,
            bool useAudiovisualEffects = true)
        {
            string letterLabel;
            string letter;
            DoRecruit(recruiter, recruitee, recruitChance, out letterLabel, out letter, useAudiovisualEffects);
        }
    }
}