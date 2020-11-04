// PawnGeneratorPatches.cs modified by Iron Wolf for Pawnmorph on 11/02/2019 10:07 AM
// last updated 11/02/2019  10:07 AM

using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Factions;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;
using Verse.Noise;

#pragma warning disable 1591
namespace Pawnmorph.HPatches
{
    /// <summary>
    /// static class for containing HPatches to PawnGenerator class
    /// </summary>
    public static class PawnGeneratorPatches
    {

        static void HandleAlienRaceExtensions([NotNull] Pawn pawn, [NotNull] RaceMutationSettingsExtension ext)
        {
            var retrievers = ext.mutationRetrievers;
            if (retrievers == null || retrievers.Count == 0 || pawn.def == null) return;

            foreach (MutationDef mutationDef in retrievers.GetMutationsFor(pawn.def, pawn))
            {
                var mutations = MutationUtilities.AddMutation(pawn, mutationDef, ancillaryEffects:MutationUtilities.AncillaryMutationEffects.None);


                foreach (Hediff_AddedMutation mutationAdded in mutations)
                {
                    var adjComp = mutationAdded.SeverityAdjust;
                    if (adjComp != null) mutationAdded.Severity = adjComp.NaturalSeverityLimit; 
                }
            }
        }

        [HarmonyPatch(typeof(PawnGenerator))]
        [HarmonyPatch("GenerateInitialHediffs")] //might want to patch PawnGroupKindWorker_Normal,Trader instead 
        [HarmonyPatch(new Type[]
        {
            typeof(Pawn), typeof(PawnGenerationRequest)
        })]
        public static class InitialHediffsPatch
        {
            public static void Postfix(Pawn pawn, PawnGenerationRequest request)
            {

                var raceExt = pawn?.def?.GetModExtension<RaceMutationSettingsExtension>();
                if (raceExt?.immuneToAll == true) return;
                if (raceExt != null)
                {
                    HandleAlienRaceExtensions(pawn, raceExt);
                }


                var backstories = pawn.story?.AllBackstories ?? Enumerable.Empty<Backstory>();
                var extensions = backstories.Select(b => DefDatabase<BackstoryDef>.GetNamedSilentFail(b.identifier))
                                               .Where(bd => bd != null)
                                               .OrderBy(bd => bd.slot) //make sure the adult backstories overrides the child backstories 
                                               .Select(bd => bd.GetModExtension<MorphPawnKindExtension>())
                                               .Where(ext => ext != null);

                bool anyAdded = false; 
                foreach (MorphPawnKindExtension extension in extensions)
                {
                    anyAdded = true; 
                    MorphGroupMakerUtilities.ApplyMutationExtensionToPawn(pawn, true, true, extension); //now apply all mutations in order of child -> adult 
                }

                if (!anyAdded)
                {
                    var kindExtension = pawn.kindDef.GetModExtension<MorphPawnKindExtension>();
                    if (kindExtension != null)
                    {
                        MorphGroupMakerUtilities.ApplyMutationExtensionToPawn(pawn, false, true, kindExtension); 
                    }
                }

            }
        }

    }
}