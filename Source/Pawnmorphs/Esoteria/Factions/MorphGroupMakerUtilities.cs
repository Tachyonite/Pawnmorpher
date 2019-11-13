// MorphGroupMakerUtilities.cs created by Iron Wolf for Pawnmorph on 10/30/2019 11:18 AM
// last updated 10/30/2019  11:18 AM

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Factions
{
    /// <summary>
    ///     static container for applying mutations and morphs to pawns during generation
    /// </summary>
    public class MorphGroupMakerUtilities
    {
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

        /// <summary>
        ///     Applies the mutation extension to pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="canApplyRestricted">if set to <c>true</c> restricted mutations can be applied as well as regular ones.</param>
        /// <param name="setAtMaxStage">if set to <c>true</c>all mutations will be set at the maximum stage.</param>
        /// <param name="kindExtension">The kind extension.</param>
        public static void ApplyMutationExtensionToPawn(Pawn pawn, bool canApplyRestricted, bool setAtMaxStage,
                                                        MorphPawnKindExtension kindExtension)
        {
            ApplyMutations(pawn, canApplyRestricted, setAtMaxStage, kindExtension);
            ApplyAspects(pawn, kindExtension);
        }

        private static void ApplyAspects(Pawn pawn, MorphPawnKindExtension extension)
        {
            if (pawn.GetAspectTracker() == null) return;
            if (extension.aspects.Count == 0) return;

            int addAspect = extension.aspectRange.RandomInRange;
            if (addAspect == 0) return;

            var aspectTracker = pawn.GetAspectTracker();
            if (aspectTracker == null) return; 

            var availableAspects = extension.GetAllAspectDefs().ToList();

            addAspect = Mathf.Min(availableAspects.Count - 1, addAspect);

            if (addAspect == availableAspects.Count - 1)
            {
                foreach (AspectDef aspectDef in availableAspects)
                {
                    var stage = extension.GetAvailableStagesFor(aspectDef).RandomElementWithFallback();
                    aspectTracker.Add(aspectDef, stage); 
                }

                return;
            }


            for (var i = 0; i < addAspect; i++)
            {
                int r = Rand.Range(0, availableAspects.Count); //pick a random entry 
                var def = availableAspects[r];
                var stage = extension.GetAvailableStagesFor(def).RandomElementWithFallback();
                aspectTracker.Add(def, stage); 
                availableAspects.RemoveAt(r); //remove it so we don't pick it twice
            }
        }

        private static void AddAspectToPawn(Pawn pawn, MorphPawnKindExtension.AspectEntry aspectEntry)
        {
            var tracker = pawn.GetAspectTracker();
            tracker?.Add(aspectEntry.aspect, aspectEntry.stage); 
        }

        private static void ApplyMutations(Pawn pawn, bool canApplyRestricted, bool setAtMaxStage, MorphPawnKindExtension kindExtension)
        {
            List<HediffGiver_Mutation> givers;
            var addedPartsSet = new HashSet<BodyPartDef>();
            if (canApplyRestricted)
                givers = kindExtension.AllMutationGivers.ToList();
            else
                givers = kindExtension.AllMutationGivers.Where(g => !(g.hediff.GetModExtension<MutationHediffExtension>()
                                                                      ?.IsRestricted
                                                                   ?? false)) //only keep the unrestricted mutations 
                                      .ToList();


            var toGive = new List<HediffGiver_Mutation>();
            List<Hediff> allAdded = setAtMaxStage ? new List<Hediff>() : null;


            int toGiveCount = kindExtension.hediffRange.RandomInRange;


            int max = Mathf.Min(givers.Count, toGiveCount);

            for (var i = 0; i < max; i++)
            {
                if (givers.Count == 0) break;
                while (true)
                {
                    if (givers.Count == 0) break;
                    int rI = Rand.Range(0, givers.Count);
                    HediffGiver_Mutation mGiver = givers[rI];

                    givers.RemoveAt(rI); //remove the entry so we don't pull duplicates 
                    if (mGiver.GetPartsToAddTo().Any(p => p != null && addedPartsSet.Contains(p))
                    ) //make sure its for a part we haven't encountered yet
                        continue;

                    foreach (BodyPartDef part in mGiver.GetPartsToAddTo()) addedPartsSet.Add(part);
                    toGive.Add(mGiver);
                    break;
                }
            }

            foreach (HediffGiver_Mutation giver in toGive)
            {
                giver.ClearOverlappingHediffs(pawn); // make sure to remove any overlapping hediffs added during a different stage 

                giver.TryApply(pawn, MutagenDefOf.defaultMutagen, allAdded, addLogEntry:false);
            }

            if (setAtMaxStage)
                foreach (Hediff hediff in allAdded)
                {
                    if (hediff.pawn == null) continue; //sometimes the hediffs are removed by other mutations 
                    HediffStage lastStage = hediff.def.stages?.LastOrDefault();
                    if (lastStage == null) continue;
                    float severity = lastStage.minSeverity + 0.01f;
                    hediff.Severity = severity;
                }


            pawn.CheckRace(false); //don't apply missing mutations to avoid giving restricted mutations and to respect the limit
        }
    }
}