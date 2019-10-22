// Comp_MutagenicFixWorstCondition.cs modified by Iron Wolf for Pawnmorph on 10/02/2019 5:20 PM
// last updated 10/02/2019  5:20 PM

using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// comp that heals the worst condition, and adds a random mutation if the condition healed is either a missing body part or permanent injury 
    /// </summary>
    /// <seealso cref="RimWorld.CompUseEffect" />
    public class Comp_MutagenicFixWorstCondition : CompUseEffect
    {
        /// <summary>
        /// Gets the props.
        /// </summary>
        /// <value>
        /// The props.
        /// </value>
        /// <exception cref="InvalidCastException">unable to convert compProps {props.GetType().Name} to {nameof(CompProps_MutagenicFixWorstCondition)}</exception>
        public CompProps_MutagenicFixWorstCondition Props
        {
            get
            {
                try
                {
                    return (CompProps_MutagenicFixWorstCondition) props;
                }
                catch (InvalidCastException e)
                {
                    throw new
                        InvalidCastException($"unable to convert compProps {props.GetType().Name} to {nameof(CompProps_MutagenicFixWorstCondition)}",
                                             e);
                }
            }
        }

        private float HandCoverageAbsWithChildren =>
            ThingDefOf.Human.race.body.GetPartsWithDef(BodyPartDefOf.Hand).First().coverageAbsWithChildren;

        /// <summary>
        /// Does the effect.
        /// </summary>
        /// <param name="usedBy">the pawn that used this instance</param>
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            Hediff hediff = FindLifeThreateningHediff(usedBy);
            if (hediff != null)
            {
                Cure(hediff);
                return;
            }

            if (HealthUtility.TicksUntilDeathDueToBloodLoss(usedBy) < 2500)
            {
                Hediff hediff2 = FindMostBleedingHediff(usedBy);
                if (hediff2 != null)
                {
                    Cure(hediff2);
                    return;
                }
            }

            if (usedBy.health.hediffSet.GetBrain() != null)
            {
                Hediff_Injury hediff_Injury = FindPermanentInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
                if (hediff_Injury != null)
                {
                    Cure(hediff_Injury);
                    return;
                }
            }

            BodyPartRecord bodyPartRecord = FindBiggestMissingBodyPart(usedBy, HandCoverageAbsWithChildren);
            if (bodyPartRecord != null)
            {
                Cure(bodyPartRecord, usedBy);
                return;
            }

            Hediff_Injury hediff_Injury2 = FindPermanentInjury(usedBy, from x in usedBy.health.hediffSet.GetNotMissingParts()
                                                                       where x.def == BodyPartDefOf.Eye
                                                                       select x);
            if (hediff_Injury2 != null)
            {
                Cure(hediff_Injury2);

                if (hediff_Injury2.Part != null)
                    AddMutationToPart(hediff_Injury2.Part, usedBy, usedBy.GetMutationTracker()?.HighestInfluence); 


                return;
            }

            Hediff hediff3 = FindImmunizableHediffWhichCanKill(usedBy);
            if (hediff3 != null)
            {
                Cure(hediff3);
                return;
            }

            Hediff hediff4 = FindNonInjuryMiscBadHediff(usedBy, true);
            if (hediff4 != null)
            {
                Cure(hediff4);
                return;
            }

            Hediff hediff5 = FindNonInjuryMiscBadHediff(usedBy, false);
            if (hediff5 != null)
            {
                Cure(hediff5);
                return;
            }

            if (usedBy.health.hediffSet.GetBrain() != null)
            {
                Hediff_Injury hediff_Injury3 = FindInjury(usedBy, Gen.YieldSingle(usedBy.health.hediffSet.GetBrain()));
                if (hediff_Injury3 != null)
                {
                    Cure(hediff_Injury3);
                    return;
                }
            }

            BodyPartRecord bodyPartRecord2 = FindBiggestMissingBodyPart(usedBy);
            if (bodyPartRecord2 != null)
            {
                Cure(bodyPartRecord2, usedBy);
                return;
            }

            Hediff_Addiction hediff_Addiction = FindAddiction(usedBy);
            if (hediff_Addiction != null)
            {
                Cure(hediff_Addiction);
                return;
            }

            Hediff_Injury hediff_Injury4 = FindPermanentInjury(usedBy);
            if (hediff_Injury4 != null)
            {
                Cure(hediff_Injury4);
                return;
            }

            Hediff_Injury hediff_Injury5 = FindInjury(usedBy);
            if (hediff_Injury5 != null) Cure(hediff_Injury5);
        }

        /// <summary> Add mutations to the given part. </summary>
        private void AddMutationToPart(BodyPartRecord record, Pawn pawn, MorphDef morph = null, bool recursive = false)
        {
            List<HediffGiver_Mutation> allGivers = MorphUtilities.GetMutationGivers(record.def).ToList();

            if (allGivers.Count > 0)
            {
                HediffGiver_Mutation giver;
                if (morph != null)
                {
                    giver = allGivers.Where(g => morph.AllAssociatedAndAdjacentMutations.Contains(g)) // This will get all hediff givers that are on the same morph tf.
                                     .RandomElementWithFallback(); // i.e. wolf/husky/warg morphs can get paw hands.
                    giver = giver ?? allGivers.RandElement();
                }
                else
                {
                    giver = allGivers.RandElement();
                }

                giver.TryApply(pawn, MutagenDefOf.defaultMutagen);
            }

            if (recursive) // Recursively add mutations to child parts.
                foreach (BodyPartRecord cPart in record.GetDirectChildParts())
                    AddMutationToPart(cPart, pawn, morph, true);
        }

        private bool CanEverKill(Hediff hediff)
        {
            if (hediff.def.stages != null)
                foreach (HediffStage stage in hediff.def.stages)
                    if (stage.lifeThreatening)
                        return true;
            return hediff.def.lethalSeverity >= 0f;
        }

        private void Cure(Hediff hediff)
        {
            Pawn pawn = hediff.pawn;
            pawn.health.RemoveHediff(hediff);
            if (hediff.def.cureAllAtOnceIfCuredByItem)
            {
                var num = 0;
                while (true)
                {
                    num++;
                    if (num > 10000) break;
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def);
                    if (firstHediffOfDef == null) goto Block_3;
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }

                Log.Error("Too many iterations.");
                Block_3: ;
            }

            Messages.Message("MessageHediffCuredByItem".Translate(hediff.LabelBase.CapitalizeFirst()), pawn,
                             MessageTypeDefOf.PositiveEvent);
        }

        private void Cure(BodyPartRecord part, Pawn pawn)
        {
            pawn.health.RestorePart(part);
            // Add mutations.

            AddMutationToPart(part, pawn, pawn.GetMutationTracker()?.HighestInfluence, true);

            Messages.Message("MessageBodyPartCuredByItem".Translate(part.LabelCap), pawn, MessageTypeDefOf.PositiveEvent);
        }

        private Hediff_Addiction FindAddiction(Pawn pawn)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff in hediffs)
                if (hediff is Hediff_Addiction hediff_Addiction
                 && hediff_Addiction.Visible
                 && hediff_Addiction.def.everCurableByItem)
                    return hediff_Addiction;
            return null;
        }


        private BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (Hediff_MissingPart current in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
                if (current.Part.coverageAbsWithChildren >= minCoverage)
                    if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(current.Part))
                        if (bodyPartRecord == null
                         || current.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren)
                            bodyPartRecord = current.Part;
            return bodyPartRecord;
        }

        private Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff2 in hediffs)
                if (hediff2.Visible && hediff2.def.everCurableByItem)
                    if (hediff2.TryGetComp<HediffComp_Immunizable>() != null)
                        if (!hediff2.FullyImmune())
                            if (CanEverKill(hediff2))
                            {
                                float severity = hediff2.Severity;
                                if (hediff == null || severity > num)
                                {
                                    hediff = hediff2;
                                    num = severity;
                                }
                            }

            return hediff;
        }

        private Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediff_Injury = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff in hediffs)
                if (hediff is Hediff_Injury hediff_Injury2 && hediff_Injury2.Visible && hediff_Injury2.def.everCurableByItem)
                    if (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part))
                        if (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity)
                            hediff_Injury = hediff_Injury2;

            return hediff_Injury;
        }

        private Hediff FindLifeThreateningHediff(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff h in hediffs)
                if (h.Visible && h.def.everCurableByItem)
                    if (!h.FullyImmune())
                    {
                        HediffStage curStage = h.CurStage;
                        bool flag = curStage != null && curStage.lifeThreatening;
                        bool flag2 = h.def.lethalSeverity >= 0f
                                  && h.Severity / h.def.lethalSeverity >= 0.8f;
                        if (flag || flag2)
                        {
                            float num2 = h.Part?.coverageAbsWithChildren ?? 999f;
                            if (hediff == null || num2 > num)
                            {
                                hediff = h;
                                num = num2;
                            }
                        }
                    }

            return hediff;
        }

        private Hediff FindMostBleedingHediff(Pawn pawn)
        {
            var num = 0f;
            Hediff hediff = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (var i = 0; i < hediffs.Count; i++)
                if (hediffs[i].Visible && hediffs[i].def.everCurableByItem)
                {
                    float bleedRate = hediffs[i].BleedRate;
                    if (bleedRate > 0f && (bleedRate > num || hediff == null))
                    {
                        num = bleedRate;
                        hediff = hediffs[i];
                    }
                }

            return hediff;
        }

        private Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff2 in hediffs)
                if (hediff2.Visible && hediff2.def.isBad && hediff2.def.everCurableByItem)
                    if (!(hediff2 is Hediff_Injury)
                     && !(hediff2 is Hediff_MissingPart)
                     && !(hediff2 is Hediff_Addiction)
                     && !(hediff2 is Hediff_AddedPart)
                     && !(hediff2 is Hediff_AddedMutation))
                        if (!onlyIfCanKill || CanEverKill(hediff2))
                        {
                            float num2 = hediff2.Part?.coverageAbsWithChildren ?? 999f;
                            if (hediff == null || num2 > num)
                            {
                                hediff = hediff2;
                                num = num2;
                            }
                        }

            return hediff;
        }

        private Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediff_Injury = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (var i = 0; i < hediffs.Count; i++)
                if (hediffs[i] is Hediff_Injury hediff_Injury2
                 && hediff_Injury2.Visible
                 && hediff_Injury2.IsPermanent()
                 && hediff_Injury2.def.everCurableByItem)
                    if (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part))
                        if (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity)
                            hediff_Injury = hediff_Injury2;

            return hediff_Injury;
        }
    }
}