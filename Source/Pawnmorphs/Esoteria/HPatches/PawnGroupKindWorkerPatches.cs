// PawnGroupKindWorkerPatches.cs created by Iron Wolf for Pawnmorph on 10/30/2019 1:00 PM
// last updated 10/30/2019  1:01 PM

using System;
using System.Collections.Generic;
using Harmony;
using Pawnmorph.Factions;
using RimWorld;
using Verse;

#pragma warning disable 01591
namespace Pawnmorph.HPatches
{
    public static class PawnGroupKindWorkerPatches
    {
        /*[HarmonyPatch(typeof(PawnGroupKindWorker))]
        [HarmonyPatch(nameof(PawnGroupKindWorker.GeneratePawns))] //might want to patch PawnGroupKindWorker_Normal,Trader instead 
        [HarmonyPatch(new Type[]
        {
            typeof(PawnGroupMakerParms), typeof(PawnGroupMaker), typeof(bool)
        })]*/ //disabled until later 
        public static class GeneratePawnsPatch
        {
            public static void Postfix(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults,
                                       ref List<Pawn> __result, PawnGroupKindWorker __instance)
            {
                if ((__result?.Count ?? 0) == 0) return;

                foreach (Pawn pawn in __result)
                {
                    MorphGroupMakerUtilities.ApplyMutationsPostLoad(pawn, false); 
                }
            }
        }


    }
}