// Comp_MutagenicFixWorstCondition.cs modified by Iron Wolf for Pawnmorph on 10/02/2019 5:20 PM
// last updated 10/02/2019  5:20 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.DefOfs;
using Pawnmorph.Hediffs;
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
					return (CompProps_MutagenicFixWorstCondition)props;
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
			ThingDefOf.Human.race.body.GetPartsWithDef(PM_BodyPartDefOf.Hand).First().coverageAbsWithChildren;

		/// <summary>
		/// Does the effect.
		/// </summary>
		/// <param name="usedBy">the pawn that used this instance</param>
		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			var mutagen = parent.def.GetModExtension<MutagenExtension>()?.mutagen;

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
																	   where x.def == PM_BodyPartDefOf.Eye
																	   select x);
			if (hediff_Injury2 != null)
			{
				Cure(hediff_Injury2);

				if (hediff_Injury2.Part != null)
					AddMutationToPart(hediff_Injury2.Part, usedBy, usedBy.GetMutationTracker()?.HighestInfluence, mutagen: mutagen);


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

			var tracker = usedBy.GetAspectTracker();
			var bAspect = FindBadAspectToRemove(usedBy);
			if (tracker != null && bAspect != null)
			{
				tracker.Remove(bAspect);
				ApplyMutagen(usedBy);
			}
		}

		private void ApplyMutagen([NotNull] Pawn pawn)
		{
			var mutagen = parent.def.GetModExtension<MutagenExtension>()?.mutagen;
			if (mutagen == null) return; //default mutagen has no aspects to add 
			mutagen.TryApplyAspects(pawn);
		}

		/// <summary> Add mutations to the given part. </summary>
		private void AddMutationToPart(BodyPartRecord record, [NotNull] Pawn pawn, AnimalClassBase aClass = null, bool recursive = false, MutagenDef mutagen = null)
		{
			MutationDef mutation;
			if (aClass != null)
				mutation = aClass?.GetAllMorphsInClass()
								  .SelectMany(m => m.GetMutationForPart(record.def))
								  .RandomElementWithFallback();
			else
				mutation = MutationUtilities.GetMutationsByPart(record.def).RandomElementWithFallback();

			if (mutation != null) MutationUtilities.AddMutation(pawn, mutation, record);

			mutagen?.TryApplyAspects(pawn);

			if (recursive) // Recursively add mutations to child parts.
				foreach (BodyPartRecord cPart in record.GetDirectChildParts())
					AddMutationToPart(cPart, pawn, aClass, true, mutagen);
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
			Block_3:;
			}

			Messages.Message("MessageHediffCuredByItem".Translate(hediff.LabelBase.CapitalizeFirst()), pawn,
							 MessageTypeDefOf.PositiveEvent);
		}

		private void Cure(BodyPartRecord part, Pawn pawn)
		{
			pawn.health.RestorePart(part);
			// Add mutations.

			AddMutationToPart(part, pawn, pawn.GetMutationTracker()?.HighestInfluence, true, parent.def.GetModExtension<MutagenExtension>()?.mutagen);

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


		[CanBeNull]
		Aspect FindBadAspectToRemove([NotNull] Pawn pawn)
		{
			var tracker = pawn.GetAspectTracker();
			if (tracker == null) return null;
			var bAspects = tracker.Aspects.Where(a => a.IsBad);
			Aspect retVal;
			if (!bAspects.TryRandomElement(out retVal)) return null; //just pick one randomly 
			return retVal;
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