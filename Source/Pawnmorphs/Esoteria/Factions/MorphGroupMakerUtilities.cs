// MorphGroupMakerUtilities.cs created by Iron Wolf for Pawnmorph on 10/30/2019 11:18 AM
// last updated 10/30/2019  11:18 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.Factions
{
	/// <summary>
	///     static container for applying mutations and morphs to pawns during generation
	/// </summary>
	public class MorphGroupMakerUtilities
	{
		/// <summary>
		///     Applies the mutation extension to pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="canApplyRestricted">if set to <c>true</c> restricted mutations can be applied as well as regular ones.</param>
		/// <param name="setAtMaxStage">if set to <c>true</c>all mutations will be set at the maximum stage.</param>
		/// <param name="kindExtension">The kind extension.</param>
		public static void ApplyMutationExtensionToPawn([NotNull] Pawn pawn, bool canApplyRestricted, bool setAtMaxStage,
														[NotNull] MorphPawnKindExtension kindExtension)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (kindExtension == null) throw new ArgumentNullException(nameof(kindExtension));
			ApplyMutations(pawn, canApplyRestricted, setAtMaxStage, kindExtension);
			ApplyAspects(pawn, kindExtension);
		}

		/// <summary>Applies the mutations after the pawn has been loaded by the game.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="canApplyRestricted">if set to <c>true</c> allow restricted mutations to be applied.</param>
		/// <param name="setAtMaxStage">if true, the hediffs added will be set to the maximum stage (familiar usually)</param>
		public static void ApplyMutationsPostLoad(Pawn pawn, bool canApplyRestricted, bool setAtMaxStage = true)
		{
			var kindExtension = pawn.kindDef.GetModExtension<MorphPawnKindExtension>();
			if (kindExtension == null) return;

			ApplyMutationExtensionToPawn(pawn, canApplyRestricted, setAtMaxStage, kindExtension);
		}

		private static void ApplyAspects(Pawn pawn, MorphPawnKindExtension extension)
		{
			if (pawn.GetAspectTracker() == null) return;
			if (extension.aspects.Count == 0) return;

			int addAspect = extension.aspectRange.RandomInRange;
			if (addAspect == 0) return;

			AspectTracker aspectTracker = pawn.GetAspectTracker();
			if (aspectTracker == null) return;

			List<AspectDef> availableAspects = extension.GetAllAspectDefs().ToList();

			addAspect = Mathf.Min(availableAspects.Count - 1, addAspect);

			if (addAspect == availableAspects.Count - 1)
			{
				foreach (AspectDef aspectDef in availableAspects)
				{
					int stage = extension.GetAvailableStagesFor(aspectDef).RandomElementWithFallback();
					aspectTracker.Add(aspectDef, stage);
				}

				return;
			}


			for (var i = 0; i < addAspect; i++)
			{
				int r = Rand.Range(0, availableAspects.Count); //pick a random entry 
				AspectDef def = availableAspects[r];
				int stage = extension.GetAvailableStagesFor(def).RandomElementWithFallback();
				aspectTracker.Add(def, stage);
				availableAspects.RemoveAt(r); //remove it so we don't pick it twice
			}
		}

		private static void ApplyMutations([NotNull] Pawn pawn, bool canApplyRestricted, bool setAtMaxStage,
										   [NotNull] MorphPawnKindExtension kindExtension)
		{
			List<MutationDef> mutations;
			var addedPartsSet = new HashSet<BodyPartDef>();
			if (!canApplyRestricted) canApplyRestricted = pawn.CanReceiveRareMutations();


			if (canApplyRestricted)
				mutations = kindExtension.GetRandomMutations(pawn.thingIDNumber).ToList();
			else
				mutations = kindExtension.GetRandomMutations(pawn.thingIDNumber)
										 .Where(g => !g.IsRestricted) //only keep the unrestricted mutations 
										 .ToList();

			if (mutations.Count == 0)
			{
				Warning($"could not get any mutations for {pawn.Name} using extension\n{kindExtension.ToStringFull()}");
			}

			var toGive = new List<MutationDef>();
			var addedList = new List<BodyPartRecord>();

			int toGiveCount = kindExtension.hediffRange.RandomInRange; //get a random number of mutations to add
			int max = Mathf.Min(mutations.Count, toGiveCount);
			var i = 0;
			while (i < max)
			{
				if (mutations.Count == 0) break;
				while (true)
				{
					if (mutations.Count == 0) break;
					int rI = Rand.Range(0, mutations.Count);
					MutationDef mGiver = mutations[rI];

					mutations.RemoveAt(rI); //remove the entry so we don't pull duplicates 
					if (mGiver.parts.Any(p => p != null && addedPartsSet.Contains(p))
					) //make sure its for a part we haven't encountered yet
						continue;

					foreach (BodyPartDef part in mGiver.parts) addedPartsSet.Add(part);
					toGive.Add(mGiver);
					i++; //only count 1 regardless of what was added 
					break;
				}
			}


			foreach (MutationDef giver in toGive)
			{
				giver.ClearOverlappingMutations(pawn); // make sure to remove any overlapping hediffs added during a different stage 

				var result = MutationUtilities.AddMutation(pawn, giver, int.MaxValue, MutationUtilities.AncillaryMutationEffects.None);
				addedList.AddRange(result.Parts);
			}

			if (toGive.Count > 0 && (addedList.Count == 0 || !pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>().Any()))
			{
				LogMsg(LogLevel.Warnings, $"could not add mutations to pawn {pawn.Name} with ext\n{kindExtension}");
			}

			if (setAtMaxStage)
			{
				var addedMutations = new List<Hediff_AddedMutation>();
				List<Hediff_AddedMutation> allMutationsOnPawn =
					pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>().ToList(); //save these 
				foreach (BodyPartRecord bodyPartRecord in addedList) //get a list of all mutations we just added 
					foreach (Hediff_AddedMutation mutation in allMutationsOnPawn)
						if (mutation.Part == bodyPartRecord && toGive.Contains(mutation.def as MutationDef))
							if (!addedMutations.Contains(mutation))
								addedMutations.Add(mutation);


				foreach (Hediff_AddedMutation hediff in addedMutations)
				{
					if (hediff.pawn == null) continue; //sometimes the hediffs are removed by other mutations 

					if (hediff.TryGetComp<HediffComp_Production>() != null) continue; //do not increase production hediffs 

					var comp = hediff.TryGetComp<Comp_MutationSeverityAdjust>();
					if (comp != null)
					{
						hediff.Severity = comp.NaturalSeverityLimit;
						continue;
					}

					HediffStage lastStage = hediff.def.stages?.LastOrDefault();
					if (lastStage == null) continue;

					float severity = lastStage.minSeverity + 0.01f;
					hediff.Severity = severity;
				}
			}


			pawn.CheckRace(false); //don't apply missing mutations to avoid giving restricted mutations and to respect the limit
		}
	}
}