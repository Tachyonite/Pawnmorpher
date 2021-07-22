// PawnComponentPatches.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:00 PM
// last updated 11/27/2019  1:00 PM

using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Noise;

#pragma warning disable 1591

#if true
namespace Pawnmorph.HPatches
{
    public static class PawnComponentPatches
    {
        [HarmonyPatch(typeof(PawnComponentsUtility))]
        [HarmonyPatch("AddAndRemoveDynamicComponents")]
        public static class AddRemoveComponentsPatch
        {
            internal static void Postfix(Pawn pawn)
            {
                var sState = pawn.GetSapienceState(); 
                sState?.AddOrRemoveDynamicComponents();
            }


        }
    }
}
#endif