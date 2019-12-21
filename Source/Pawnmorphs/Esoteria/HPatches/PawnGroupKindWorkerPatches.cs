// PawnGroupKindWorkerPatches.cs created by Iron Wolf for Pawnmorph on 10/30/2019 1:00 PM
// last updated 10/30/2019  1:01 PM

using System.Collections.Generic;
using System.Linq;
using AlienRace;
using Harmony;
using Pawnmorph.Factions;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

#pragma warning disable 1591

namespace Pawnmorph.HPatches
{
    public static class PawnGroupKindWorkerPatches
    {
        [HarmonyPatch(typeof(PawnGroupKindWorker))]
        [HarmonyPatch(nameof(PawnGroupKindWorker.GeneratePawns))] //might want to patch PawnGroupKindWorker_Normal,Trader instead 
        [HarmonyPatch(new[]
        {
            typeof(PawnGroupMakerParms), typeof(PawnGroupMaker), typeof(bool)
        })]
        public static class GeneratePawnsPatch
        {
            public static void Postfix(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults,
                                       ref List<Pawn> __result, PawnGroupKindWorker __instance)
            {
                if ((__result?.Count ?? 0) == 0) return;


                foreach (Pawn pawn in __result)
                {
                    IEnumerable<BackstoryDef> backstories = (pawn.story?.AllBackstories)
                                                           .MakeSafe() //probably want to make this without linq for performance reasons? 
                                                           .Select(b => DefDatabase<BackstoryDef>.GetNamed(b.identifier,
                                                                                                           false))
                                                           .Where(b => b
                                                                    != null); //only alien race's backstories can add mutations 

                    if (backstories.Any(b => b.GetModExtension<MorphPawnKindExtension>() != null))
                        continue; //check if they have any backstories that apply mutations, if so don't add more 

                    MorphGroupMakerUtilities.ApplyMutationsPostLoad(pawn, false);
                }
            }
        }
    }
}