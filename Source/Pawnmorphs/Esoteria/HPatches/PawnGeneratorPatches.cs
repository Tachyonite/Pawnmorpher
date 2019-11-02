// PawnGeneratorPatches.cs modified by Iron Wolf for Pawnmorph on 11/02/2019 10:07 AM
// last updated 11/02/2019  10:07 AM

using System;
using System.Linq;
using AlienRace;
using Harmony;
using Pawnmorph.Factions;
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
                
                var backstories = pawn.story?.AllBackstories ?? Enumerable.Empty<Backstory>();
                var extensions = backstories.Select(b => DefDatabase<BackstoryDef>.GetNamedSilentFail(b.identifier))
                                               .Where(bd => bd != null)
                                               .OrderBy(bd => bd.slot) //make sure the adult backstories overrides the child backstories 
                                               .Select(bd => bd.GetModExtension<MorphPawnKindExtension>())
                                               .Where(ext => ext != null);


                foreach (MorphPawnKindExtension extension in extensions)
                {
                    MorphGroupMakerUtilities.ApplyMutationExtensionToPawn(pawn, true, true, extension); //now apply all mutations in order of child -> adult 
                }



            }
        }

    }
}